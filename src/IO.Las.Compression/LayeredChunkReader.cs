// -----------------------------------------------------------------------
// <copyright file="LayeredChunkReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Compressor.LayeredChunked"/> chunk reader.
/// </summary>
internal sealed class LayeredChunkReader : ChunkReader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredChunkReader"/> class.
    /// </summary>
    /// <inheritdoc cref="ChunkReader"/>
    public LayeredChunkReader(Readers.IPointDataRecordReader rawReader, in HeaderBlock header, LasZip zip)
        : base(rawReader, header, zip)
    {
        if (zip.Compressor is not Compressor.LayeredChunked)
        {
            throw new NotSupportedException();
        }
    }

    /// <inheritdoc/>
    protected override void InitializeCompression(Stream stream, IBasePointDataRecord point, ReadOnlySpan<byte> extraBytes, Readers.IPointDataRecordReader recordReader)
    {
        // for layered compression 'decoder' only hands over the stream
        _ = this.Decoder.Initialize(stream, reallyInit: false);

        // read how many points are in the chunk
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        Span<byte> buffer = stackalloc byte[4];
        _ = stream.Read(buffer);
#else
        var buffer = new byte[4];
        _ = stream.Read(buffer, 0, 4);
#endif

        InitializeCompression(point, extraBytes, recordReader);
    }
}