// -----------------------------------------------------------------------
// <copyright file="ArrowLasReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Apache.Arrow"/> to <see cref="ILasWriter"/> Stream.
/// </summary>
public sealed class ArrowLasReader : ILasReader, IDisposable
{
    private readonly IEnumerator<RecordBatch> batchEnumerator;
    private readonly Lazy<HeaderBlock> lazyHeaderBlock;
    private Schema? schema;
    private RecordBatch? currentBatch;
    private int currentRowIndex;
    private ulong currentBatchStartIndex;
    private bool isClosed;

    /// <summary>
    /// Initialises a new instance of the <see cref="ArrowLasReader"/> class.
    /// </summary>
    /// <param name="stream">The record batches.</param>
    public ArrowLasReader(IEnumerable<RecordBatch> stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        this.batchEnumerator = stream.GetEnumerator();
        this.currentRowIndex = -1;
        this.lazyHeaderBlock = new(() =>
        {
            if (!this.EnsureSchema())
            {
                return default;
            }

            var pointDataFormatId = byte.Parse(this.schema.Metadata[Constants.Metadata.PointDataFormatId], System.Globalization.CultureInfo.InvariantCulture);
            var builder = new HeaderBlockBuilder(pointDataFormatId)
            {
                Version = Version.Parse(this.schema.Metadata[Constants.Metadata.Version]),
                GlobalEncoding = Enum.Parse<GlobalEncoding>(this.schema.Metadata[Constants.Metadata.GlobalEncoding]),
                Offset = ParseVector3D(this.schema.Metadata[Constants.Metadata.Offset], System.Globalization.CultureInfo.InvariantCulture),
                ScaleFactor = ParseVector3D(this.schema.Metadata[Constants.Metadata.ScaleFactor], System.Globalization.CultureInfo.InvariantCulture),
#if LAS1_5_OR_GREATER
                TimeOffset = ushort.Parse(this.schema.Metadata[Constants.Metadata.TimeOffset], System.Globalization.CultureInfo.InvariantCulture),
#endif
            };

            return builder.HeaderBlock;
        });
    }

    /// <inheritdoc/>
    public HeaderBlock Header => this.lazyHeaderBlock.Value;

    /// <inheritdoc/>
    public IReadOnlyList<VariableLengthRecord> VariableLengthRecords { get; } = [];

    /// <inheritdoc/>
    public IReadOnlyList<ExtendedVariableLengthRecord> ExtendedVariableLengthRecords { get; } = [];

    /// <inheritdoc/>
    public LasPointSpan ReadPointDataRecord()
    {
        if (this.isClosed)
        {
            return default;
        }

        if (!this.EnsureSchema())
        {
            return default; // null stream
        }

        this.currentRowIndex++;

        // Current Batch is not null
        if (this.currentBatch is not null && this.currentRowIndex < this.currentBatch.Length)
        {
            return new(ReadPointDataRecord(this.Header.PointDataFormatId, this.currentBatch, this.currentRowIndex), []);
        }

        // Need next Batch
        if (this.MoveToNextBatch())
        {
            return new(ReadPointDataRecord(this.Header.PointDataFormatId, this.currentBatch, this.currentRowIndex), []);
        }

        // No more data batch
        this.currentBatch = null!;
        this.isClosed = true;
        return default;
    }

    /// <inheritdoc/>
    public LasPointSpan ReadPointDataRecord(ulong index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, this.currentBatchStartIndex + (ulong)this.currentRowIndex);

        while (!IsInCurrentBatch(index))
        {
            if (this.MoveToNextBatch())
            {
                continue;
            }

            this.currentBatch = null!;
            this.isClosed = true;
            return default;
        }

