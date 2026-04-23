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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    private readonly Lazy<ushort> lazyPointDataLength;
#endif
    private Schema? schema;
    private RecordBatch? currentBatch;
    private int currentRowIndex;
    private ulong currentBatchStartIndex;
    private bool isClosed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrowLasReader"/> class.
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

            HeaderBlockBuilder builder;
            if (!this.schema.HasMetadata)
            {
                builder = new(InferPointDataRecordId(this.schema));
            }
            else
            {
                var pointDataFormatId = byte.Parse(this.schema.Metadata[Arrow.Constants.Metadata.PointDataFormatId], System.Globalization.CultureInfo.InvariantCulture);
                builder = new(pointDataFormatId)
                {
                    Version = Version.Parse(this.schema.Metadata[Arrow.Constants.Metadata.Version]),
#if LAS1_2_OR_GREATER
                    GlobalEncoding = Enum.Parse<GlobalEncoding>(this.schema.Metadata[Arrow.Constants.Metadata.GlobalEncoding]),
#endif
                    Offset = ParseVector3D(this.schema.Metadata[Arrow.Constants.Metadata.Offset], System.Globalization.CultureInfo.InvariantCulture),
                    ScaleFactor = ParseVector3D(this.schema.Metadata[Arrow.Constants.Metadata.ScaleFactor], System.Globalization.CultureInfo.InvariantCulture),
#if LAS1_5_OR_GREATER
                    TimeOffset = ushort.Parse(this.schema.Metadata[Arrow.Constants.Metadata.TimeOffset], System.Globalization.CultureInfo.InvariantCulture),
#endif
                };
            }

            return builder.HeaderBlock;
        });

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        this.lazyPointDataLength = new(() => this.lazyHeaderBlock.Value.GetPointDataRecordLength());
#endif
    }

    /// <inheritdoc/>
    public HeaderBlock Header => this.lazyHeaderBlock.Value;

    /// <inheritdoc/>
    public IReadOnlyList<VariableLengthRecord> VariableLengthRecords { get; } = [];

#if LAS1_4_OR_GREATER
    /// <inheritdoc/>
    public IReadOnlyList<ExtendedVariableLengthRecord> ExtendedVariableLengthRecords { get; } = [];
#endif

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <inheritdoc/>
    public ushort PointDataLength => this.lazyPointDataLength.Value;
#endif

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
            this.currentRowIndex++;
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
        ArgumentOutOfRangeException.ThrowIfLessThan(index, this.currentBatchStartIndex);

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
            return this.currentBatch is { Length: var length } && requiredIndex < (this.currentBatchStartIndex + (ulong)length);
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

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <inheritdoc/>
    public int ReadPointDataRecordData(Span<byte> buffer)
    {
        if (this.isClosed)
        {
            return default;
        }

        if (!this.EnsureSchema())
        {
            return default; // null stream
        }

        var pointDataLength = this.PointDataLength;
        var count = 0;

        while (buffer.Length > this.PointDataLength)
        {
            this.currentRowIndex++;

            // Current Batch is not null
            if (this.currentBatch is not null && this.currentRowIndex < this.currentBatch.Length)
            {
                var bytesWritten = ReadPointDataRecord(this.Header.PointDataFormatId, this.currentBatch, this.currentRowIndex, buffer[..pointDataLength]);
                buffer = buffer[bytesWritten..];
                count++;
                continue;
            }

            // Need next Batch
            if (this.MoveToNextBatch())
            {
                this.currentRowIndex++;
                var bytesWritten = ReadPointDataRecord(this.Header.PointDataFormatId, this.currentBatch, this.currentRowIndex, buffer[..pointDataLength]);
                buffer = buffer[bytesWritten..];
                count++;
                continue;
            }

            break;
        }

        return count;
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method", Justification = "This would cause recursion")]
    public ValueTask<int> ReadPointDataRecordDataAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(this.ReadPointDataRecordData(buffer.Span));
    }
#endif

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

    private static IBasePointDataRecord ReadPointDataRecord(byte pointDataFormatId, RecordBatch recordBatch, int index) =>

        // get the base values
        pointDataFormatId switch
        {
            PointDataRecord.Id => new PointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
            },
            GpsPointDataRecord.Id => new GpsPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
            },
#if LAS1_2_OR_GREATER
            ColorPointDataRecord.Id => new ColorPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
            },
            GpsColorPointDataRecord.Id => new GpsColorPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
            },
