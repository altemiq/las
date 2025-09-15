// -----------------------------------------------------------------------
// <copyright file="CopcExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace Altemiq.IO.Las;
#pragma warning restore IDE0130, CheckNamespace

/// <summary>
/// Cloud optimized extension methods.
/// </summary>
public static class CopcExtensions
{
    /// <summary>
    /// Gets a value indicating whether the specified variable length record is for cloud optimization.
    /// </summary>
    /// <param name="record">The variable length record.</param>
    /// <returns><see langword="true"/> is <paramref name="record"/> is for compression; otherwise <see langword="false"/>.</returns>
    /// <remarks>This calls <see cref="CompressionExtensions.IsForCompression(VariableLengthRecord)"/> as COPC files must be compressed.</remarks>
    public static bool IsForCloudOptimization(this VariableLengthRecord record) => record.IsForCompression() || record is Cloud.CopcInfo;

    /// <summary>
    /// Gets a value indicating whether the specified extended variable length record is for cloud optimization.
    /// </summary>
    /// <param name="record">The extended variable length record.</param>
    /// <returns><see langword="true"/> is <paramref name="record"/> is for compression; otherwise <see langword="false"/>.</returns>
    /// <remarks>This calls <see cref="CompressionExtensions.IsForCompression(ExtendedVariableLengthRecord)"/> as COPC files must be compressed.</remarks>
    public static bool IsForCloudOptimization(this ExtendedVariableLengthRecord record) => record.IsForCompression() || record is Cloud.CopcHierarchy;

    /// <summary>
    /// Gets a value indicating whether the specified reader is cloud optimized.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns><see langword="true"/> if the reader is cloud optimized.</returns>
    public static bool IsCloudOptimized(this LasReader reader) => reader is LazReader laz && laz.IsCloudOptimized();

    /// <summary>
    /// Gets a value indicating whether the specified reader is cloud optimized.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns><see langword="true"/> if the reader is cloud optimized.</returns>
    public static bool IsCloudOptimized(this LazReader reader) => reader.VariableLengthRecords.Any(static vlr => vlr is Cloud.CopcInfo);

    /// <summary>
    /// Registers cloud optimized VLRs.
    /// </summary>
    /// <param name="processor">The VLR processor.</param>
    public static void RegisterCloudOptimized(this VariableLengthRecordProcessor processor)
    {
        processor.Register(Cloud.CopcConstants.UserId, Cloud.CopcInfo.TagRecordId, ProcessCopcInfo);
        processor.Register(Cloud.CopcConstants.UserId, Cloud.CopcHierarchy.TagRecordId, ProcessCopcHierarchy);
    }

    /// <summary>
    /// Registers cloud optimized VLRs.
    /// </summary>
    /// <param name="processor">The VLR processor.</param>
    /// <returns><see langword="true" /> when the compression processors are successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processors, in which case nothing gets added.</returns>
    public static bool TryRegisterCloudOptimized(this VariableLengthRecordProcessor processor) => processor.TryRegister(Cloud.CopcConstants.UserId, Cloud.CopcInfo.TagRecordId, ProcessCopcInfo)
        && processor.TryRegister(Cloud.CopcConstants.UserId, Cloud.CopcHierarchy.TagRecordId, ProcessCopcHierarchy);

    /// <summary>
    /// Moves to the specified entry.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="entry">The COPC entry.</param>
    public static void MoveToEntry(this LazReader reader, in Cloud.CopcHierarchy.Entry entry) => reader.MoveToChunk((long)entry.Offset);

    /// <summary>
    /// Reads the entry.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="entry">The COPC entry.</param>
    /// <returns>The points that are in <paramref name="entry"/>.</returns>
    public static IEnumerable<LasPointMemory> ReadPointDataRecords(this LazReader reader, Cloud.CopcHierarchy.Entry entry)
    {
        reader.MoveToEntry(entry);
        var count = entry.PointCount;
        while (count > 0)
        {
            var span = reader.ReadPointDataRecord();
            yield return new(span.PointDataRecord!, span.ExtraBytes.ToArray());
            count--;
        }
    }

    /// <summary>
    /// Reads the point data records for the specified resolution.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="resolution">The requested resolution.</param>
    /// <returns>The points that are in <paramref name="resolution"/>.</returns>
    public static IEnumerable<LasPointMemory> ReadPointDataRecords(this LazReader reader, double resolution)
    {
        var info = reader.VariableLengthRecords.OfType<Cloud.CopcInfo>().Single();
        var hierarchy = reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single();
        var maximumDepth = info.GetDepthAtResolution(hierarchy, resolution);
        return ReadPointDataRecords(reader, hierarchy, maximumDepth);
    }

    /// <summary>
    /// Reads the point data records for the specified resolution.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="maximumDepth">The maximum depth.</param>
    /// <returns>The points that are in <paramref name="maximumDepth"/>.</returns>
    public static IEnumerable<LasPointMemory> ReadPointDataRecords(this LazReader reader, int maximumDepth) => ReadPointDataRecords(reader, reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single(), maximumDepth);