        this.currentRowIndex = (int)(index - this.currentBatchStartIndex);
        return new(ReadPointDataRecord(this.Header.PointDataFormatId, this.currentBatch, this.currentRowIndex), []);

        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(currentBatch))]
        bool IsInCurrentBatch(ulong requiredIndex)
        {
            return this.currentBatch is not null && (this.currentBatchStartIndex + (ulong)this.currentBatch.Length) < requiredIndex;
        }
    }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method", Justification = "This would cause recursion")]
    public ValueTask<LasPointMemory> ReadPointDataRecordAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var lasPointSpan = this.ReadPointDataRecord();
        return new(new LasPointMemory(lasPointSpan.PointDataRecord!, ReadOnlyMemory<byte>.Empty));
    }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method", Justification = "This would cause recursion")]
    public ValueTask<LasPointMemory> ReadPointDataRecordAsync(ulong index, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var lasPointSpan = this.ReadPointDataRecord(index);
        return new(new LasPointMemory(lasPointSpan.PointDataRecord!, ReadOnlyMemory<byte>.Empty));
    }

    /// <inheritdoc/>
    public void Dispose() => this.batchEnumerator.Dispose();

    private static Vector3D ParseVector3D(string input, IFormatProvider? formatProvider)
    {
        var numberGroupSeparator = System.Globalization.NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;

        ReadOnlySpan<char> span = input;
        span = span[(span.IndexOf('<') + 1)..span.IndexOf('>')];

        var values = new double[3];
        var valuesIndex = 0;
        while (true)
        {
            var nextIndex = span.IndexOf(numberGroupSeparator, StringComparison.Ordinal);
            if (nextIndex is -1)
            {
                nextIndex = span.Length;
            }

            values[valuesIndex++] = double.Parse(span[..nextIndex], formatProvider);

            if (nextIndex == span.Length)
            {
                break;
            }

            span = span[(nextIndex + 1)..];
        }

        return new(values);
    }

    private static IBasePointDataRecord ReadPointDataRecord(byte pointDataFormatId, RecordBatch recordBatch, int index)
    {
        // get the base values
        return pointDataFormatId switch
        {
            PointDataRecord.Id => new PointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<uint>(recordBatch.Column(Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
            },
            GpsPointDataRecord.Id => new GpsPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<uint>(recordBatch.Column(Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
            },
#if LAS1_2_OR_GREATER
            ColorPointDataRecord.Id => new ColorPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<uint>(recordBatch.Column(Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
            },
            GpsColorPointDataRecord.Id => new GpsColorPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<uint>(recordBatch.Column(Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
            },
#endif
#if LAS1_3_OR_GREATER
            GpsWaveformPointDataRecord.Id => new GpsWaveformPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<uint>(recordBatch.Column(Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                WavePacketDescriptorIndex = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                ByteOffsetToWaveformData = GetValue<ulong>(recordBatch.Column(Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                WaveformPacketSizeInBytes = GetValue<uint>(recordBatch.Column(Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                ReturnPointWaveformLocation = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                ParametricDx = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                ParametricDy = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                ParametricDz = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index),
            },
            GpsColorWaveformPointDataRecord.Id => new GpsColorWaveformPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<uint>(recordBatch.Column(Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
                WavePacketDescriptorIndex = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                ByteOffsetToWaveformData = GetValue<ulong>(recordBatch.Column(Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                WaveformPacketSizeInBytes = GetValue<uint>(recordBatch.Column(Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                ReturnPointWaveformLocation = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                ParametricDx = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                ParametricDy = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                ParametricDz = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index),
            },
#endif
#if LAS1_4_OR_GREATER
            ExtendedGpsPointDataRecord.Id => new ExtendedGpsPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Overlap = GetValue<bool>(recordBatch.Column(Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index),
                ScannerChannel = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index),
                Classification = (ExtendedClassification)GetValue<uint>(recordBatch.Column(Constants.Columns.Extended.Classification, StringComparer.Ordinal), index),
                ScanAngle = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
            },
            ExtendedGpsColorPointDataRecord.Id => new ExtendedGpsColorPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Overlap = GetValue<bool>(recordBatch.Column(Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index),
                ScannerChannel = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index),
                Classification = (ExtendedClassification)GetValue<uint>(recordBatch.Column(Constants.Columns.Extended.Classification, StringComparer.Ordinal), index),
                ScanAngle = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
            },
            ExtendedGpsColorNearInfraredPointDataRecord.Id => new ExtendedGpsColorNearInfraredPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Overlap = GetValue<bool>(recordBatch.Column(Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index),
                ScannerChannel = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index),
                Classification = (ExtendedClassification)GetValue<uint>(recordBatch.Column(Constants.Columns.Extended.Classification, StringComparer.Ordinal), index),
                ScanAngle = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
                NearInfrared = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Nir.NearInfrared, StringComparer.Ordinal), index),
            },
            ExtendedGpsWaveformPointDataRecord.Id => new ExtendedGpsWaveformPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Overlap = GetValue<bool>(recordBatch.Column(Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index),
                ScannerChannel = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index),
                Classification = (ExtendedClassification)GetValue<uint>(recordBatch.Column(Constants.Columns.Extended.Classification, StringComparer.Ordinal), index),
                ScanAngle = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                WavePacketDescriptorIndex = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                ByteOffsetToWaveformData = GetValue<ulong>(recordBatch.Column(Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                WaveformPacketSizeInBytes = GetValue<uint>(recordBatch.Column(Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                ReturnPointWaveformLocation = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                ParametricDx = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                ParametricDy = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                ParametricDz = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index),
            },
            ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id => new ExtendedGpsColorNearInfraredWaveformPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetValue<bool>(recordBatch.Column(Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetValue<bool>(recordBatch.Column(Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetValue<bool>(recordBatch.Column(Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetValue<bool>(recordBatch.Column(Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetValue<bool>(recordBatch.Column(Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Overlap = GetValue<bool>(recordBatch.Column(Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index),
                ScannerChannel = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index),
                Classification = (ExtendedClassification)GetValue<uint>(recordBatch.Column(Constants.Columns.Extended.Classification, StringComparer.Ordinal), index),
                ScanAngle = (sbyte)GetValue<int>(recordBatch.Column(Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
                NearInfrared = (ushort)GetValue<uint>(recordBatch.Column(Constants.Columns.Nir.NearInfrared, StringComparer.Ordinal), index),
                WavePacketDescriptorIndex = (byte)GetValue<uint>(recordBatch.Column(Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                ByteOffsetToWaveformData = GetValue<ulong>(recordBatch.Column(Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                WaveformPacketSizeInBytes = GetValue<uint>(recordBatch.Column(Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                ReturnPointWaveformLocation = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                ParametricDx = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                ParametricDy = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                ParametricDz = GetValue<float>(recordBatch.Column(Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index),
            },
#endif
            _ => throw new System.Diagnostics.UnreachableException(),
        };

        static T GetValue<T>(IArrowArray arrowArray, int index)
            where T : struct, IEquatable<T>
        {
            if (arrowArray is not PrimitiveArray<T> primitiveArray)
            {
                throw new InvalidCastException();
            }

            if (primitiveArray.GetValue(index) is { } value)
            {
                return value;
            }

            throw new KeyNotFoundException();
        }
    }

    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(schema))]
    private bool EnsureSchema()
    {
        if (this.schema is not null)
        {
            return true;
        }

        if (this.batchEnumerator.MoveNext())
        {
            this.currentBatch = this.batchEnumerator.Current;
            if (this.currentBatch is null)
            {
                return false;
            }

            this.schema = this.currentBatch.Schema;
            return true;
        }

        this.isClosed = true;
        return false;
    }

    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(currentBatch))]
    private bool MoveToNextBatch()
    {
        if (!this.batchEnumerator.MoveNext())
        {
            return false;
        }

        this.currentBatch?.Dispose();
        this.currentBatch = this.batchEnumerator.Current;
        if (this.currentBatch is null)
        {
            return false;
        }

        this.schema = this.currentBatch.Schema;
        this.currentBatchStartIndex += (ulong)this.currentRowIndex;
        this.currentRowIndex = 0;

        return this.currentBatch.Length is not 0;
    }
}