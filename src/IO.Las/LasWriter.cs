// -----------------------------------------------------------------------
// <copyright file="LasWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The LAS writer.
/// </summary>
public class LasWriter : ILasWriter, IDisposable
{
#if LAS1_4_OR_GREATER
    /// <summary>
    /// The minimum version to be able to write extended variable length records.
    /// </summary>
    protected static readonly Version EvlrVersion = new(1, 4);
#endif

    private readonly bool leaveOpen;

    private byte[] buffer = [];

#if LAS1_4_OR_GREATER
    private bool canWriteExtendedVariableLengthRecords;

    private long startOfExtendedVariableLengthRecords;

    private int numberOfExtendedVariableLengthRecords;
#endif

    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasWriter"/> class based on the specified stream, and optionally leaves the stream open.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LasWriter"/> object is disposed; otherwise <see langword="false"/>.</param>
    public LasWriter(Stream stream, bool leaveOpen = false)
    {
        this.BaseStream = stream;
        this.leaveOpen = leaveOpen;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasWriter"/> class based on the reader, and optionally leaves the stream open.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LasWriter"/> object is disposed; otherwise <see langword="false"/>.</param>
    public LasWriter(LasReader reader, bool leaveOpen = false)
#if LAS1_4_OR_GREATER
        : this(
              reader.BaseStream,
              reader.Header.Version >= EvlrVersion ? reader.ExtendedVariableLengthRecords.Count : -1,
              leaveOpen)
#else
        : this(
              reader.BaseStream,
              leaveOpen)
#endif
    {
        if (reader.GetType() != typeof(LasReader))
        {
            throw new NotSupportedException();
        }
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Initializes a new instance of the <see cref="LasWriter"/> class based on the specified stream and character encoding, and optionally leaves the stream open.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="numberOfExtendedVariableLengthRecords">The number of extended variable length records.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LasWriter"/> object is disposed; otherwise <see langword="false"/>.</param>
    protected LasWriter(Stream stream, int numberOfExtendedVariableLengthRecords, bool leaveOpen = false)
        : this(stream, leaveOpen)
    {
        this.canWriteExtendedVariableLengthRecords = numberOfExtendedVariableLengthRecords >= 0;
        this.numberOfExtendedVariableLengthRecords = this.canWriteExtendedVariableLengthRecords
            ? numberOfExtendedVariableLengthRecords
            : default;
    }
#endif

    /// <summary>
    /// Gets the RAW writer.
    /// </summary>
    protected Writers.IPointDataRecordWriter RawWriter { get; } = new Writers.Raw.PointDataRecordWriter();

    /// <summary>
    /// Gets the base stream.
    /// </summary>
    protected Stream BaseStream { get; }

    /// <inheritdoc/>
    public virtual void Write(in HeaderBlock header, params IEnumerable<VariableLengthRecord> records)
    {
        var recordsList = records as IReadOnlyList<VariableLengthRecord> ?? records.ToList();

#if LAS1_4_OR_GREATER
        // check for extra bytes to increase the point record size by
        var extraByteCount = recordsList.OfType<ExtraBytes>().FirstOrDefault() switch
        {
            { } extraBytes => extraBytes.GetByteCount(),
            _ => ushort.MinValue,
        };
#endif

        HeaderBlockValidator.Instance.Validate(header, recordsList);

        // offset to point data
        var recordSize = recordsList.Aggregate(0u, (current, record) => current + record.Size());
        var pointDataRecordSize =
#if LAS1_4_OR_GREATER
            this.WriteHeader(header, (uint)recordsList.Count, recordSize, extraByteCount);
#else
            this.WriteHeader(header, (uint)recordsList.Count, recordSize);
#endif
        Array.Resize(ref this.buffer, pointDataRecordSize);

        if (recordSize is not 0)
        {
            byte[] byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(0);
            foreach (var record in recordsList)
            {
                var size = record.Size();
                if (byteArray.Length < size)
                {
                    System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);
                    byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(size);
                }

                var written = record.Write(byteArray);
                this.BaseStream.Write(byteArray, 0, written);
            }

            System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);
        }
    }

    /// <inheritdoc />
    public virtual void Write(IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(record);
#else
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record));
        }
#endif

        var written = this.RawWriter.Write(this.buffer, record);
        extraBytes.CopyTo(this.buffer.AsSpan(written));
        written += extraBytes.Length;
        this.BaseStream.Write(this.buffer, 0, written);
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Writes the extended variable length record.
    /// </summary>
    /// <param name="record">The extended variable length record value.</param>
    public virtual void Write(ExtendedVariableLengthRecord record) => this.WriteExtendedVariableLengthRecord(record);
#endif

    /// <inheritdoc/>
    public async virtual ValueTask WriteAsync(IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(record);
#else
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record));
        }
