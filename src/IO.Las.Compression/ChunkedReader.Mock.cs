// -----------------------------------------------------------------------
// <copyright file="ChunkedReader.Mock.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if DEBUG

namespace Altemiq.IO.Las;

/// <content>
/// The mock components.
/// </content>
internal partial class ChunkedReader
{
    /// <summary>
    /// Creates a mock <see cref="ChunkedReader"/> for testing.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="headerBlock">The header block.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>The mock <see cref="ChunkReader"/>.</returns>
    public static ChunkedReader CreateMock(Readers.Compressed.ICompressedPointDataRecordReader reader, in HeaderBlock headerBlock, uint chunkSize) => new MockChunkedReader(reader, in headerBlock, chunkSize);

    private sealed class MockChunkedReader : ChunkedReader
    {
        public MockChunkedReader(Readers.Compressed.ICompressedPointDataRecordReader reader, in HeaderBlock headerBlock, uint chunkSize)
            : base(
                new MockChunkReader(
                    reader,
                    headerBlock,
#if LAS1_4_OR_GREATER
                    new(headerBlock.PointDataFormat, 0, Compressor.PointWiseChunked),
#else
                    new(headerBlock.PointDataFormat, Compressor.PointWiseChunked),
#endif
                    GpsPointDataRecord.Size),
                chunkSize) => this.chunkCount = 0;
    }

    private sealed class MockChunkReader : ChunkReader
    {
        public MockChunkReader(Readers.Compressed.ICompressedPointDataRecordReader rawReader, in HeaderBlock header, LasZip zip, int pointDataLength)
            : base(rawReader, in header, zip, pointDataLength)
        {
#if NET8_0_OR_GREATER
            ref var reader = ref ReaderField(this);
            reader = rawReader;
#else
            SetReaderField(this, rawReader);
#endif
        }

#if NET8_0_OR_GREATER
        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "reader")]
        private static extern ref Readers.Compressed.ICompressedPointDataRecordReader ReaderField(PointWiseReader reader);
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Checked")]
        private static void SetReaderField(PointWiseReader reader, Readers.Compressed.ICompressedPointDataRecordReader rawReader)
        {
            if (typeof(PointWiseReader).GetField("reader", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) is { } field)
            {
                field.SetValue(reader, rawReader);
            }
        }
#endif
    }
}

#endif