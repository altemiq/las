// -----------------------------------------------------------------------
// <copyright file="PointDataRecordWriter{T}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed <see cref="Writers.PointDataRecordWriter{T}"/> for <see cref="PointDataRecord"/> intances.
/// </summary>
/// <typeparam name="T">The type of point data record.</typeparam>
internal abstract class PointDataRecordWriter<T> : IPointDataRecordWriter, ISimpleWriter
    where T : IBasePointDataRecord
{
    private readonly IEntropyEncoder encoder;
    private readonly ISymbolModel changedValuesModel;
    private readonly IntegerCompressor intensityIntegerCompressor;
    private readonly ISymbolModel[] scanAngleRankModels = new ISymbolModel[2];
    private readonly IntegerCompressor pointSourceIdIntegerCompressor;
    private readonly ISymbolModel?[] bitByteModels = new ISymbolModel[ArithmeticCoder.ModelCount];
    private readonly ISymbolModel?[] classificationModels = new ISymbolModel[ArithmeticCoder.ModelCount];
    private readonly ISymbolModel?[] userDataModels = new ISymbolModel[ArithmeticCoder.ModelCount];
    private readonly IntegerCompressor deltaXIntegerCompressor;
    private readonly IntegerCompressor deltaYIntegerCompressor;
    private readonly IntegerCompressor zIntegerCompressor;
    private readonly byte[] lastItem = new byte[PointDataRecord.Size];
    private readonly ushort[] lastIntensity = new ushort[16];
    private readonly StreamingMedian5[] lastXDiffMedian5 = new StreamingMedian5[16];
    private readonly StreamingMedian5[] lastYDiffMedian5 = new StreamingMedian5[16];
    private readonly int[] lastHeight = new int[8];

    /// <summary>
    /// Initializes a new instance of the <see cref="PointDataRecordWriter{T}"/> class.
    /// </summary>
    /// <param name="encoder">The encoder.</param>
    protected PointDataRecordWriter(IEntropyEncoder encoder)
    {
        this.encoder = encoder;

        // create models and integer compressors
        this.changedValuesModel = this.encoder.CreateSymbolModel(64);
        this.intensityIntegerCompressor = new(encoder, 16, 4);
        this.scanAngleRankModels[0] = encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        this.scanAngleRankModels[1] = encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        this.pointSourceIdIntegerCompressor = new(encoder);
        this.deltaXIntegerCompressor = new(encoder, 32, 2);  // 32 bits, 2 context
        this.deltaYIntegerCompressor = new(encoder, 32, 22); // 32 bits, 22 contexts
        this.zIntegerCompressor = new(encoder, 32, 20);  // 32 bits, 20 contexts
    }

    /// <inheritdoc/>
    public int Write(Span<byte> destination, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes) => record is T value ? this.Write(destination, value, extraBytes) : throw new InvalidOperationException();

    /// <inheritdoc cref="IPointDataRecordWriter.Write" />
    public int Write(Span<byte> destination, [System.Diagnostics.CodeAnalysis.NotNull] T record, ReadOnlySpan<byte> extraBytes)
    {
        var bytesWritten = record.CopyTo(destination);
        extraBytes.CopyTo(destination[bytesWritten..]);
        bytesWritten += extraBytes.Length;
        this.Write(destination[..bytesWritten]);
        return 0;
    }

    /// <inheritdoc/>
    public virtual void Write(Span<byte> item) => this.WriteCore(item);

    /// <inheritdoc/>
    public ValueTask<int> WriteAsync(Memory<byte> destination, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default) => record is T value ? this.WriteAsync(destination, value, extraBytes, cancellationToken) : throw new InvalidOperationException();

    /// <inheritdoc cref="IPointDataRecordWriter.WriteAsync"/>
    public async ValueTask<int> WriteAsync(Memory<byte> destination, [System.Diagnostics.CodeAnalysis.NotNull] T record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default)
    {
        var bytesWritten = record.CopyTo(destination.Span);
        extraBytes.CopyTo(destination[bytesWritten..]);
        bytesWritten += extraBytes.Length;
        await this.WriteAsync(destination[..bytesWritten], cancellationToken).ConfigureAwait(false);
        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default)
    {
        this.WriteCore(item.Span);
        return default;
    }

    /// <inheritdoc/>
    public virtual bool Initialize(ReadOnlySpan<byte> item)
    {
        // init state
        for (var i = 0; i < 16; i++)
        {
            this.lastXDiffMedian5[i] = new();
            this.lastYDiffMedian5[i] = new();
            this.lastIntensity[i] = default;
            this.lastHeight[i / 2] = default;
        }

        // init models and integer compressors
        _ = this.changedValuesModel.Initialize();
        this.intensityIntegerCompressor.Initialize();
        _ = this.scanAngleRankModels[0].Initialize();
        _ = this.scanAngleRankModels[1].Initialize();
        this.pointSourceIdIntegerCompressor.Initialize();
        for (var i = 0; i < ArithmeticCoder.ModelCount; i++)
        {
            _ = this.bitByteModels[i]?.Initialize();
            _ = this.classificationModels[i]?.Initialize();
            _ = this.userDataModels[i]?.Initialize();
        }

        this.deltaXIntegerCompressor.Initialize();
        this.deltaYIntegerCompressor.Initialize();
        this.zIntegerCompressor.Initialize();

        // init last item
        item[..PointDataRecord.Size].CopyTo(this.lastItem);

        // but set intensity to zero
        this.lastItem[12] = default;
        this.lastItem[13] = default;

        return true;
    }

    private void WriteCore(ReadOnlySpan<byte> item)
    {
        uint returnNumber = FieldAccessors.PointDataRecord.GetReturnNumber(item);
        uint numberOfReturns = FieldAccessors.PointDataRecord.GetNumberOfReturns(item);
        uint m = Common.NumberReturnMap[numberOfReturns][returnNumber];
        uint l = Common.NumberReturnLevel[numberOfReturns][returnNumber];

        // compress which other values have changed
        var changedValues = ((this.lastItem[14] != item[14] ? 1 : 0) << 5) // bit_byte
            | ((this.lastIntensity[m] != FieldAccessors.PointDataRecord.GetIntensity(item) ? 1 : 0) << 4)
            | ((this.lastItem[15] != item[15] ? 1 : 0) << 3) // classification
            | ((this.lastItem[16] != item[16] ? 1 : 0) << 2) // scan_angle_rank
            | ((this.lastItem[17] != item[17] ? 1 : 0) << 1) // user_data
            | (FieldAccessors.PointDataRecord.GetPointSourceId(this.lastItem) != FieldAccessors.PointDataRecord.GetPointSourceId(item) ? 1 : 0);

        this.encoder.EncodeSymbol(this.changedValuesModel, (uint)changedValues);

        // compress the bit_byte (edge_of_flight_line, scan_direction_flag, returns, ...) if it has changed
        if ((changedValues & 32) is not 0)
        {
            var model = this.bitByteModels[this.lastItem[14]];
            if (model is null)
            {
                model = this.encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                this.bitByteModels[this.lastItem[14]] = model;
                _ = model.Initialize();
            }

            this.encoder.EncodeSymbol(model, item[14]);
        }

        // compress the intensity if it has changed
        if ((changedValues & 16) is not 0)
        {
            var intensity = FieldAccessors.PointDataRecord.GetIntensity(item);
            this.intensityIntegerCompressor.Compress(this.lastIntensity[m], intensity, m < 3 ? m : 3);
            this.lastIntensity[m] = intensity;
        }

        // compress the classification ... if it has changed
        if ((changedValues & 8) is not 0)
        {
            var model = this.classificationModels[this.lastItem[15]];
            if (model is null)
            {
                model = this.encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                this.classificationModels[this.lastItem[15]] = model;
                _ = model.Initialize();
            }

            this.encoder.EncodeSymbol(model, item[15]);
        }

        // compress the scan_angle_rank ... if it has changed
        if ((changedValues & 4) is not 0)
        {
            this.encoder.EncodeSymbol(this.scanAngleRankModels[FieldAccessors.PointDataRecord.GetScanDirectionFlag(item) ? 1 : 0], (item[16] - this.lastItem[16]).Fold());
        }

        // compress the user_data ... if it has changed
        if ((changedValues & 2) is not 0)
        {
            var model = this.userDataModels[this.lastItem[17]];
            if (model is null)
            {
                model = this.encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                this.userDataModels[this.lastItem[17]] = model;
                _ = model.Initialize();
            }

            this.encoder.EncodeSymbol(model, item[17]);
        }

        // compress the point_source_ID ... if it has changed
        if ((changedValues & 1) is not 0)
        {
            this.pointSourceIdIntegerCompressor.Compress(FieldAccessors.PointDataRecord.GetPointSourceId(this.lastItem), FieldAccessors.PointDataRecord.GetPointSourceId(item));
        }

        // compress x coordinate
        var median = this.lastXDiffMedian5[m].Get();
        var diff = FieldAccessors.PointDataRecord.GetX(item) - FieldAccessors.PointDataRecord.GetX(this.lastItem);
        this.deltaXIntegerCompressor.Compress(median, diff, numberOfReturns is 1 ? 1U : 0U);
        this.lastXDiffMedian5[m].Add(diff);

        // compress y coordinate
        var kBits = this.deltaXIntegerCompressor.K;
        median = this.lastYDiffMedian5[m].Get();
        diff = FieldAccessors.PointDataRecord.GetY(item) - FieldAccessors.PointDataRecord.GetY(this.lastItem);
        this.deltaYIntegerCompressor.Compress(median, diff, (numberOfReturns is 1 ? 1U : 0U) + (kBits < 20 ? kBits.ZeroBit0() : 20U));
        this.lastYDiffMedian5[m].Add(diff);

        // compress z coordinate
        kBits = (this.deltaXIntegerCompressor.K + this.deltaYIntegerCompressor.K) / 2;
        var z = FieldAccessors.PointDataRecord.GetZ(item);
        this.zIntegerCompressor.Compress(this.lastHeight[l], z, (numberOfReturns is 1 ? 1U : 0U) + (kBits < 18 ? kBits.ZeroBit0() : 18U));
        this.lastHeight[l] = z;

        // copy the last item
        item[..PointDataRecord.Size].CopyTo(this.lastItem);
    }
}