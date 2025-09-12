// -----------------------------------------------------------------------
// <copyright file="PointWiseReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Compressor.PointWise"/> <see cref="IPointReader"/>.
/// </summary>
internal class PointWiseReader : RawReader
{
    private readonly Readers.IPointDataRecordReader compressedReader;

    private Readers.IPointDataRecordReader? reader;

    /// <summary>
    /// Initializes a new instance of the <see cref="PointWiseReader"/> class.
    /// </summary>
    /// <param name="rawReader">The raw reader.</param>
    /// <param name="header">The header block.</param>
    /// <param name="zip">The zip information.</param>
    /// <param name="pointDataLength">The point data length.</param>
    /// <param name="pointStart">The point start.</param>
    public PointWiseReader(Readers.IPointDataRecordReader rawReader, in HeaderBlock header, LasZip zip, int pointDataLength, long pointStart)
        : base(rawReader, pointDataLength, pointStart)
    {
        if (zip is { Compressor: Compressor.None } or { Items.Count: 0 })
        {
            throw new NotSupportedException();
        }

        this.Decoder = zip.Coder switch
        {
            Coder.Arithmetic => new ArithmeticDecoder(),
            _ => throw new NotSupportedException(),
        };

        var extraBytes = zip.Items.GetExtraBytesCount();

        this.compressedReader = header.PointDataFormatId switch
        {
            PointDataRecord.Id => new Readers.Compressed.PointDataRecordReader(this.Decoder, extraBytes),
            GpsPointDataRecord.Id => new Readers.Compressed.GpsPointDataRecordReader(this.Decoder, extraBytes),
#if LAS1_2_OR_GREATER
            ColorPointDataRecord.Id => new Readers.Compressed.ColorPointDataRecordReader(this.Decoder, extraBytes),
            GpsColorPointDataRecord.Id => new Readers.Compressed.GpsColorPointDataRecordReader(this.Decoder, extraBytes),
#endif
#if LAS1_3_OR_GREATER
            GpsWaveformPointDataRecord.Id => new Readers.Compressed.GpsWaveformPointDataRecordReader(this.Decoder, extraBytes),
            GpsColorWaveformPointDataRecord.Id => new Readers.Compressed.GpsColorPointDataRecordReader(this.Decoder, extraBytes),
#endif
#if LAS1_4_OR_GREATER
            ExtendedGpsPointDataRecord.Id => new Readers.Compressed.ExtendedGpsPointDataRecordReader(this.Decoder, extraBytes),
            ExtendedGpsColorPointDataRecord.Id => new Readers.Compressed.ExtendedGpsColorPointDataRecordReader(this.Decoder, extraBytes),
            ExtendedGpsColorNearInfraredPointDataRecord.Id => new Readers.Compressed.ExtendedGpsColorNearInfraredPointDataRecordReader(this.Decoder, extraBytes),
            ExtendedGpsWaveformPointDataRecord.Id => new Readers.Compressed.ExtendedGpsWaveformPointDataRecordReader(this.Decoder, extraBytes),
            ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id => new Readers.Compressed.ExtendedGpsColorNearInfraredWaveformPointDataRecordReader(this.Decoder, extraBytes),
#endif
#if LAS1_4_OR_GREATER
            _ => throw new InvalidOperationException(Properties.v1_4.Resources.OnlyDataPointsAreAllowed),
#elif LAS1_3_OR_GREATER
            _ => throw new InvalidOperationException(Properties.v1_3.Resources.OnlyDataPointsAreAllowed),
#elif LAS1_2_OR_GREATER
            _ => throw new InvalidOperationException(Properties.v1_2.Resources.OnlyDataPointsAreAllowed),
#else
            _ => throw new InvalidOperationException(Properties.v1_1.Resources.OnlyDataPointsAreAllowed),
#endif
        };
    }

    /// <summary>
    /// Gets the decoder.
    /// </summary>
    public IEntropyDecoder Decoder { get; }

    /// <inheritdoc/>
    public sealed override void Close(Stream stream) => this.Decoder.Done();

    /// <inheritdoc/>
    public sealed override LasPointSpan Read(Stream stream)
    {
        if (this.reader is not null)
        {
            return this.reader.Read(default);
        }

        var point = base.Read(stream);

        this.InitializeCompression(stream, point.PointDataRecord!, point.ExtraBytes, this.compressedReader);

        this.reader = this.compressedReader;

        return point;
    }

    /// <inheritdoc/>
    public sealed override async ValueTask<LasPointMemory> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (this.reader is not null)
        {
            return await this.reader.ReadAsync(default, cancellationToken).ConfigureAwait(false);
        }

        var point = await base.ReadAsync(stream, cancellationToken).ConfigureAwait(false);

        this.InitializeCompression(stream, point.PointDataRecord!, point.ExtraBytes.Span, this.compressedReader);

        this.reader = this.compressedReader;

        return point;
    }

    /// <summary>
    /// Initializes the decoder.
    /// </summary>
    /// <returns><see langword="true"/> if the decoder is initialized; otherwise <see langword="false"/>.</returns>
    public bool InitializeDecoder()
    {
        this.reader = default;
        return true;
    }

    /// <inheritdoc/>
    public sealed override bool MoveToPoint(Stream stream, ulong current, ulong target)
    {
        if (!stream.CanSeek)
        {
            return false;
        }

        var delta = target - current;
        if (current > target)
        {
            this.Decoder.Done();
            stream.MoveToPositionAbsolute(this.PointStart);
            _ = this.InitializeDecoder();
            delta = target;
        }

        while (delta > 0)
        {
            _ = this.Read(stream);
            delta--;
        }

        return true;
    }

    /// <inheritdoc/>
    public sealed override async ValueTask<bool> MoveToPointAsync(Stream stream, ulong current, ulong target, CancellationToken cancellationToken = default)
    {
        if (!stream.CanSeek)
        {
            return false;
        }

        var delta = target - current;
        if (current > target)
        {
            this.Decoder.Done();
            stream.MoveToPositionAbsolute(this.PointStart);
            _ = this.InitializeDecoder();
            delta = target;
        }

        while (delta > 0)
        {
            _ = await this.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
            delta--;
        }

        return true;
    }

    /// <summary>
    /// Called after the first raw point is read.
    /// </summary>
    /// <param name="point">The raw point.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="recordReader">The compressed reader.</param>
    protected static void InitializeCompression(IBasePointDataRecord point, ReadOnlySpan<byte> extraBytes, Readers.IPointDataRecordReader recordReader)
    {
        Span<byte> bytes = stackalloc byte[100];

        var bytesWritten = point.CopyTo(bytes);
        extraBytes.CopyTo(bytes[bytesWritten..]);
        bytesWritten += extraBytes.Length;

        switch (recordReader)
        {
#if LAS1_4_OR_GREATER
            case Readers.Compressed.IContext context:
                _ = context.ChunkSizes();
                var ctx = default(uint);
                _ = context.Initialize(bytes[..bytesWritten], ref ctx);
                break;
#endif
            case Readers.Compressed.ISimple simple:
                _ = simple.Initialize(bytes[..bytesWritten]);
                break;
        }
    }

    /// <summary>
    /// Called after the first raw point is read.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="point">The raw point.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="recordReader">The compressed reader.</param>
    protected virtual void InitializeCompression(Stream stream, IBasePointDataRecord point, ReadOnlySpan<byte> extraBytes, Readers.IPointDataRecordReader recordReader)
    {
        InitializeCompression(point, extraBytes, recordReader);
        _ = this.Decoder.Initialize(stream);
    }
}