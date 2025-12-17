// -----------------------------------------------------------------------
// <copyright file="PointWiseWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The compressed point writer.
/// </summary>
internal class PointWiseWriter : RawWriter
{
    private readonly Writers.IPointDataRecordWriter compressedWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PointWiseWriter"/> class.
    /// </summary>
    /// <param name="rawWriter">The raw writer.</param>
    /// <param name="pointDataLength">The point data length.</param>
    /// <param name="pointDataFormatId">The point data format ID.</param>
    /// <param name="zip">The zip information.</param>
    public PointWiseWriter(Writers.IPointDataRecordWriter rawWriter, int pointDataLength, byte pointDataFormatId, LasZip zip)
        : base(rawWriter, pointDataLength)
    {
        if (zip is { Compressor: Compressor.None } or { Items.Count: 0 })
        {
            throw new NotSupportedException();
        }

        this.Encoder = zip.Coder switch
        {
            Coder.Arithmetic => new(),
            _ => throw new NotSupportedException(),
        };

        var extraBytes = zip.Items.GetExtraBytesCount();
#if LAS1_5_OR_GREATER
        var requiredVersion = zip.Items.GetPointVersion();
#endif

        this.compressedWriter = pointDataFormatId switch
        {
            PointDataRecord.Id => new Writers.Compressed.PointDataRecordWriter(this.Encoder, extraBytes),
            GpsPointDataRecord.Id => new Writers.Compressed.GpsPointDataRecordWriter(this.Encoder, extraBytes),
#if LAS1_2_OR_GREATER
            ColorPointDataRecord.Id => new Writers.Compressed.ColorPointDataRecordWriter(this.Encoder, extraBytes),
            GpsColorPointDataRecord.Id => new Writers.Compressed.GpsColorPointDataRecordWriter(this.Encoder, extraBytes),
#endif
#if LAS1_3_OR_GREATER
            GpsWaveformPointDataRecord.Id => new Writers.Compressed.GpsWaveformPointDataRecordWriter(this.Encoder, extraBytes),
            GpsColorWaveformPointDataRecord.Id => new Writers.Compressed.GpsColorWaveformPointDataRecordWriter(this.Encoder, extraBytes),
#endif
#if LAS1_5_OR_GREATER
            ExtendedGpsPointDataRecord.Id when requiredVersion is 4 => new Writers.Compressed.ExtendedGpsPointDataRecordWriter4(this.Encoder, extraBytes),
            ExtendedGpsColorPointDataRecord.Id when requiredVersion is 4 => new Writers.Compressed.ExtendedGpsColorPointDataRecordWriter4(this.Encoder, extraBytes),
            ExtendedGpsColorNearInfraredPointDataRecord.Id when requiredVersion is 4 => new Writers.Compressed.ExtendedGpsColorNearInfraredPointDataRecordWriter4(this.Encoder, extraBytes),
            ExtendedGpsWaveformPointDataRecord.Id when requiredVersion is 4 => new Writers.Compressed.ExtendedGpsWaveformPointDataRecordWriter4(this.Encoder, extraBytes),
            ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id when requiredVersion is 4 => new Writers.Compressed.ExtendedGpsColorNearInfraredWaveformPointDataRecordWriter4(this.Encoder, extraBytes),
#endif
#if LAS1_4_OR_GREATER
            ExtendedGpsPointDataRecord.Id => new Writers.Compressed.ExtendedGpsPointDataRecordWriter3(this.Encoder, extraBytes),
            ExtendedGpsColorPointDataRecord.Id => new Writers.Compressed.ExtendedGpsColorPointDataRecordWriter3(this.Encoder, extraBytes),
            ExtendedGpsColorNearInfraredPointDataRecord.Id => new Writers.Compressed.ExtendedGpsColorNearInfraredPointDataRecordWriter3(this.Encoder, extraBytes),
            ExtendedGpsWaveformPointDataRecord.Id => new Writers.Compressed.ExtendedGpsWaveformPointDataRecordWriter3(this.Encoder, extraBytes),
            ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id => new Writers.Compressed.ExtendedGpsColorNearInfraredWaveformPointDataRecordWriter3(this.Encoder, extraBytes),
#endif
#if LAS1_5_OR_GREATER
            _ => throw new InvalidOperationException(Properties.v1_5.Resources.OnlyDataPointsAreAllowed),
#elif LAS1_4_OR_GREATER
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
    /// Gets the writer.
    /// </summary>
    protected Writers.IPointDataRecordWriter? Writer { get; private set; }

    /// <summary>
    /// Gets the encoder.
    /// </summary>
    protected ArithmeticEncoder Encoder { get; }

    /// <inheritdoc/>
    public override void Write(Stream stream, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes)
    {
        if (this.Writer is null)
        {
            base.Write(stream, record, extraBytes);

            Span<byte> bytes = stackalloc byte[100];
            var bytesWritten = record.CopyTo(bytes);
            extraBytes.CopyTo(bytes[bytesWritten..]);
            bytesWritten += extraBytes.Length;

            switch (this.compressedWriter)
            {
#if LAS1_4_OR_GREATER
                case Writers.Compressed.IContext context:
                    var cxt = default(uint);
                    _ = context.Initialize(bytes[..bytesWritten], ref cxt);
                    break;
#endif
                case Writers.Compressed.ISimple simple:
                    _ = simple.Initialize(bytes[..bytesWritten]);
                    break;
            }

            this.Writer = this.compressedWriter;
            _ = this.Encoder.Initialize(stream);
        }
        else
        {
            this.Write(stream, this.Writer, record, extraBytes);
        }
    }

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(Stream stream, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default)
    {
        if (this.Writer is null)
        {
            await base.WriteAsync(stream, record, extraBytes, cancellationToken).ConfigureAwait(false);

            Span<byte> bytes = stackalloc byte[100];
            var bytesWritten = record.CopyTo(bytes);
            extraBytes.Span.CopyTo(bytes[bytesWritten..]);
            bytesWritten += extraBytes.Length;

            switch (this.compressedWriter)
            {
#if LAS1_4_OR_GREATER
                case Writers.Compressed.IContext context:
                    var cxt = default(uint);
                    _ = context.Initialize(bytes[..bytesWritten], ref cxt);
                    break;
#endif
                case Writers.Compressed.ISimple simple:
                    _ = simple.Initialize(bytes[..bytesWritten]);
                    break;
            }

            this.Writer = this.compressedWriter;
            _ = this.Encoder.Initialize(stream);
        }
        else
        {
            await this.WriteAsync(stream, this.Writer, record, extraBytes, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override void Initialize(Stream stream)
    {
        base.Initialize(stream);
        this.Writer = default;
    }
}