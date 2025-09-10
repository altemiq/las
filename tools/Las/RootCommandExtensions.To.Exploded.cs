// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.To.Exploded.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// The <c>to exploded</c> extensions.
/// </content>
internal static partial class RootCommandExtensions
{
    /// <summary>
    /// Adds the <c>to exploded</c> command.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>The input builder.</returns>
    public static T AddToExploded<T>(this T command)
        where T : Command
    {
        command.Add(CreateCommand());

        return command;

        static Command CreateCommand()
        {
            var output = new Option<Uri>("-o", "--output") { Required = true };

            var command = new Command("exploded", Tool.Properties.Resources.Command_ToExplodedDescription)
            {
                Arguments.Input,
                output,
            };

            command.SetAction(parseResult => Process(parseResult.GetRequiredValue(Arguments.Input), parseResult.GetValue(output)));

            return command;

            static void Process(Uri input, Uri? output)
            {
                using var stream = File.OpenRead(input);
                output ??= input;

                Exploded(
                    stream,
                    name =>
                    {
                        var builder = new UriBuilder(output);
                        builder.Path = builder.Path + '/' + name;
                        return File.OpenWrite(builder.Uri);
                    });
            }
        }
    }

    private static void Exploded(Stream input, Func<string, Stream> segmentOutputFunc)
    {
        var segments = GetSegmentLocations(input).ToArray();

        input.Position = 0;

        for (var i = 0; i < segments.Length - 1; i++)
        {
            var (name, position) = segments[i];
            var length = segments[i + 1].Position - position;
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent((int)System.Math.Min(81920L, length));

            using var outputStream = segmentOutputFunc(name + LasMultipleFileStream.Extension);
            var totalBytes = default(long);
            while (totalBytes < length)
            {
                var currentBytesToRead = (int)Math.Min(buffer.Length, length - totalBytes);

                var bytesRead = input.Read(buffer, 0, currentBytesToRead);

                // put this to the output stream
                outputStream.Write(buffer, 0, bytesRead);

                totalBytes += bytesRead;
            }

            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }

        static IEnumerable<(string Name, long Position)> GetSegmentLocations(Stream stream)
        {
            // this outputs the start of each part of the LAS file
            yield return (LasStreams.Header, 0);

            // read the header size, i.e. the variable length records
            stream.Position = 94;
            var headerSize = stream.ReadUInt16LittleEndian();
            yield return (LasStreams.VariableLengthRecord, headerSize);

            // read the offset to point data
            stream.Position = 96;
            var pointData = stream.ReadUInt32LittleEndian();
            var numberOfVariableLengthRecords = stream.ReadUInt32LittleEndian();
            var pointDataFormat = stream.ReadByteLittleEndian();

#if LAS1_4_OR_GREATER
            // the extended variable length records
            var extendedVariableLengthRecord = default(long);
            if (headerSize > HeaderBlock.Size13)
            {
                stream.Position = 235;
                var position = stream.ReadUInt64LittleEndian();
                if (position is not 0)
                {
                    extendedVariableLengthRecord = (long)position;
                }
            }
#endif

            var chunkTable = default(long);
            if (pointDataFormat.IsCompressed())
            {
                // lots more work to do

                // move to the end of the header
                stream.Position = headerSize;

                var headerReader = new HeaderBlockReader(stream);

                // lots more work to do, need to read the VLRs
                CompressedTag? compressedTag = default;
                for (var i = 0; i < numberOfVariableLengthRecords; i++)
                {
                    if (headerReader.GetVariableLengthRecord() is CompressedTag tempCompressedTag)
                    {
                        compressedTag = tempCompressedTag;
                    }
                }

                if (compressedTag is { Compressor: not Compressor.None and not Compressor.PointWise })
                {
                    // this is chunked, so read the chunk table
                    yield return (LazStreams.ChunkTablePosition, stream.Position);

                    chunkTable = stream.ReadInt64LittleEndian();

                    var chunksStart = stream.Position;
                    pointData = default;

                    stream.Position = chunkTable;

                    var decoder = compressedTag.Coder switch
                    {
                        Coder.Arithmetic => new Compression.ArithmeticDecoder(),
                        _ => throw new NotSupportedException(),
                    };

                    var (chunkStarts, numberChunks, _, _) = ChunkedReader.ReadChunkTable(
                        stream,
                        chunksStart,
#if LAS1_4_OR_GREATER
                        readChunkTotals: compressedTag is { Compressor: Compressor.LayeredChunked, ChunkSize: CompressedTag.VariableChunkSize },
#else
                        readChunkTotals: false,
#endif
                        decoder);

                    for (var i = 0; i < numberChunks; i++)
                    {
                        yield return (LazStreams.FormatChunk(i), chunkStarts[i]);
                    }
                }
            }

            if (pointData > 0)
            {
                yield return (LasStreams.PointData, pointData);
            }

            if (chunkTable > 0)
            {
                yield return (LazStreams.ChunkTable, chunkTable);
            }

#if LAS1_4_OR_GREATER
            if (extendedVariableLengthRecord > 0)
            {
                yield return (LasStreams.ExtendedVariableLengthRecord, extendedVariableLengthRecord);
            }
#endif

            // return the end of the stream
            yield return (string.Empty, stream.Length);
        }
    }
}