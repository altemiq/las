// -----------------------------------------------------------------------
// <copyright file="PointDataRecordReader{T}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.PointDataRecordReader{T}"/> for <see cref="PointDataRecord"/> intances.
/// </summary>
/// <typeparam name="T">The type of point data record.</typeparam>
/// <param name="decoder">The decoder.</param>
/// <param name="pointDataLength">The point data length.</param>
/// <param name="basePointDataLength">The base point data length, without extra bytes.</param>
internal abstract class PointDataRecordReader<T>(IEntropyDecoder decoder, int pointDataLength, int basePointDataLength) : IPointDataRecordReader, ISimple
    where T : IBasePointDataRecord
{
    private readonly ISymbolModel changedValuesModel = decoder.CreateSymbolModel(64);
    private readonly IntegerDecompressor intensityIntegerDecompressor = new(decoder, 16, 4);
    private readonly ISymbolModel scanAngleRankModels0 = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
    private readonly ISymbolModel scanAngleRankModels1 = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
    private readonly IntegerDecompressor pointSourceIdIntegerDecompressor = new(decoder);
    private readonly ISymbolModel?[] bitByteModels = new ISymbolModel[ArithmeticCoder.ModelCount];
    private readonly ISymbolModel?[] classificationModels = new ISymbolModel[ArithmeticCoder.ModelCount];
    private readonly ISymbolModel?[] userDataModels = new ISymbolModel[ArithmeticCoder.ModelCount];
    private readonly IntegerDecompressor deltaXIntegerDecompressor = new(decoder, 32, 2);
    private readonly IntegerDecompressor deltaYIntegerDecompressor = new(decoder, 32, 22);
    private readonly IntegerDecompressor zIntegerDecompressor = new(decoder, 32, 20);
    private readonly byte[] lastPoint = new byte[PointDataRecord.Size];
    private readonly ushort[] lastIntensity = new ushort[16];
    private readonly StreamingMedian5[] lastXDiffMedian5 = new StreamingMedian5[16];
    private readonly StreamingMedian5[] lastYDiffMedian5 = new StreamingMedian5[16];
    private readonly int[] lastHeight = new int[8];

    private readonly byte[] data = new byte[pointDataLength];

    /// <summary>
    /// Initializes the reader with the specified data.
    /// </summary>
    /// <param name="item">The item to initialize from.</param>
    /// <returns><see langword="true"/> if successfully initialized; otherwise <see langword="false"/>.</returns>
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
        this.intensityIntegerDecompressor.Initialize();
        _ = this.scanAngleRankModels0.Initialize();
        _ = this.scanAngleRankModels1.Initialize();
        this.pointSourceIdIntegerDecompressor.Initialize();
        for (var i = 0; i < ArithmeticCoder.ModelCount; i++)
        {
            _ = this.bitByteModels[i]?.Initialize();
            _ = this.classificationModels[i]?.Initialize();
            _ = this.userDataModels[i]?.Initialize();
        }

        this.deltaXIntegerDecompressor.Initialize();
        this.deltaYIntegerDecompressor.Initialize();
        this.zIntegerDecompressor.Initialize();

        // init last item
        item[..PointDataRecord.Size].CopyTo(this.lastPoint);

        // but set intensity to zero
        this.lastPoint[12] = default;
        this.lastPoint[13] = default;

        return true;
    }

    /// <inheritdoc/>
    LasPointSpan IPointDataRecordReader.Read(ReadOnlySpan<byte> source)
    {
        ReadOnlySpan<byte> processedData = this.ProcessData();
        return new(this.Read(processedData[..basePointDataLength]), processedData[basePointDataLength..]);
    }

    /// <inheritdoc/>
    async ValueTask<LasPointMemory> IPointDataRecordReader.ReadAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken)
    {
        ReadOnlyMemory<byte> processedData = await this.ProcessDataAsync(cancellationToken).ConfigureAwait(false);
        return new(await this.ReadAsync(processedData[..basePointDataLength], cancellationToken).ConfigureAwait(false), processedData[basePointDataLength..]);
    }

    /// <inheritdoc cref="IPointDataRecordReader.Read"/>
    protected abstract T Read(ReadOnlySpan<byte> source);

    /// <inheritdoc cref="IPointDataRecordReader.ReadAsync"/>
    protected virtual ValueTask<T> ReadAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(this.Read(source.Span));
    }

    /// <summary>
    /// Processes the data.
    /// </summary>
    /// <returns>The processed data.</returns>
    protected virtual Span<byte> ProcessData() => this.ProcessDataCore();

    /// <summary>
    /// Processes the data asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The processed data.</returns>
    protected virtual ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default) => new(this.ProcessDataCore());

    private byte[] ProcessDataCore()
    {
        // decompress which other values have changed
        var changedValues = (int)decoder.DecodeSymbol(this.changedValuesModel);

        byte numberOfReturns;
        uint returnMap;
        uint returnLevel;
        if (changedValues is not 0)
        {
            // decompress the edge of flight line, scan direction flag, ... if it has changed
            if ((changedValues & 32) is not 0)
            {
                var model = this.bitByteModels[this.lastPoint[Constants.PointDataRecord.FlagsFieldOffset]];
                if (model is null)
                {
                    model = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                    this.bitByteModels[this.lastPoint[Constants.PointDataRecord.FlagsFieldOffset]] = model;
                    _ = model.Initialize();
                }

                this.lastPoint[Constants.PointDataRecord.FlagsFieldOffset] = (byte)decoder.DecodeSymbol(model);
            }

            var returnNumber = FieldAccessors.PointDataRecord.GetReturnNumber(this.lastPoint);
            numberOfReturns = FieldAccessors.PointDataRecord.GetNumberOfReturns(this.lastPoint);
            returnMap = Common.NumberReturnMap[numberOfReturns][returnNumber];
            returnLevel = Common.NumberReturnLevel[numberOfReturns][returnNumber];

            // decompress the intensity if it has changed
            if ((changedValues & 16) is not 0)
            {
                var intensity = (ushort)this.intensityIntegerDecompressor.Decompress(this.lastIntensity[returnMap], returnMap < 3U ? returnMap : 3U);
                FieldAccessors.PointDataRecord.SetIntensity(this.lastPoint, intensity);
                this.lastIntensity[returnMap] = intensity;
            }
            else
            {
                FieldAccessors.PointDataRecord.SetIntensity(this.lastPoint, this.lastIntensity[returnMap]);
            }

            // decompress the classification ... if it has changed
            if ((changedValues & 8) is not 0)
            {
                var model = this.classificationModels[this.lastPoint[Constants.PointDataRecord.ClassificationFieldOffset]];
                if (model is null)
                {
                    model = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                    this.classificationModels[this.lastPoint[Constants.PointDataRecord.ClassificationFieldOffset]] = model;
                    _ = model.Initialize();
                }

                this.lastPoint[Constants.PointDataRecord.ClassificationFieldOffset] = (byte)decoder.DecodeSymbol(model);
            }

            // decompress the scan angle rank ... if it has changed
            if ((changedValues & 4) is not 0)
            {
                var model = FieldAccessors.PointDataRecord.GetScanDirectionFlag(this.lastPoint) ? this.scanAngleRankModels1 : this.scanAngleRankModels0;
                var val = (int)decoder.DecodeSymbol(model);
                this.lastPoint[Constants.PointDataRecord.ScanAngleRankFieldOffset] = Compression.ExtensionMethods.Fold(val + this.lastPoint[Constants.PointDataRecord.ScanAngleRankFieldOffset]);
            }

            // decompress the user data ... if it has changed
            if ((changedValues & 2) is not 0)
            {
                var model = this.userDataModels[this.lastPoint[Constants.PointDataRecord.UserDataFieldOffset]];
                if (model is null)
                {
                    model = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                    this.userDataModels[this.lastPoint[Constants.PointDataRecord.UserDataFieldOffset]] = model;
                    _ = model.Initialize();
                }

                this.lastPoint[Constants.PointDataRecord.UserDataFieldOffset] = (byte)decoder.DecodeSymbol(model);
            }

            // decompress the point source ID ... if it has changed
            if ((changedValues & 1) is not 0)
            {
                var pointSourceId = (ushort)this.pointSourceIdIntegerDecompressor.Decompress(FieldAccessors.PointDataRecord.GetPointSourceId(this.lastPoint));
                FieldAccessors.PointDataRecord.SetPointSourceId(this.lastPoint, pointSourceId);
            }
        }
        else
        {
            var returnNumber = FieldAccessors.PointDataRecord.GetReturnNumber(this.lastPoint);
            numberOfReturns = FieldAccessors.PointDataRecord.GetNumberOfReturns(this.lastPoint);
            returnMap = Common.NumberReturnMap[numberOfReturns][returnNumber];
            returnLevel = Common.NumberReturnLevel[numberOfReturns][returnNumber];
        }

        // decompress x coordinate
        var coordinateContext = numberOfReturns is 1 ? 1U : 0U;
        var median = this.lastXDiffMedian5[returnMap].Get();
        var diff = this.deltaXIntegerDecompressor.Decompress(median, coordinateContext);
        FieldAccessors.PointDataRecord.SetX(this.lastPoint, FieldAccessors.PointDataRecord.GetX(this.lastPoint) + diff);
        this.lastXDiffMedian5[returnMap].Add(diff);

        // decompress y coordinate
        median = this.lastYDiffMedian5[returnMap].Get();
        var kBits = this.deltaXIntegerDecompressor.K;
        diff = this.deltaYIntegerDecompressor.Decompress(median, coordinateContext + (kBits < 20U ? kBits.ZeroBit0() : 20U));
        FieldAccessors.PointDataRecord.SetY(this.lastPoint, FieldAccessors.PointDataRecord.GetY(this.lastPoint) + diff);
        this.lastYDiffMedian5[returnMap].Add(diff);

        // decompress z coordinate
        kBits = (this.deltaXIntegerDecompressor.K + this.deltaYIntegerDecompressor.K) / 2;
        var z = this.zIntegerDecompressor.Decompress(this.lastHeight[returnLevel], coordinateContext + (kBits < 18U ? kBits.ZeroBit0() : 18U));
        FieldAccessors.PointDataRecord.SetZ(this.lastPoint, z);
        this.lastHeight[returnLevel] = z;

        // copy the last point
        Array.Copy(this.lastPoint, this.data, PointDataRecord.Size);

        return this.data;
    }
}