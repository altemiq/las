// -----------------------------------------------------------------------
// <copyright file="Processor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The information processor.
/// </summary>
internal static class Processor
{
    /// <summary>
    /// Processes the specified file.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="console">The console.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <param name="noMinMax">No min/max calculate.</param>
    /// <param name="noReturns">No returns.</param>
    /// <param name="json">Output JSON.</param>
    /// <param name="boundingBox">The bounding box.</param>
    public static void Process(Stream stream, IAnsiConsole console, IFormatProvider formatProvider, bool noMinMax, bool noReturns, bool json, BoundingBox? boundingBox)
    {
        using var reader = LazReader.Create(stream);
        if (json)
        {
            // writer
            var writer = new System.Text.Json.Utf8JsonWriter(
                new ConsoleStream(console),
                new()
                {
                    Indented = true,
                    IndentCharacter = ' ',
                    IndentSize = 2,
                    NewLine = "\r\n",
                });

            writer.WriteStartObject();

            writer.WriteStartArray("lasinfo");

            writer.WriteStartObject();

            writer.WriteString("las_json_version", "1.0");

            if (stream is FileStream fs)
            {
                writer.WriteString("input_file_name", fs.Name);
            }

            var formatter = new JsonLasReaderFormatter(writer);
            formatter.Format(reader, noMinMax, noReturns, boundingBox);

            writer.WriteEndObject();

            writer.WriteEndArray();

            writer.WriteEndObject();

            writer.Flush();
        }
        else
        {
            if (stream is FileStream fs)
            {
                console.WriteLine($"las info report for '{fs.Name}'", AnsiConsoleStyles.Title);
            }

            var formatter = new DefaultLasReaderFormatter(new ConsoleFormatBuilder(console, new LasFormatProvider(formatProvider, reader)));
            formatter.Format(reader, noMinMax, noReturns, boundingBox);
        }
    }

    private sealed class ConsoleStream(IAnsiConsole console) : Stream
    {
        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => int.MaxValue;

        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => console.Write(System.Text.Encoding.UTF8.GetString(buffer, offset, count));
    }
}