#endif

        var written = await this.RawWriter.WriteAsync(this.buffer, record, cancellationToken).ConfigureAwait(false);
        extraBytes.CopyTo(this.buffer.AsMemory(written));
        written += extraBytes.Length;
        await this.BaseStream.WriteAsync(this.buffer, 0, written, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

#if NETSTANDARD2_0_OR_GREATER || NETFRAMEWORK || NET
    /// <inheritdoc cref="Stream.Close" />
    public virtual void Close() => this.BaseStream.Close();
#endif

    /// <inheritdoc cref="Stream.Flush" />
    public virtual void Flush() => this.BaseStream.Flush();

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Writes the extended variable length record.
    /// </summary>
    /// <param name="record">The extended variable length record.</param>
    /// <returns>The position at which <paramref name="record"/> was written.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Cannot write <see cref="ExtendedVariableLengthRecord"/> values to this instance.</exception>
    protected long WriteExtendedVariableLengthRecord(ExtendedVariableLengthRecord record)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(record);
#else
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record));
        }
#endif

        // if we can't seek, then we can't write out the EVLR information
        if (!this.canWriteExtendedVariableLengthRecords)
        {
            // invalid LAS version
            throw new InvalidOperationException(Properties.v1_4.Resources.InvalidLASVersionForEVLRWriting);
        }

        var startPosition = this.BaseStream.Position;
        var size = (int)record.Size();

        var byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(size);

        record.Write(byteArray);

        this.BaseStream.Write(byteArray, 0, size);

        if (!this.BaseStream.CanSeek)
        {
            // just put this into the registers
            if (this.numberOfExtendedVariableLengthRecords is 0)
            {
                this.startOfExtendedVariableLengthRecords = startPosition;
            }

            this.numberOfExtendedVariableLengthRecords++;
            return startPosition;
        }

        var currentPosition = this.BaseStream.Position;

        // update the count
        if (this.numberOfExtendedVariableLengthRecords is 0)
        {
            // write the start value
            this.BaseStream.Position = 235;
            this.startOfExtendedVariableLengthRecords = startPosition;
            System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(byteArray, startPosition);
            this.BaseStream.Write(byteArray, 0, sizeof(long));
        }

        this.numberOfExtendedVariableLengthRecords++;
        this.BaseStream.Position = 243;
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(byteArray, this.numberOfExtendedVariableLengthRecords);
        this.BaseStream.Write(byteArray, 0, sizeof(int));

        this.BaseStream.Position = currentPosition;

        System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);

        return startPosition;
    }
#endif

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Writes the header.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="recordCount">The record count.</param>
    /// <param name="recordSize">The record size.</param>
    /// <param name="extraByteCount">The extra bytes count.</param>
    /// <returns>The point data record size.</returns>
    /// <exception cref="InvalidCastException">Invalid point data format.</exception>
    protected int WriteHeader(in HeaderBlock header, uint recordCount, uint recordSize, ushort extraByteCount)
#else
    /// <summary>
    /// Writes the header.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="recordCount">The record count.</param>
    /// <param name="recordSize">The record size.</param>
    /// <returns>The point data record size.</returns>
    /// <exception cref="InvalidCastException">Invalid point data format.</exception>
    protected int WriteHeader(in HeaderBlock header, uint recordCount, uint recordSize)
#endif
    {
#if LAS1_4_OR_GREATER
        this.canWriteExtendedVariableLengthRecords = header.Version >= EvlrVersion;
        return WriteHeaderCore(this.BaseStream, header, 0UL, extraByteCount, (ulong)this.startOfExtendedVariableLengthRecords, (uint)this.numberOfExtendedVariableLengthRecords, recordCount, recordSize);
#elif LAS1_3_OR_GREATER
        return WriteHeaderCore(this.BaseStream, header, 0UL, recordCount, recordSize);
#else
        return WriteHeaderCore(this.BaseStream, header, recordCount, recordSize);
#endif

        static int WriteHeaderCore(
            Stream stream,
            in HeaderBlock header,
#if LAS1_3_OR_GREATER
            ulong waveformDataPacketRecordPosition,
#endif
#if LAS1_4_OR_GREATER
            ushort extraByteCount,
            ulong extendedVariableLengthRecordsPosition,
            uint extendedVariableLengthRecordCount,
#endif
            uint recordCount,
            uint recordSize)
        {
            ushort size = header.Version switch
            {
                { Major: 1, Minor: 0 or 1 } => HeaderBlock.Size10,
#if LAS1_2_OR_GREATER
                { Major: 1, Minor: 2 } => HeaderBlock.Size10,
#endif
#if LAS1_3_OR_GREATER
                { Major: 1, Minor: 3 } => HeaderBlock.Size13,
#endif
#if LAS1_4_OR_GREATER
                { Major: 1, Minor: 4 } => HeaderBlock.Size14,
#endif
#if LAS1_5_OR_GREATER
                { Major: 1, Minor: 5 } => HeaderBlock.Size15,
#endif
                _ => throw new ArgumentOutOfRangeException(nameof(header)),
            };

            var byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(size);
            Span<byte> destination = byteArray;

            // write the file signature
            System.Text.Encoding.UTF8.GetBytes(header.FileSignature, destination[..4]);

            // write identification
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[4..6], header.FileSourceId);
#if LAS1_2_OR_GREATER
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[6..8], (ushort)header.GlobalEncoding);
#endif
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            _ = header.ProjectId.TryWriteBytes(destination[8..24]);
#else
            header.ProjectId.ToByteArray().CopyTo(destination[8..24]);
