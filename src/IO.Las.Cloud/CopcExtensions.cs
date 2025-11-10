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
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "False positive")]
public static class CopcExtensions
{
    /// <summary>
    /// The <see cref="LazReader"/> extensions
    /// </summary>
    extension(LazReader reader)
    {
        /// <summary>
        /// Gets a value indicating whether the specified reader is cloud optimized.
        /// </summary>
        /// <returns><see langword="true"/> if the reader is cloud optimized.</returns>
        public bool IsCloudOptimized() => reader.VariableLengthRecords.Any(static vlr => vlr is Cloud.CopcInfo);

        /// <summary>
        /// Moves to the specified entry.
        /// </summary>
        /// <param name="entry">The COPC entry.</param>
        public void MoveToEntry(in Cloud.CopcHierarchy.Entry entry) => reader.MoveToChunk((long)entry.Offset);

        /// <summary>
        /// Reads the entry.
        /// </summary>
        /// <param name="entry">The COPC entry.</param>
        /// <returns>The points that are in <paramref name="entry"/>.</returns>
        public IEnumerable<LasPointMemory> ReadPointDataRecords(Cloud.CopcHierarchy.Entry entry)
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
        /// <param name="resolution">The requested resolution.</param>
        /// <returns>The points that are in <paramref name="resolution"/>.</returns>
        public IEnumerable<LasPointMemory> ReadPointDataRecords(double resolution)
        {
            var info = reader.VariableLengthRecords.OfType<Cloud.CopcInfo>().Single();
            var hierarchy = reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single();
            var maximumDepth = info.GetDepthAtResolution(hierarchy, resolution);
            return LazReader.ReadPointDataRecords(reader, hierarchy, maximumDepth);
        }

        /// <summary>
        /// Reads the point data records for the specified resolution.
        /// </summary>
        /// <param name="maximumDepth">The maximum depth.</param>
        /// <returns>The points that are in <paramref name="maximumDepth"/>.</returns>
        public IEnumerable<LasPointMemory> ReadPointDataRecords(int maximumDepth) => LazReader.ReadPointDataRecords(reader, reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single(), maximumDepth);

        /// <summary>
        /// Reads the point data records for the specified resolution.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <param name="resolution">The requested resolution.</param>
        /// <returns>The points that are in <paramref name="box"/> and <paramref name="resolution"/>.</returns>
        public IEnumerable<LasPointMemory> ReadPointDataRecords(BoundingBox box, double resolution)
        {
            var info = reader.VariableLengthRecords.OfType<Cloud.CopcInfo>().Single();
            var hierarchy = reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single();
            return LazReader.ReadPointDataRecords(reader, box, hierarchy, info.GetDepthAtResolution(hierarchy, resolution));
        }

        /// <summary>
        /// Reads the point data records for the specified resolution.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <param name="maximumDepth">The maximum depth.</param>
        /// <returns>The points that are in <paramref name="box"/> and <paramref name="maximumDepth"/>.</returns>
        public IEnumerable<LasPointMemory> ReadPointDataRecords(BoundingBox box, int maximumDepth) => LazReader.ReadPointDataRecords(reader, box, reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single(), maximumDepth);

        /// <summary>
        /// Gets the depth at the specified resolution.
        /// </summary>
        /// <param name="resolution">The specified resolution.</param>
        /// <returns>The depth at <paramref name="resolution"/>.</returns>
        public int GetDepthAtResolution(double resolution)
        {
            var info = reader.VariableLengthRecords.OfType<Cloud.CopcInfo>().Single();
            var hierarchy = reader.ExtendedVariableLengthRecords.OfType<Cloud.CopcHierarchy>().Single();
            return info.GetDepthAtResolution(hierarchy, resolution);
        }

        private static IEnumerable<LasPointMemory> ReadPointDataRecords(LazReader lazReader, Cloud.CopcHierarchy hierarchy, int maximumDepth) => hierarchy
            .GetAllEntries()
            .Where(entry => entry.Key.Level <= maximumDepth)
            .OrderBy(static entry => entry.Offset)
            .SelectMany(lazReader.ReadPointDataRecords);

        private static IEnumerable<LasPointMemory> ReadPointDataRecords(LazReader lazReader, BoundingBox box, Cloud.CopcHierarchy hierarchy, int maximumDepth)
        {
            var header = lazReader.Header;
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
                        return lazReader.ReadPointDataRecords(entry);
                    }

                    // If the node only crosses the box then get subset of points within box
                    return box.IntersectsWith(keyBox)
                        ? lazReader.ReadPointDataRecords(entry).Where(point =>
                        {
                            var (x, y, z) = quantizer.Get(point.PointDataRecord!);
                            return box.Contains(x, y, z);
                        })
                        : [];
                });
        }
    }

    /// <summary>
    /// The <see cref="VariableLengthRecordProcessor"/> extensions
    /// </summary>
    extension(VariableLengthRecordProcessor processor)
    {
        /// <summary>
        /// Registers cloud optimized VLRs.
        /// </summary>
        public void RegisterCloudOptimized()
        {
            processor.Register(Cloud.CopcConstants.UserId, Cloud.CopcInfo.TagRecordId, VariableLengthRecordProcessor.ProcessCopcInfo);
            processor.Register(Cloud.CopcConstants.UserId, Cloud.CopcHierarchy.TagRecordId, VariableLengthRecordProcessor.ProcessCopcHierarchy);
        }

        /// <summary>
        /// Registers cloud optimized VLRs.
        /// </summary>
        /// <returns><see langword="true" /> when the compression processors are successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processors, in which case nothing gets added.</returns>
        public bool TryRegisterCloudOptimized() => processor.TryRegister(Cloud.CopcConstants.UserId, Cloud.CopcInfo.TagRecordId, VariableLengthRecordProcessor.ProcessCopcInfo)
                                                   && processor.TryRegister(Cloud.CopcConstants.UserId, Cloud.CopcHierarchy.TagRecordId, VariableLengthRecordProcessor.ProcessCopcHierarchy);

        private static Cloud.CopcInfo ProcessCopcInfo(VariableLengthRecordHeader header, ReadOnlySpan<byte> data) => new(header, data);

        private static Cloud.CopcHierarchy ProcessCopcHierarchy(ExtendedVariableLengthRecordHeader header, IEnumerable<VariableLengthRecord> variableLengthRecords, long position, ReadOnlySpan<byte> data) => new(header, variableLengthRecords.OfType<Cloud.CopcInfo>().Single(), (ulong)position, data);
    }

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

        static IEnumerable<Cloud.CopcHierarchy.Entry> GetAllChildrenOfPage(Cloud.CopcHierarchy hierarchy, Cloud.CopcHierarchy.Page page)
        {
            return LoadPageHierarchy(hierarchy, page);
        }
    }

    private static IEnumerable<Cloud.CopcHierarchy.Entry> GetAllEntries(this Cloud.CopcHierarchy hierarchy) => LoadPageHierarchy(hierarchy, hierarchy.Root);

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