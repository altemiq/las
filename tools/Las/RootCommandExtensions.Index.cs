// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.Index.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// The <c>index</c> extensions.
/// </content>
internal static partial class RootCommandExtensions
{
    /// <summary>
    /// Adds the <c>index</c> command.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>The input builder.</returns>
    public static T AddIndex<T>(this T command)
        where T : RootCommand
    {
#if LAS1_4_OR_GREATER
        VariableLengthRecordProcessor.Instance.RegisterIndexing();
#endif

        command.Add(CreateCommand());

        return command;

        static Command CreateCommand()
        {
            var command = new Command("index", Tool.Properties.Resources.Command_IndexDescription)
            {
                CreateDump(),
                CreateCheck(),
                Arguments.Inputs,
                Options.Indexing.TileSize,
                Options.Indexing.MaximumIntervals,
                Options.Indexing.MinimumPoints,
                Options.Indexing.Threshold,
#if LAS1_4_OR_GREATER
                Options.Indexing.Append,
#endif
            };

            command.SetAction(static parseResult => Index.Processor.Process(
                parseResult.GetServices(),
                parseResult.GetRequiredValue(Arguments.Inputs),
                parseResult.GetValue(Options.Indexing.TileSize),
                parseResult.GetValue(Options.Indexing.MaximumIntervals),
                parseResult.GetValue(Options.Indexing.MinimumPoints),
#if LAS1_4_OR_GREATER
                parseResult.GetValue(Options.Indexing.Threshold),
                parseResult.GetValue(Options.Indexing.Append)));
#else
                parseResult.GetValue(Options.Indexing.Threshold)));
#endif

            return command;

            static Command CreateDump()
            {
                var command = new Command("dump", Tool.Properties.Resources.Command_IndexDumpDescription)
                {
                    Arguments.Input,
                };

                command.SetAction(
                    static parseResult =>
                    {
                        var services = parseResult.GetServices();
                        var output = parseResult.InvocationConfiguration.Output;
                        var input = parseResult.GetRequiredValue(Arguments.Input);

#if LAS1_4_OR_GREATER
                        var index = Path.GetExtension(input) is ".las" or ".laz"
                            ? CreateFromReader(input, services)
                            : CreateFromLax(Path.ChangeExtension(input, ".lax"), services);
#else
                        var lax = Path.ChangeExtension(input, ".lax");
                        var index = Path.Exists(lax)
                            ? CreateFromLax(lax, services)
                            : throw new FileNotFoundException(null, lax);
#endif

                        foreach (var item in index)
                        {
                            PrintTree(output, item);
                        }

                        static void PrintTree(TextWriter output, Indexing.LasIndexCell cell)
                        {
                            output.Write(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"({cell.Minimum.X},{cell.Minimum.Y})-({cell.Maximum.X},{cell.Maximum.Y})"));
                            output.Write(": ");
                            output.WriteLine(string.Join(',', cell.Intervals));
                        }

#if LAS1_4_OR_GREATER
                        static Indexing.LasIndex CreateFromReader(Uri file, IServiceProvider? services)
                        {
                            using var reader = new LasReader(File.OpenRead(file, services), leaveOpen: false);
                            return Index.IndexingExtensions.ReadIndex(reader);
                        }
#endif

                        static Indexing.LasIndex CreateFromLax(Uri file, IServiceProvider? services)
                        {
                            using var stream = File.OpenRead(file, services);
                            return Indexing.LasIndex.ReadFrom(stream);
                        }
                    });

                return command;
            }

            static Command CreateCheck()
            {
                var indexOption = new Option<Uri>("--index") { Description = Tool.Properties.Resources.Option_IndexCheckIndexDescription, CustomParser = System.CommandLine.Parsing.UriParser.Parse };

                var command = new Command("check", Tool.Properties.Resources.Command_IndexCheckDescription)
                {
                    Arguments.Input,
                    indexOption,
                };

                command.SetAction(
                    parseResult =>
                    {
                        var services = parseResult.GetServices();
                        var error = parseResult.InvocationConfiguration.Error;
                        var input = parseResult.GetRequiredValue(Arguments.Input);
                        var indexInput = parseResult.GetRequiredValue(indexOption);

                        Indexing.LasIndex? readIndex = default;
                        if (Path.Exists(indexInput, services))
                        {
                            readIndex = Path.GetExtension(indexInput) switch
                            {
#if LAS1_4_OR_GREATER
                                ".las" or ".laz" => CreateFromReader(input, services),
#endif
                                ".lax" => CreateFromLax(input, services),
#if LAS1_4_OR_GREATER
                                _ => throw new InvalidOperationException("Extension must be 'las', 'laz', or 'lax'"),
#else
                                _ => throw new InvalidOperationException("Extension must be 'lax'"),
#endif
                            };
                        }

                        if (readIndex is null)
                        {
                            error.WriteLine("Failed to read index from either LAX file or Compressed TAG");
                            return;
                        }

                        Indexing.LasIndex createdIndex;
                        using (var reader = new LasReader(File.OpenRead(input, services)))
                        {
                            createdIndex = Indexing.LasIndex.Create(reader);
                        }

                        using var createdEnumerator = createdIndex.OrderBy(x => x.Minimum.X).ThenBy(x => x.Minimum.Y).GetEnumerator();
                        using var readEnumerator = readIndex.OrderBy(x => x.Minimum.X).ThenBy(x => x.Minimum.Y).GetEnumerator();

                        while (true)
                        {
                            if (createdEnumerator.MoveNext())
                            {
                                if (readEnumerator.MoveNext())
                                {
                                    // both moved, check the values
                                    var created = createdEnumerator.Current;
                                    var read = readEnumerator.Current;

                                    if (!created.Equals(read))
                                    {
                                        error.WriteLine($"Cells do not equal, {created} vs {read}");
                                        return;
                                    }

                                    continue;
                                }

                                error.WriteLine("More cells in created index");
                                return;
                            }

                            if (!readEnumerator.MoveNext())
                            {
                                continue;
                            }

                            error.WriteLine("More cells in read index");
                            return;
                        }
                    });

                return command;
            }

#if LAS1_4_OR_GREATER
            static Indexing.LasIndex CreateFromReader(Uri file, IServiceProvider? services)
            {
                using var reader = new LasReader(File.OpenRead(file, services), leaveOpen: false);
                return Index.IndexingExtensions.ReadIndex(reader);
            }
#endif

            static Indexing.LasIndex CreateFromLax(Uri file, IServiceProvider? services)
            {
                using var stream = File.OpenRead(file, services);
                return Indexing.LasIndex.ReadFrom(stream);
            }
        }
    }
}