    /// <summary>
    /// Reads the point data records for the specified resolution.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="box">The bounding box.</param>
    /// <param name="resolution">The requested resolution.</param>
    /// <returns>The points that are in <paramref name="box"/> and <paramref name="resolution"/>.</returns>
    public static IEnumerable<LasPointMemory> ReadPointDataRecords(this LazReader reader, BoundingBox box, double resolution)
    {
        var info = reader.VariableLengthRecords.OfType<Cloud.CopcInfo>().Single();
        var hierarchy = reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single();
        return ReadPointDataRecords(reader, box, hierarchy, info.GetDepthAtResolution(hierarchy, resolution));
    }

    /// <summary>
    /// Reads the point data records for the specified resolution.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="box">The bounding box.</param>
    /// <param name="maximumDepth">The maximum depth.</param>
    /// <returns>The points that are in <paramref name="box"/> and <paramref name="maximumDepth"/>.</returns>
    public static IEnumerable<LasPointMemory> ReadPointDataRecords(this LazReader reader, BoundingBox box, int maximumDepth) => ReadPointDataRecords(reader, box, reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single(), maximumDepth);

    /// <summary>
    /// Gets the depth at the specified resolution.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="resolution">The specified resolution.</param>
    /// <returns>The depth at <paramref name="resolution"/>.</returns>
    public static int GetDepthAtResolution(this LazReader reader, double resolution)
    {
        var info = reader.VariableLengthRecords.OfType<Cloud.CopcInfo>().Single();
        var hierarchy = reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single();
        return GetDepthAtResolution(info, hierarchy, resolution);
    }

    /// <summary>
    /// Gets the depth at the specified resolution.
    /// </summary>
    /// <param name="info">The information.</param>
    /// <param name="hierarchy">The hierarchy.</param>
    /// <param name="resolution">The specified resolution.</param>
    /// <returns>The depth at <paramref name="resolution"/>.</returns>
    public static int GetDepthAtResolution(this Cloud.CopcInfo info, Cloud.CopcHierarchy hierarchy, double resolution)
    {
        // Compute max depth
        var maximumDepth = GetAllChildrenOfPage(hierarchy, hierarchy.Root).Max(static e => e.Key.Level);

        if (resolution <= 0)
        {
            return maximumDepth;
        }

        var currentResolution = info.Spacing;

        for (var i = 0; i < maximumDepth; i++)
        {
            if (currentResolution <= resolution)
            {
                return i;
            }

            currentResolution /= 2;
        }

        return maximumDepth;
    }

    private static IEnumerable<LasPointMemory> ReadPointDataRecords(this LazReader reader, Cloud.CopcHierarchy hierarchy, int maximumDepth) => hierarchy
        .GetAllEntries()
        .Where(entry => entry.Key.Level <= maximumDepth)
        .OrderBy(static entry => entry.Offset)
        .SelectMany(reader.ReadPointDataRecords);

    private static IEnumerable<LasPointMemory> ReadPointDataRecords(LazReader reader, BoundingBox box, Cloud.CopcHierarchy hierarchy, int maximumDepth)
    {
        var header = reader.Header;
        var quantizer = new PointDataRecordQuantizer(header);

        return hierarchy
            .GetAllEntries()
            .Where(entry => entry.Key.Level <= maximumDepth)
            .OrderBy(static entry => entry.Offset)
            .SelectMany(entry =>
            {
                var keyBox = Cloud.VoxelKeyExtensions.ToBoundingBox(entry.Key, header);
                if (box.Contains(keyBox))
                {
                    // If the node is within the box add all points
                    return reader.ReadPointDataRecords(entry);
                }

                // If the node only crosses the box then get subset of points within box
                return box.IntersectsWith(keyBox)
                    ? reader.ReadPointDataRecords(entry).Where(point =>
                    {
                        var (x, y, z) = quantizer.Get(point.PointDataRecord!);
                        return box.Contains(x, y, z);
                    })
                    : [];
            });
    }

    private static Cloud.CopcInfo ProcessCopcInfo(VariableLengthRecordHeader header, ReadOnlySpan<byte> data) => new(header, data);

    private static Cloud.CopcHierarchy ProcessCopcHierarchy(ExtendedVariableLengthRecordHeader header, IEnumerable<VariableLengthRecord> variableLengthRecords, long position, ReadOnlySpan<byte> data) => new(header, variableLengthRecords.OfType<Cloud.CopcInfo>().Single(), (ulong)position, data);

    private static IEnumerable<Cloud.CopcHierarchy.Entry> GetAllEntries(this Cloud.CopcHierarchy hierarchy) => LoadPageHierarchy(hierarchy, hierarchy.Root);

    private static IEnumerable<Cloud.CopcHierarchy.Entry> GetAllChildrenOfPage(Cloud.CopcHierarchy hierarchy, Cloud.CopcHierarchy.Page page) => LoadPageHierarchy(hierarchy, page);

    private static IEnumerable<Cloud.CopcHierarchy.Entry> LoadPageHierarchy(Cloud.CopcHierarchy hierarchy, Cloud.CopcHierarchy.Page page)
    {
        foreach (var entry in page)
        {
            if (entry.PointCount is -1)
            {
                foreach (var e in LoadPageHierarchy(hierarchy, hierarchy.GetPage(entry)))
                {
                    yield return e;
                }
            }
            else
            {
                yield return entry;
            }
        }
    }
}