#endif
#if LAS1_3_OR_GREATER
            GpsWaveformPointDataRecord.Id => new GpsWaveformPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                WavePacketDescriptorIndex = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                ByteOffsetToWaveformData = GetValue<ulong>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                WaveformPacketSizeInBytes = GetValue<uint>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                ReturnPointWaveformLocation = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                ParametricDx = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                ParametricDy = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                ParametricDz = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index),
            },
            GpsColorWaveformPointDataRecord.Id => new GpsColorWaveformPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Classification = (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index),
                ScanAngleRank = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
                WavePacketDescriptorIndex = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                ByteOffsetToWaveformData = GetValue<ulong>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                WaveformPacketSizeInBytes = GetValue<uint>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                ReturnPointWaveformLocation = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                ParametricDx = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                ParametricDy = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                ParametricDz = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index),
            },
#endif
#if LAS1_4_OR_GREATER
            ExtendedGpsPointDataRecord.Id => new ExtendedGpsPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Overlap = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index),
                ScannerChannel = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index),
                Classification = (ExtendedClassification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.Classification, StringComparer.Ordinal), index),
                ScanAngle = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
            },
            ExtendedGpsColorPointDataRecord.Id => new ExtendedGpsColorPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Overlap = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index),
                ScannerChannel = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index),
                Classification = (ExtendedClassification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.Classification, StringComparer.Ordinal), index),
                ScanAngle = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
            },
            ExtendedGpsColorNearInfraredPointDataRecord.Id => new ExtendedGpsColorNearInfraredPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Overlap = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index),
                ScannerChannel = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index),
                Classification = (ExtendedClassification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.Classification, StringComparer.Ordinal), index),
                ScanAngle = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
                NearInfrared = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Nir.NearInfrared, StringComparer.Ordinal), index),
            },
            ExtendedGpsWaveformPointDataRecord.Id => new ExtendedGpsWaveformPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Overlap = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index),
                ScannerChannel = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index),
                Classification = (ExtendedClassification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.Classification, StringComparer.Ordinal), index),
                ScanAngle = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                WavePacketDescriptorIndex = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                ByteOffsetToWaveformData = GetValue<ulong>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                WaveformPacketSizeInBytes = GetValue<uint>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                ReturnPointWaveformLocation = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                ParametricDx = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                ParametricDy = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                ParametricDz = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index),
            },
            ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id => new ExtendedGpsColorNearInfraredWaveformPointDataRecord
            {
                X = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index),
                Y = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index),
                Z = GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index),
                Intensity = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index),
                ReturnNumber = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index),
                NumberOfReturns = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index),
                ScanDirectionFlag = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index),
                EdgeOfFlightLine = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index),
                Synthetic = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index),
                KeyPoint = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index),
                Withheld = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index),
                UserData = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index),
                PointSourceId = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index),
                Overlap = GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index),
                ScannerChannel = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index),
                Classification = (ExtendedClassification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.Classification, StringComparer.Ordinal), index),
                ScanAngle = GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index),
                GpsTime = GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index),
                Color = new()
                {
                    R = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    G = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    B = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                },
                NearInfrared = GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Nir.NearInfrared, StringComparer.Ordinal), index),
                WavePacketDescriptorIndex = GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                ByteOffsetToWaveformData = GetValue<ulong>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                WaveformPacketSizeInBytes = GetValue<uint>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                ReturnPointWaveformLocation = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                ParametricDx = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                ParametricDy = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                ParametricDz = GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index),
            },