#endif

            // version
            destination[24] = (byte)header.Version.Major;
            destination[25] = (byte)header.Version.Minor;

            System.Text.Encoding.UTF8.GetBytes(header.SystemIdentifier, destination[26..58]);

            System.Text.Encoding.UTF8.GetBytes(header.GeneratingSoftware, destination[58..90]);

            // file creation date
            if (header.FileCreation.HasValue)
            {
                var fileCreation = header.FileCreation.Value;
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[90..92], (ushort)fileCreation.DayOfYear);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[92..94], (ushort)fileCreation.Year);
            }

            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[94..96], size);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[96..100], size + recordSize);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[100..104], recordCount);
            destination[104] = header.PointDataFormat;

            var pointDataRecordSize = header switch
            {
                { PointDataFormatId: PointDataRecord.Id, Version: { Major: 1, Minor: >= 0 and < 5 } } => PointDataRecord.Size,
                { PointDataFormatId: GpsPointDataRecord.Id, Version: { Major: 1, Minor: >= 0 and < 5 } } => GpsPointDataRecord.Size,
#if LAS1_2_OR_GREATER
                { PointDataFormatId: ColorPointDataRecord.Id, Version: { Major: 1, Minor: >= 2 and < 5 } } => ColorPointDataRecord.Size,
                { PointDataFormatId: GpsColorPointDataRecord.Id, Version: { Major: 1, Minor: >= 2 and < 5 } } => GpsColorPointDataRecord.Size,
#endif
#if LAS1_3_OR_GREATER
                { PointDataFormatId: GpsWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 3 and < 5 } } => GpsWaveformPointDataRecord.Size,
                { PointDataFormatId: GpsColorWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 3 and < 5 } } => GpsColorWaveformPointDataRecord.Size,
#endif
#if LAS1_4_OR_GREATER
                { PointDataFormatId: ExtendedGpsPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => ExtendedGpsPointDataRecord.Size,
                { PointDataFormatId: ExtendedGpsColorPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => ExtendedGpsColorPointDataRecord.Size,
                { PointDataFormatId: ExtendedGpsColorNearInfraredPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => ExtendedGpsColorNearInfraredPointDataRecord.Size,
                { PointDataFormatId: ExtendedGpsWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => ExtendedGpsWaveformPointDataRecord.Size,
                { PointDataFormatId: ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size,
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

            // add the extra bytes.
#if LAS1_4_OR_GREATER
            pointDataRecordSize += extraByteCount;
#endif
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[105..107], pointDataRecordSize);
#if LAS1_4_OR_GREATER
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[107..111], header.LegacyNumberOfPointRecords);
            for (int i = 0; i < 5; i++)
            {
                var start = 111 + (i * sizeof(uint));
                System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[start..(start + sizeof(uint))], header.LegacyNumberOfPointsByReturn[i]);
            }
#else
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[107..111], header.NumberOfPointRecords);
            for (int i = 0; i < 5; i++)
            {
                var start = 111 + (i * sizeof(uint));
                System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[start..(start + sizeof(uint))], header.NumberOfPointsByReturn[i]);
            }
#endif
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[131..139], header.ScaleFactor.X);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[139..147], header.ScaleFactor.Y);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[147..155], header.ScaleFactor.Z);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[155..163], header.Offset.X);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[163..171], header.Offset.Y);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[171..179], header.Offset.Z);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[179..187], header.Max.X);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[187..195], header.Min.X);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[195..203], header.Max.Y);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[203..211], header.Min.Y);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[211..219], header.Max.Z);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[219..227], header.Min.Z);

#if LAS1_3_OR_GREATER
            if (header.Version is { Major: 1, Minor: >= 3 })
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination[227..235], waveformDataPacketRecordPosition);
            }
#endif

#if LAS1_4_OR_GREATER
            if (header.Version is { Major: 1, Minor: >= 4 })
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination[235..243], extendedVariableLengthRecordsPosition);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[243..247], extendedVariableLengthRecordCount);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination[247..255], header.NumberOfPointRecords);
                for (int i = 0; i < 15; i++)
                {
                    var start = 255 + (i * sizeof(ulong));
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination[start..(start + sizeof(ulong))], header.NumberOfPointsByReturn[i]);
                }
            }
#endif

#if LAS1_5_OR_GREATER
            if (header.Version is { Major: 1, Minor: >= 5 })
            {
                System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[375..383], header.MaxGpsTime);
                System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[383..391], header.MinGpsTime);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[391..393], header.TimeOffset);
            }
#endif

            stream.Write(byteArray, 0, size);

            System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);

            return pointDataRecordSize;
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="LasWriter"/> class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing && !this.leaveOpen)
            {
                this.BaseStream.Dispose();
            }

            this.disposedValue = true;
        }
    }
}