// -----------------------------------------------------------------------
// <copyright file="LasReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The LAS reader.
/// </summary>
public class LasReader : ILasReader, IDisposable
{
    private readonly bool leaveOpen;

    private readonly Readers.IPointDataRecordReader rawReader;

    private readonly ushort pointDataLength;

#if LAS1_4_OR_GREATER
    private readonly ulong numberOfPointRecords;
#else
    private readonly uint numberOfPointRecords;
#endif

    private readonly uint offsetToPointData;

    private readonly long offsetToVariableLengthRecords;

    private readonly long endOfPointDataRecords;

    private readonly byte[] buffer;

    private ulong currentPointIndex;

    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasReader"/> class based on the specified stream and character encoding, and optionally leaves the stream open.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LasReader"/> object is disposed; otherwise <see langword="false"/>.</param>
    /// <exception cref="ArgumentException">The stream does not support reading, the stream is null, or the stream is already closed.</exception>
    public LasReader(Stream input, bool leaveOpen = false)
    {
        this.BaseStream = input;
        this.leaveOpen = leaveOpen;
#if LAS1_4_OR_GREATER
        (this.Header, this.VariableLengthRecords, this.ExtendedVariableLengthRecords) =
            Initialize(
                this.BaseStream,
                default,
                out this.rawReader,
                out this.offsetToPointData,
                out this.offsetToVariableLengthRecords,
                out _,
                out this.pointDataLength,
                out this.endOfPointDataRecords);
#else
        (this.Header, this.VariableLengthRecords) =
            Initialize(
                this.BaseStream,
                default,
                out this.rawReader,
                out this.offsetToPointData,
                out this.offsetToVariableLengthRecords,
                out this.pointDataLength,
                out this.endOfPointDataRecords);
#endif
        this.numberOfPointRecords = this.Header.NumberOfPointRecords;
        this.buffer = new byte[this.pointDataLength];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasReader"/> class based on the specified stream and character encoding, and optionally leaves the stream open.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="fileSignature">The file signature.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LasReader"/> object is disposed; otherwise <see langword="false"/>.</param>
    /// <exception cref="ArgumentException">The stream does not support reading, the stream is null, or the stream is already closed.</exception>
    public LasReader(Stream input, string fileSignature, bool leaveOpen = false)
    {
        this.leaveOpen = leaveOpen;
        this.BaseStream = input;
#if LAS1_4_OR_GREATER
        (this.Header, this.VariableLengthRecords, this.ExtendedVariableLengthRecords) =
            Initialize(
                this.BaseStream,
                fileSignature,
                out this.rawReader,
                out this.offsetToPointData,
                out this.offsetToVariableLengthRecords,
                out _,
                out this.pointDataLength,
                out this.endOfPointDataRecords);
#else
        (this.Header, this.VariableLengthRecords) =
            Initialize(
                this.BaseStream,
                fileSignature,
                out this.rawReader,
                out this.offsetToPointData,
                out this.offsetToVariableLengthRecords,
                out this.pointDataLength,
                out this.endOfPointDataRecords);
#endif
        this.numberOfPointRecords = this.Header.NumberOfPointRecords;
        this.buffer = new byte[this.pointDataLength];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasReader"/> class based on the specified path.
    /// </summary>
    /// <param name="path">The file to be opened for reading.</param>
    public LasReader(string path)
        : this(CreateStream(path))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasReader"/> class based on the specified stream and character encoding, and optionally leaves the stream open.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LasReader"/> object is disposed; otherwise <see langword="false"/>.</param>
    /// <param name="headerReader">The header reader.</param>
    /// <param name="header">The header.</param>
    internal LasReader(Stream input, bool leaveOpen, HeaderBlockReader headerReader, HeaderBlock header)
    {
        this.BaseStream = input;
        this.leaveOpen = leaveOpen;
        this.Header = header;
#if LAS1_4_OR_GREATER
        (this.VariableLengthRecords, this.ExtendedVariableLengthRecords) =
            Initialize(
                this.BaseStream,
                headerReader,
                header,
                out this.rawReader,
                out this.offsetToPointData,
                out this.offsetToVariableLengthRecords,
                out _,
                out this.pointDataLength,
                out this.endOfPointDataRecords);
#else
        this.VariableLengthRecords =
            Initialize(
                this.BaseStream,
                headerReader,
                header,
                out this.rawReader,
                out this.offsetToPointData,
                out this.offsetToVariableLengthRecords,
                out this.pointDataLength,
                out this.endOfPointDataRecords);
#endif
        this.numberOfPointRecords = header.NumberOfPointRecords;
        this.buffer = new byte[this.pointDataLength];
    }

    /// <inheritdoc/>
    public HeaderBlock Header { get; }

    /// <inheritdoc/>
    public IReadOnlyList<VariableLengthRecord> VariableLengthRecords { get; }

#if LAS1_4_OR_GREATER
    /// <inheritdoc/>
    public IReadOnlyList<ExtendedVariableLengthRecord> ExtendedVariableLengthRecords { get; }
#endif

    /// <summary>
    /// Gets the base stream.
    /// </summary>
    public Stream BaseStream { get; }

    /// <summary>
    /// Gets the RAW reader.
    /// </summary>
    protected Readers.IPointDataRecordReader RawReader => this.rawReader;

    /// <summary>
    /// Gets the point data length.
    /// </summary>
    protected ushort PointDataLength => this.pointDataLength;

    /// <inheritdoc/>
    public virtual LasPointSpan ReadPointDataRecord()
    {
        if (!this.CheckEndOfPointData() || !this.CheckPointIndex())
        {
            return default;
        }

        _ = this.BaseStream.SwitchStreamIfMultiple(LasStreams.PointData);
        this.MoveToPointData();

        // read the data
        _ = this.BaseStream.Read(this.buffer, 0, this.buffer.Length);
        ReadOnlySpan<byte> data = this.buffer;

        // read the point.
        var point = this.rawReader.Read(data);
        this.IncrementPointIndex();
        return point;
    }

    /// <inheritdoc/>
    public LasPointSpan ReadPointDataRecord(ulong index)
    {
        this.MoveToPoint(index);
        var pointSpan = this.ReadPointDataRecord();
        return pointSpan.PointDataRecord is not null ? pointSpan : throw new KeyNotFoundException();
    }

    /// <inheritdoc/>
    public async virtual ValueTask<LasPointMemory> ReadPointDataRecordAsync(CancellationToken cancellationToken = default)
    {
        if (!this.CheckEndOfPointData() || !this.CheckPointIndex())
        {
            return default;
        }

        _ = this.BaseStream.SwitchStreamIfMultiple(LasStreams.PointData);
        this.MoveToPointData();

        // read the data
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        _ = await this.BaseStream.ReadAsync(this.buffer, cancellationToken).ConfigureAwait(false);
#else
        _ = await this.BaseStream.ReadAsync(this.buffer, 0, this.buffer.Length, cancellationToken).ConfigureAwait(false);
#endif

        // read the point.
        var point = await this.rawReader.ReadAsync(this.buffer, cancellationToken).ConfigureAwait(false);
        this.IncrementPointIndex();
        return point;
    }

    /// <inheritdoc/>
    public async ValueTask<LasPointMemory> ReadPointDataRecordAsync(ulong index, CancellationToken cancellationToken = default)
    {
        this.MoveToPoint(index);
        var pointSpan = await this.ReadPointDataRecordAsync(cancellationToken).ConfigureAwait(false);
        return pointSpan.PointDataRecord is not null ? pointSpan : throw new KeyNotFoundException();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET20_OR_GREATER
    /// <inheritdoc cref="Stream.Close" />
    public virtual void Close() => this.BaseStream.Close();
#endif

    /// <summary>
    /// Moves the reader to the variable length records.
    /// </summary>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    internal bool MoveToVariableLengthRecords()
    {
        if (this.BaseStream.CanSeek)
        {
            _ = this.BaseStream.Seek(this.offsetToVariableLengthRecords, SeekOrigin.Begin);
            return this.BaseStream.Position == this.offsetToVariableLengthRecords;
        }

        return false;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="LasReader"/> class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this.disposedValue)
        {
            return;
        }

        if (disposing && !this.leaveOpen)
        {
            this.BaseStream.Dispose();
        }

        this.disposedValue = true;
    }

    /// <summary>
    /// Checks the end of point data.
    /// </summary>
    /// <returns><see langword="true"/> if the position is before the end of point data; otherwise <see langword="false"/>.</returns>
    protected bool CheckEndOfPointData() => (this.BaseStream.Position + this.pointDataLength) <= this.endOfPointDataRecords;

    /// <summary>
    /// Checks the point index.
    /// </summary>
    /// <returns><see langword="true"/> if the point index is less than the number of records; otherwise <see langword="false"/>.</returns>
    protected bool CheckPointIndex() => this.CheckPointIndex(this.currentPointIndex);

    /// <summary>
    /// Checks the specified point index.
    /// </summary>
    /// <param name="pointIndex">The point index.</param>
    /// <returns><see langword="true"/> if <paramref name="pointIndex"/> is less than the number of records; otherwise <see langword="false"/>.</returns>
    protected bool CheckPointIndex(ulong pointIndex) => pointIndex < this.numberOfPointRecords;

    /// <summary>
    /// Increments the point index.
    /// </summary>
    protected void IncrementPointIndex() => this.currentPointIndex++;

    /// <summary>
    /// Move to the point data.
    /// </summary>
    protected void MoveToPointData() => this.BaseStream.MoveToPositionForwardsOnly(this.offsetToPointData);

    /// <summary>
    /// Gets the current index.
    /// </summary>
    /// <returns>The current point index.</returns>
    protected ulong GetCurrentIndex() => this.currentPointIndex;

    /// <summary>
    /// Move to the specified point.
    /// </summary>
    /// <param name="pointIndex">The point index.</param>
    protected void MoveToPoint(ulong pointIndex)
    {
        if (!this.CheckPointIndex(pointIndex))
        {
            throw new ArgumentOutOfRangeException(nameof(pointIndex));
        }

        if (!this.MoveToPoint(this.currentPointIndex, pointIndex))
        {
            ThrowInvalidOperationException(pointIndex);
        }

        this.currentPointIndex = pointIndex;

#if NET8_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1863:Use 'CompositeFormat'", Justification = "This is a formatted string for an exception.")]
#endif
        [System.Diagnostics.CodeAnalysis.DoesNotReturn]
        static void ThrowInvalidOperationException(ulong pointIndex)
        {
            throw new InvalidOperationException(string.Format(Properties.Resources.Culture, Properties.Resources.FailedToMoveToPoint, pointIndex));
        }
    }

    /// <summary>
    /// Move to the specified point.
    /// </summary>
    /// <param name="current">The current point index.</param>
    /// <param name="target">The target point index.</param>
    /// <returns><see langword="true"/> if the reader is now at <paramref name="target"/>; otherwise <see langword="false"/>.</returns>
    protected virtual bool MoveToPoint(ulong current, ulong target)
    {
        // get the position of the point
        var pointPosition = (long)(this.offsetToPointData + (target * this.pointDataLength));
        _ = this.BaseStream.SwitchStreamIfMultiple(LasStreams.PointData);
        this.BaseStream.MoveToPositionAbsolute(pointPosition);
        return this.BaseStream.Position == pointPosition;
    }

    private static
#if LAS1_4_OR_GREATER
        (HeaderBlock HeaderBlock, VariableLengthRecord[] VariableLengthRecords, ExtendedVariableLengthRecord[] ExtendedVariableLengthRecords)
#else
        (HeaderBlock HeaderBlock, VariableLengthRecord[] VariableLengthRecords)
#endif
        Initialize(
            Stream stream,
            string? fileSignature,
            out Readers.IPointDataRecordReader rawReader,
            out uint offsetToPointData,
            out long offsetToVariableLengthRecords,
#if LAS1_4_OR_GREATER
            out long offsetToExtendedVariableLengthRecords,
#endif
            out ushort pointDataLength,
            out long endOfPointDataRecords)
    {
        var headerReader = new HeaderBlockReader(stream);
        var header = headerReader.GetHeaderBlock(fileSignature ?? headerReader.GetFileSignature());
#if LAS1_4_OR_GREATER
        var (variableLengthRecords, extendedVariableLengthRecords) = Initialize(stream, headerReader, header, out rawReader, out offsetToPointData, out offsetToVariableLengthRecords, out offsetToExtendedVariableLengthRecords, out pointDataLength, out endOfPointDataRecords);
        return (header, variableLengthRecords, extendedVariableLengthRecords);
#else
        var variableLengthRecords = Initialize(stream, headerReader, header, out rawReader, out offsetToPointData, out offsetToVariableLengthRecords, out pointDataLength, out endOfPointDataRecords);
        return (header, variableLengthRecords);
#endif
    }

    private static
#if LAS1_4_OR_GREATER
        (VariableLengthRecord[] VariableLengthRecords, ExtendedVariableLengthRecord[] ExtendedVariableLengthRecords)
#else
        VariableLengthRecord[]
#endif
        Initialize(
            Stream stream,
            HeaderBlockReader headerReader,
            in HeaderBlock header,
            out Readers.IPointDataRecordReader rawReader,
            out uint offsetToPointData,
            out long offsetToVariableLengthRecords,
#if LAS1_4_OR_GREATER
            out long offsetToExtendedVariableLengthRecords,
#endif
            out ushort pointDataLength,
            out long endOfPointDataRecords)
    {
        offsetToPointData = headerReader.OffsetToPointData;
        pointDataLength = headerReader.PointDataLength;
        var variableLengthRecords = new VariableLengthRecord[headerReader.VariableLengthRecordCount];
        _ = headerReader.MoveToVariableLengthRecords();
        offsetToVariableLengthRecords = stream.Position;
        for (var i = 0; i < headerReader.VariableLengthRecordCount; i++)
        {
            var record = headerReader.GetVariableLengthRecord();
            variableLengthRecords[i] = record;
        }

#if LAS1_4_OR_GREATER
        var extendedVariableLengthRecords = new ExtendedVariableLengthRecord[headerReader.ExtendedVariableLengthRecordCount];
        if (headerReader.ExtendedVariableLengthRecordCount is not 0)
        {
            _ = headerReader.MoveToExtendedVariableLengthRecords();
            offsetToExtendedVariableLengthRecords = stream.Position;
            for (var i = 0; i < headerReader.ExtendedVariableLengthRecordCount; i++)
            {
                extendedVariableLengthRecords[i] = headerReader.GetExtendedVariableLengthRecord(variableLengthRecords);
            }
        }
        else
        {
            offsetToExtendedVariableLengthRecords = default;
        }
#endif
        endOfPointDataRecords = headerReader.GetEndOfPointDataRecords();

        // move to the start
        if (stream.CanSeek)
        {
            stream.Position = offsetToPointData;
        }

        rawReader = header switch
        {
            { PointDataFormatId: PointDataRecord.Id, Version: { Major: 1, Minor: >= 0 and < 5 } } => new Readers.Raw.PointDataRecordReader(),
            { PointDataFormatId: GpsPointDataRecord.Id, Version: { Major: 1, Minor: >= 0 and < 5 } } => new Readers.Raw.GpsPointDataRecordReader(),
#if LAS1_2_OR_GREATER
            { PointDataFormatId: ColorPointDataRecord.Id, Version: { Major: 1, Minor: >= 2 and < 5 } } => new Readers.Raw.ColorPointDataRecordReader(),
            { PointDataFormatId: GpsColorPointDataRecord.Id, Version: { Major: 1, Minor: >= 2 and < 5 } } => new Readers.Raw.GpsColorPointDataRecordReader(),
#endif
#if LAS1_3_OR_GREATER
            { PointDataFormatId: GpsWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 3 and < 5 } } => new Readers.Raw.GpsWaveformPointDataRecordReader(),
            { PointDataFormatId: GpsColorWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 3 and < 5 } } => new Readers.Raw.GpsColorWaveformPointDataRecordReader(),
#endif
#if LAS1_4_OR_GREATER
            { PointDataFormatId: ExtendedGpsPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => new Readers.Raw.ExtendedGpsPointDataRecordReader(),
            { PointDataFormatId: ExtendedGpsColorPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => new Readers.Raw.ExtendedGpsColorPointDataRecordReader(),
            { PointDataFormatId: ExtendedGpsColorNearInfraredPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => new Readers.Raw.ExtendedGpsColorNearInfraredPointDataRecordReader(),
            { PointDataFormatId: ExtendedGpsWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => new Readers.Raw.ExtendedGpsWaveformPointDataRecordReader(),
            { PointDataFormatId: ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => new Readers.Raw.ExtendedGpsColorNearInfraredWaveformPointDataRecordReader(),
#endif
            { Version: { Major: 1, Minor: <= 1 } } => throw new InvalidOperationException(Properties.v1_1.Resources.OnlyDataPointsAreAllowed),
#if LAS1_2_OR_GREATER
            { Version: { Major: 1, Minor: 2 } } => throw new InvalidOperationException(Properties.v1_2.Resources.OnlyDataPointsAreAllowed),
#endif
#if LAS1_3_OR_GREATER
            { Version: { Major: 1, Minor: 3 } } => throw new InvalidOperationException(Properties.v1_3.Resources.OnlyDataPointsAreAllowed),
#endif
#if LAS1_4_OR_GREATER
            { Version: { Major: 1, Minor: 4 } } => throw new InvalidOperationException(Properties.v1_4.Resources.OnlyDataPointsAreAllowed),
#endif
#if LAS1_5_OR_GREATER
            { Version: { Major: 1, Minor: 5 } } => throw new InvalidOperationException(Properties.v1_5.Resources.OnlyDataPointsAreAllowed),
#endif
            _ => throw new InvalidCastException(),
        };

#if LAS1_4_OR_GREATER
        return (variableLengthRecords, extendedVariableLengthRecords);
#else
        return variableLengthRecords;
#endif
    }

    private static Stream CreateStream(string path) => path switch
    {
        not null when File.Exists(path) => File.OpenRead(path),
        not null when Directory.Exists(path) => LasMultipleFileStream.OpenRead(path),
        _ => throw new NotSupportedException(),
    };
}