#endif
            _ => throw new System.Diagnostics.UnreachableException(),
        };

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    private static int ReadPointDataRecord(byte pointDataFormatId, RecordBatch recordBatch, int index, Span<byte> destination)
    {
        switch (pointDataFormatId)
        {
            case PointDataRecord.Id:
                FieldAccessors.PointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetSynthetic(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetClassification(destination, (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanAngleRank(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index));
                return PointDataRecord.Size;
            case GpsPointDataRecord.Id:
                FieldAccessors.PointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetSynthetic(ref destination[Constants.PointDataRecord.ClassificationFieldOffset], GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetClassification(destination, (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanAngleRank(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetGpsTime(destination, GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index));
                return GpsPointDataRecord.Size;
#if LAS1_2_OR_GREATER
            case ColorPointDataRecord.Id:
                FieldAccessors.PointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetSynthetic(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetClassification(destination, (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanAngleRank(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetColor(
                    destination,
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index));
                return ColorPointDataRecord.Size;
            case GpsColorPointDataRecord.Id:
                FieldAccessors.PointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetSynthetic(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetClassification(destination, (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanAngleRank(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetGpsTime(destination, GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetGpsColor(
                    destination,
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index));
                return GpsPointDataRecord.Size;
#endif
#if LAS1_3_OR_GREATER
            case GpsWaveformPointDataRecord.Id:
                FieldAccessors.PointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetSynthetic(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetClassification(destination, (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanAngleRank(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetGpsTime(destination, GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetGpsWaveform(
                    destination,
                    GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                    GetValue<ulong>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                    GetValue<uint>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index));
                return GpsWaveformPointDataRecord.Size;
            case GpsColorWaveformPointDataRecord.Id:
                FieldAccessors.PointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetSynthetic(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetClassification(destination, (Classification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.Classification, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetScanAngleRank(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Legacy.ScanAngleRank, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetGpsTime(destination, GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index));
                FieldAccessors.PointDataRecord.SetGpsColorWaveform(
                    destination,
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index),
                    GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                    GetValue<ulong>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                    GetValue<uint>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index));
                return GpsColorWaveformPointDataRecord.Size;
#endif
#if LAS1_4_OR_GREATER
            case ExtendedGpsPointDataRecord.Id:
                FieldAccessors.ExtendedPointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetSynthetic(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetOverlap(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScannerChannel(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetClassification(destination, (ExtendedClassification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.Classification, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScanAngle(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetGpsTime(destination, GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index));
                return ExtendedGpsPointDataRecord.Size;
            case ExtendedGpsColorPointDataRecord.Id:
                FieldAccessors.ExtendedPointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetSynthetic(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetOverlap(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScannerChannel(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetClassification(destination, (ExtendedClassification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.Classification, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScanAngle(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetGpsTime(destination, GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetColor(
                    destination,
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index));
                return ExtendedGpsColorPointDataRecord.Size;
            case ExtendedGpsColorNearInfraredPointDataRecord.Id:
                FieldAccessors.ExtendedPointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetSynthetic(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetOverlap(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScannerChannel(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetClassification(destination, (ExtendedClassification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.Classification, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScanAngle(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetGpsTime(destination, GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetColor(
                    destination,
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetNearInfrared(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Nir.NearInfrared, StringComparer.Ordinal), index));
                return ExtendedGpsColorNearInfraredPointDataRecord.Size;
            case ExtendedGpsWaveformPointDataRecord.Id:
                FieldAccessors.ExtendedPointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetSynthetic(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetOverlap(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScannerChannel(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetClassification(destination, (ExtendedClassification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.Classification, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScanAngle(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetGpsTime(destination, GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetWaveform(
                    destination,
                    GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                    GetValue<ulong>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                    GetValue<uint>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index));
                return ExtendedGpsWaveformPointDataRecord.Size;
            case ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id:
                FieldAccessors.ExtendedPointDataRecord.SetX(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.X, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetY(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Y, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetZ(destination, GetValue<int>(recordBatch.Column(Arrow.Constants.Columns.Z, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetIntensity(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Intensity, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetReturnNumber(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.ReturnNumber, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetNumberOfReturns(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.NumberOfReturns, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScanDirectionFlag(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.ScanDirectionFlag, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetEdgeOfFlightLine(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.EdgeOfFlightLine, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetSynthetic(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Synthetic, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetKeyPoint(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.KeyPoint, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetWithheld(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Withheld, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetUserData(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.UserData, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetPointSourceId(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.PointSourceId, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetOverlap(destination, GetBooleanValue(recordBatch.Column(Arrow.Constants.Columns.Extended.Overlap, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScannerChannel(destination, GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScannerChannel, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetClassification(destination, (ExtendedClassification)GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Extended.Classification, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetScanAngle(destination, GetValue<sbyte>(recordBatch.Column(Arrow.Constants.Columns.Extended.ScanAngle, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetGpsTime(destination, GetValue<double>(recordBatch.Column(Arrow.Constants.Columns.Gps.GpsTime, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetColor(
                    destination,
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Red, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Green, StringComparer.Ordinal), index),
                    GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Color.Blue, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetNearInfrared(destination, GetValue<ushort>(recordBatch.Column(Arrow.Constants.Columns.Nir.NearInfrared, StringComparer.Ordinal), index));
                FieldAccessors.ExtendedPointDataRecord.SetNearInfraredWaveform(
                    destination,
                    GetValue<byte>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WavePacketDescriptorIndex, StringComparer.Ordinal), index),
                    GetValue<ulong>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ByteOffsetToWaveformData, StringComparer.Ordinal), index),
                    GetValue<uint>(recordBatch.Column(Arrow.Constants.Columns.Waveform.WaveformPacketSizeInBytes, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ReturnPointWaveformLocation, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDx, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDy, StringComparer.Ordinal), index),
                    GetValue<float>(recordBatch.Column(Arrow.Constants.Columns.Waveform.ParametricDz, StringComparer.Ordinal), index));
                return ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size;
#endif
            default:
                throw new System.Diagnostics.UnreachableException();
        }
    }
#endif

    private static byte InferPointDataRecordId(Schema schema)
    {
        if (IsLegacy(schema))
        {
            if (!HasGps(schema))
            {
                // No GPS
#if LAS1_2_OR_GREATER
                return HasColor(schema) ? ColorPointDataRecord.Id : PointDataRecord.Id;
#else
                return PointDataRecord.Id;
#endif
            }

            // Have GPS
#if LAS1_2_OR_GREATER
            if (!HasColor(schema))
            {
                return GpsPointDataRecord.Id;
            }

            // Have Color
#if LAS1_3_OR_GREATER
            return HasWaveform(schema)
                ? GpsColorWaveformPointDataRecord.Id
                : GpsColorPointDataRecord.Id;
#endif
#else
            return GpsPointDataRecord.Id;
#endif
        }

#if LAS1_4_OR_GREATER
        if (!IsExtended(schema))
        {
            throw new System.Diagnostics.UnreachableException();
        }

        if (!HasColor(schema))
        {
            return HasWaveform(schema)
                ? ExtendedGpsWaveformPointDataRecord.Id
                : ExtendedGpsPointDataRecord.Id;
        }

        if (!HasNir(schema))
        {
            return ExtendedGpsColorPointDataRecord.Id;
        }

        return HasWaveform(schema)
            ? ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id
            : ExtendedGpsColorNearInfraredPointDataRecord.Id;
#else
        throw new System.Diagnostics.UnreachableException();
#endif

        static bool IsLegacy(Schema schema)
        {
            return schema.FieldsList.Any(static field => field.Name is Arrow.Constants.Columns.Legacy.ScanAngleRank);
        }

        static bool HasGps(Schema schema)
        {
            return schema.FieldsList.Any(static field => field.Name is Arrow.Constants.Columns.Gps.GpsTime);
        }

#if LAS1_2_OR_GREATER
        static bool HasColor(Schema schema)
        {
            return schema.FieldsList.Any(static field => field.Name is Arrow.Constants.Columns.Color.Red);
        }
#endif

#if LAS1_3_OR_GREATER
        static bool HasWaveform(Schema schema)
        {
            return schema.FieldsList.Any(static field => field.Name is Arrow.Constants.Columns.Waveform.WavePacketDescriptorIndex);
        }
#endif

#if LAS1_4_OR_GREATER
        static bool IsExtended(Schema schema3)
        {
            return schema3.FieldsList.Any(static field => field.Name is Arrow.Constants.Columns.Extended.ScannerChannel);
        }

        static bool HasNir(Schema schema4)
        {
            return schema4.FieldsList.Any(static field => field.Name is Arrow.Constants.Columns.Nir.NearInfrared);
        }
#endif
    }

    private static bool GetBooleanValue(IArrowArray arrowArray, int index) => arrowArray switch
    {
        BooleanArray booleanArray when booleanArray.GetValue(index) is { } value => value,
        BooleanArray => throw new KeyNotFoundException(),
        _ when Convert.ChangeType(GetObjectValue(arrowArray, index), typeof(bool), System.Globalization.CultureInfo.InvariantCulture) is bool convertedValue => convertedValue,
        _ => throw new InvalidCastException(),
    };

    private static T GetValue<T>(IArrowArray arrowArray, int index)
        where T : struct, IEquatable<T> => arrowArray switch
        {
            PrimitiveArray<T> primitiveArray when primitiveArray.GetValue(index) is { } value => value,
            PrimitiveArray<T> => throw new KeyNotFoundException(),
            _ when Convert.ChangeType(GetObjectValue(arrowArray, index), typeof(T), System.Globalization.CultureInfo.InvariantCulture) is T convertedValue => convertedValue,
            _ => throw new InvalidCastException(),
        };

    private static object? GetObjectValue(IArrowArray arrowArray, int index) => arrowArray switch
    {
        BooleanArray values => values.GetValue(index),
        UInt8Array values => values.GetValue(index),
        Int8Array values => values.GetValue(index),
        UInt16Array values => values.GetValue(index),
        Int16Array values => values.GetValue(index),
        UInt32Array values => values.GetValue(index),
        Int32Array values => values.GetValue(index),
        UInt64Array values => values.GetValue(index),
        Int64Array values => values.GetValue(index),
        FloatArray values => values.GetValue(index),
        DoubleArray values => values.GetValue(index),
        _ => null,
    };

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

        var currentBatchLength = this.currentBatch?.Length ?? 0;
        this.currentBatch?.Dispose();
        this.currentBatch = this.batchEnumerator.Current;
        if (this.currentBatch is null)
        {
            return false;
        }

        this.schema = this.currentBatch.Schema;
        this.currentBatchStartIndex += (ulong)currentBatchLength;
        this.currentRowIndex = -1;

        return this.currentBatch.Length is not 0;
    }
}