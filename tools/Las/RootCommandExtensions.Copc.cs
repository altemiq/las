﻿// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.Copc.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// The <c>copc</c> extensions.
/// </content>
internal static partial class RootCommandExtensions
{
    /// <summary>
    /// Adds the <c>copc</c> command.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>The input builder.</returns>
    public static T AddCloudOptimized<T>(this T command)
        where T : Command
    {
        VariableLengthRecordProcessor.Instance.RegisterCloudOptimized();

        command.Add(CreateCommand());

        return command;

        static Command CreateCommand()
        {
            return new("copc", Tool.Properties.v1_4.Resources.Command_CopcDescription)
            {
                CreateVerify(),
            };

            static Command CreateVerify()
            {
                var dumpOption = new Option<bool>("--dump", "-d") { Description = Tool.Properties.v1_4.Resources.Option_CopcVerifyDumpDescription };
                var chunksOption = new Option<bool>("--chunks", "-c") { Description = Tool.Properties.v1_4.Resources.Option_CopcVerifyChunksDescription };

                var command = new Command("verify", Tool.Properties.v1_4.Resources.Command_CopcVerifyDescription)
                {
                    Arguments.Input,
                    dumpOption,
                    chunksOption,
                };

                command.SetAction(
                    parseResult =>
                    {
                        var file = parseResult.GetRequiredValue(Arguments.Input);
                        var dump = parseResult.GetValue(dumpOption);
                        var chunks = parseResult.GetValue(chunksOption);
                        var output = parseResult.InvocationConfiguration.Output;
                        var error = parseResult.InvocationConfiguration.Error;

                        using var reader = new LazReader(file.LocalPath);
                        var info = reader.VariableLengthRecords.OfType<Cloud.CopcInfo>().Single();

                        if (dump)
                        {
                            DumpCopcVlr(output, info);
                            DumpHeader(output, reader);

                            static void DumpCopcVlr(TextWriter output, Cloud.CopcInfo copcVlr)
                            {
                                output.WriteLine("COPC VLR:");
                                output.WriteLine("\tCenter X Y Z: {0} {1} {2}", copcVlr.CentreX, copcVlr.CentreY, copcVlr.CentreZ);
                                output.WriteLine("\tRoot node halfsize: {0}", copcVlr.HalfSize);
                                output.WriteLine("\tRoot node point spacing: {0}", copcVlr.Spacing);
                                output.WriteLine("\tGPS time min/max = {0}/{1}", copcVlr.GpsTimeMinimum, copcVlr.GpsTimeMaximum);
                                output.WriteLine();
                            }

                            static void DumpHeader(TextWriter output, LasReader reader)
                            {
                                var h = reader.Header;
                                output.WriteLine("LAS Header:");
                                output.WriteLine("\tFile source ID: {0}", h.FileSourceId);
                                output.WriteLine("\tGlobal encoding: {0}", (int)h.GlobalEncoding);
                                output.WriteLine("\t\tTime representation: {0}", h.GlobalEncoding.HasFlag(GlobalEncoding.StandardGpsTime) ? "GPS Satellite Time" : "GPS Week Time");
                                output.WriteLine("\t\tSRS Type: {0}", h.GlobalEncoding.HasFlag(GlobalEncoding.Wkt) ? "WKT" : "GeoTIFF");
                                output.WriteLine("\tVersion: {0}", h.Version);
                                output.WriteLine("\tSystem ID: {0}", h.SystemIdentifier);
                                output.WriteLine("\tSoftware ID: {0}", h.GeneratingSoftware);
                                output.WriteLine("\tCreation day/year: {0} / {1}", h.FileCreation?.DayOfYear, h.FileCreation?.Year);

                                output.WriteLine("\tHeader Size: {0}", Info.Information.GetValue(reader, "offsetToVariableLengthRecords"));
                                output.WriteLine("\tPoint Offset: {0}", Info.Information.GetValue(reader, "offsetToPointData"));
                                output.WriteLine("\tVLR Count: {0}", reader.VariableLengthRecords.Count);
                                output.WriteLine("\tEVLR Count: {0}", reader.ExtendedVariableLengthRecords.Count);

                                //// writer.WriteLine("\tEVLR Offset: {0}", h.evlr_offset);
                                output.WriteLine("\tPoint Format: {0}", h.PointDataFormatId);

                                output.WriteLine("\tPoint Length: {0}", Info.Information.GetValue(reader, "pointDataLength"));
                                output.WriteLine("\tNumber of Points old/1.4: {0} / {1}", h.LegacyNumberOfPointRecords, h.RawNumberOfPointRecords);
                                output.WriteLine("\tScale X Y Z: {0} {1} {2}", h.ScaleFactor.X, h.ScaleFactor.Y, h.ScaleFactor.Z);
                                output.WriteLine("\tOffset X Y Z: {0} {1} {2}", h.Offset.X, h.Offset.Y, h.Offset.Z);
                                output.WriteLine("\tMin X Y Z: {0} {1} {2}", h.Min.X, h.Min.Y, h.Min.Z);
                                output.WriteLine("\tMax X Y Z: {0} {1} {2}", h.Max.X, h.Max.Y, h.Max.Z);
                                output.Write("\tPoint Counts by Return:     ");
                                for (var i = 0; i < 5; ++i)
                                {
                                    output.Write(" ");
                                    output.Write(h.LegacyNumberOfPointRecords);
                                }

                                output.WriteLine();
                                output.Write("\tExt Point Counts by Return: ");
                                for (var i = 0; i < 15; ++i)
                                {
                                    output.Write(" ");
                                    output.Write(h.RawNumberOfPointsByReturn[i]);
                                }

                                output.WriteLine();
                                output.WriteLine();
                            }
                        }

                        var header = reader.Header;

                        if (header.Version.Major is not 1 || header.Version.Minor is not 4)
                        {
                            error.WriteLine("Invalid COPC file. Found version {0} instead of 1.4", header.Version);
                        }

                        if (Info.Information.GetValue(reader, "offsetToVariableLengthRecords") is int headerSize and not HeaderBlock.Size14)
                        {
                            error.WriteLine("Invalid COPC file. Found header size of {0} instead of {1}.", headerSize, HeaderBlock.Size14);
                        }

                        if (!header.IsCompressed())
                        {
                            error.WriteLine("Invalid COPC file. Compression bit (high bit) of point format ID not set.");
                        }

                        if (header.PointDataFormatId is < 6 or > 8)
                        {
                            error.WriteLine("Invalid COPC file. Point format is {0}. Should be 6, 7, or 8.", header.PointDataFormatId);
                        }

                        if (info.Header.UserId is not "copc")
                        {
                            error.WriteLine("Invalid COPC VLR header. User ID is '{0}', not 'copc'.", info.Header.UserId);
                        }

                        if (info.Header.RecordId is not 1)
                        {
                            error.WriteLine("Invalid COPC VLR header. Record ID is {0}, not 1.", info.Header.RecordId);
                        }

                        var bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(info.Size());
                        info.CopyTo(bytes);
                        for (var i = 0; i < 11; ++i)
                        {
                            if (bytes[160 - 11 + i] is not 0)
                            {
                                error.WriteLine("Invalid COPC VLR. COPC field reserved[{0}] is {1}, not 0.", i, bytes[160 - 11 + i]);
                            }
                        }

                        System.Buffers.ArrayPool<byte>.Shared.Return(bytes);

                        var (totals, counts, ranges) = TraverseTree(output, error, reader, reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single(), chunks);
                        if (dump)
                        {
                            output.WriteLine("Points per level:");
                            var sum = 0UL;
                            foreach (var p in totals)
                            {
                                sum += p.Value;
                                output.WriteLine("  {0}: {1} ({2})", p.Key, p.Value, p.Value / counts[p.Key]);
                            }

                            output.WriteLine("Total of all levels: {0}", sum);
                            output.WriteLine();
                        }

                        VerifyRanges(error, reader.Header, info, ranges);

                        static (IDictionary<int, ulong> Totals, IDictionary<int, ulong> Counts, (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, double MinGps, double MaxGps) Ranges) TraverseTree(TextWriter output, TextWriter error, LazReader reader, Cloud.CopcHierarchy hierarchy, bool dumpChunks)
                        {
                            var page = hierarchy.Root;
                            var positiveEntries = new List<Cloud.CopcHierarchy.Entry>();

                            positiveEntries.AddRange(page.Where(static e => e.PointCount >= 0));

                            var totals = new Dictionary<int, ulong>();
                            var counts = new Dictionary<int, ulong>();

                            VerifyPage(error, page, positiveEntries);
                            var ranges = (double.MaxValue, double.MinValue, double.MaxValue, double.MinValue, double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);
                            var entries = ProcessPage(reader, page, totals, counts, out var temp).ToList();
                            ranges = Combine(ranges, temp);
                            while (entries.Count > 0)
                            {
                                var e = entries[0];
                                entries.RemoveAt(0);

                                page = hierarchy.GetPage(e);
                                if (e.PointCount >= 0)
                                {
                                    positiveEntries.Add(e);
                                }

                                VerifyPage(error, page, positiveEntries);
                                entries.AddRange(ProcessPage(reader, page, totals, counts, out temp));
                                ranges = Combine(ranges, temp);
                            }

                            if (dumpChunks)
                            {
                                output.WriteLine("Chunks:");
                                output.WriteLine("\tKey:     Offest / Count / Total Count");
                                long total = 0;

                                foreach (var e in positiveEntries.OrderBy(static e => e.Offset))
                                {
                                    total += e.PointCount;
                                    output.WriteLine(
                                        "\t{0}-{1}-{2}-{3}: {4} / {5} / {6}",
                                        e.Key.Level,
                                        e.Key.X,
                                        e.Key.Y,
                                        e.Key.Z,
                                        e.Offset,
                                        e.PointCount,
                                        total);
                                }

                                output.WriteLine();
                            }

                            return (totals, counts, ranges);

                            static (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, double MinGps, double MaxGps) Combine((double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, double MinGps, double MaxGps) first, (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, double MinGps, double MaxGps) second)
                            {
                                return (
                                    Math.Min(first.MinX, second.MinX),
                                    Math.Max(first.MaxX, second.MaxX),
                                    Math.Min(first.MinY, second.MinY),
                                    Math.Max(first.MaxY, second.MaxY),
                                    Math.Min(first.MinZ, second.MinZ),
                                    Math.Max(first.MaxZ, second.MaxZ),
                                    Math.Min(first.MinGps, second.MinGps),
                                    Math.Max(first.MaxGps, second.MaxGps));
                            }

                            static void VerifyPage(TextWriter error, Cloud.CopcHierarchy.Page page, IReadOnlyCollection<Cloud.CopcHierarchy.Entry> all)
                            {
                                foreach (var key in page.Select(static e => e.Key))
                                {
                                    var parent = Cloud.VoxelKeyExtensions.Parent(key);
                                    if (parent.Level < 0)
                                    {
                                        continue;
                                    }

                                    if (!KeyExists(parent, all))
                                    {
                                        error.WriteLine("Hierarchy entry {0} has no parent in existing hierarchy.", key);
                                    }
                                }

                                static bool KeyExists(in Cloud.CopcHierarchy.VoxelKey k, IEnumerable<Cloud.CopcHierarchy.Entry> all)
                                {
                                    foreach (var e in all)
                                    {
                                        if (e.Key == k)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }
                            }

                            static IEnumerable<Cloud.CopcHierarchy.Entry> ProcessPage(
                                LazReader reader,
                                Cloud.CopcHierarchy.Page page,
                                IDictionary<int, ulong> totals,
                                IDictionary<int, ulong> counts,
                                out (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, double MinGps, double MaxGps) ranges)
                            {
                                var children = new List<Cloud.CopcHierarchy.Entry>();
                                var pageMinX = double.MaxValue;
                                var pageMaxX = double.MinValue;
                                var pageMinY = double.MaxValue;
                                var pageMaxY = double.MinValue;
                                var pageMinZ = double.MaxValue;
                                var pageMaxZ = double.MinValue;
                                var pageMinGps = double.MaxValue;
                                var pageMaxGps = double.MinValue;

                                foreach (var entry in page)
                                {
                                    if (entry.PointCount is -1)
                                    {
                                        children.Add(entry);
                                    }
                                    else
                                    {
                                        if (totals.TryGetValue(entry.Key.Level, out var total))
                                        {
                                            totals[entry.Key.Level] = total + (ulong)entry.PointCount;
                                        }
                                        else
                                        {
                                            totals.Add(entry.Key.Level, (ulong)entry.PointCount);
                                        }

                                        if (counts.TryGetValue(entry.Key.Level, out var count))
                                        {
                                            counts[entry.Key.Level] = count + 1;
                                        }
                                        else
                                        {
                                            counts.Add(entry.Key.Level, 1);
                                        }

                                        var (dataMinX, dataMaxX, dataMinY, dataMaxY, dataMinZ, dataMaxZ, dataMinGps, dataMaxGps) = ReadData(reader, entry);

                                        pageMinX = Math.Min(dataMinX, pageMinX);
                                        pageMaxX = Math.Max(dataMaxX, pageMaxX);
                                        pageMinY = Math.Min(dataMinY, pageMinY);
                                        pageMaxY = Math.Max(dataMaxY, pageMaxY);
                                        pageMinZ = Math.Min(dataMinZ, pageMinZ);
                                        pageMaxZ = Math.Max(dataMaxZ, pageMaxZ);
                                        pageMinGps = Math.Min(dataMinGps, pageMinGps);
                                        pageMaxGps = Math.Max(dataMaxGps, pageMaxGps);
                                    }
                                }

                                ranges = (pageMinX, pageMaxX, pageMinY, pageMaxY, pageMinZ, pageMaxZ, pageMinGps, pageMaxGps);
                                return children;
                            }
                        }

                        static void VerifyRanges(TextWriter error, in HeaderBlock headerBlock, Cloud.CopcInfo copcVlr, (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, double MinGps, double MaxGps) ranges)
                        {
                            if (!CloseEnough(ranges.MinX, headerBlock.Min.X, .0000001))
                            {
                                error.WriteLine("Minimum X value of {0} doesn't match header minimum of {1}.", ranges.MinX, headerBlock.Min.X);
                            }

                            if (!CloseEnough(ranges.MaxX, headerBlock.Max.X, .0000001))
                            {
                                error.WriteLine("Maximum X value of {0} doesn't match header maximum of {1}", ranges.MaxX, headerBlock.Max.X);
                            }

                            if (!CloseEnough(ranges.MinY, headerBlock.Min.Y, .0000001))
                            {
                                error.WriteLine("Minimum X value of {0} doesn't match header minimum of {1}.", ranges.MinY, headerBlock.Min.Y);
                            }

                            if (!CloseEnough(ranges.MaxY, headerBlock.Max.Y, .0000001))
                            {
                                error.WriteLine("Maximum X value of {0} doesn't match header maximum of {1}", ranges.MaxY, headerBlock.Max.Y);
                            }

                            if (!CloseEnough(ranges.MinZ, headerBlock.Min.Z, .0000001))
                            {
                                error.WriteLine("Minimum X value of {0} doesn't match header minimum of {1}.", ranges.MinZ, headerBlock.Min.Z);
                            }

                            if (!CloseEnough(ranges.MaxZ, headerBlock.Max.Z, .0000001))
                            {
                                error.WriteLine("Maximum X value of {0} doesn't match header maximum of {1}", ranges.MaxZ, headerBlock.Max.Z);
                            }

                            if (!CloseEnough(ranges.MinGps, copcVlr.GpsTimeMinimum, .0000001))
                            {
                                error.WriteLine("Minimum GPS time value of {0} doesn't match COPC VLR minimum of {1}", ranges.MinGps, copcVlr.GpsTimeMinimum);
                            }

                            if (!CloseEnough(ranges.MaxGps, copcVlr.GpsTimeMaximum, .0000001))
                            {
                                error.WriteLine("Maximum GPS time value of {0} doesn't match COPC VLR maximum of {1}", ranges.MaxGps, copcVlr.GpsTimeMaximum);
                            }

                            static bool CloseEnough(double a, double b, double epsilon)
                            {
                                return Math.Abs(a - b) <= ((Math.Abs(a) < Math.Abs(b) ? Math.Abs(b) : Math.Abs(a)) * epsilon);
                            }
                        }

                        static (double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, double MinGps, double MaxGps) ReadData(LazReader reader, in Cloud.CopcHierarchy.Entry entry)
                        {
                            reader.MoveToEntry(entry);
                            var count = entry.PointCount;
                            var minX = double.MaxValue;
                            var maxX = double.MinValue;
                            var minY = double.MaxValue;
                            var maxY = double.MinValue;
                            var minZ = double.MaxValue;
                            var maxZ = double.MinValue;
                            var minGps = double.MaxValue;
                            var maxGps = double.MinValue;
                            var header = reader.Header;
                            var quantizer = new PointDataRecordQuantizer(header);

                            while (count is not 0)
                            {
                                if (reader.ReadPointDataRecord() is { PointDataRecord: IExtendedPointDataRecord point })
                                {
                                    var (x, y, z) = quantizer.Get(point);
                                    var gps = point.GpsTime;

                                    minX = Math.Min(x, minX);
                                    maxX = Math.Max(x, maxX);
                                    minY = Math.Min(y, minY);
                                    maxY = Math.Max(y, maxY);
                                    minZ = Math.Min(z, minZ);
                                    maxZ = Math.Max(z, maxZ);
                                    minGps = Math.Min(gps, minGps);
                                    maxGps = Math.Max(gps, maxGps);
                                }

                                count--;
                            }

                            return (minX, maxX, minY, maxY, minZ, maxZ, minGps, maxGps);
                        }
                    });

                return command;
            }
        }
    }
}