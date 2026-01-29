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
    /// The <see cref="ILasReader"/> extensions
    /// </summary>
    extension(ILasReader reader)
    {
        /// <summary>
        /// Copies the contents of the current reader as cloud optimized points, to the specified stream.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="maximumDepth">The maximum depth.</param>
        /// <param name="minimumPointsPerOctant">The minimum number of points per octant.</param>
        /// <param name="maximumPointsPerOctant">The maximum number of points per octant.</param>
        /// <param name="occupancyResolution">The occupancy resolution.</param>
        /// <param name="rootGridSize">The root grid size.</param>
        /// <param name="probabilitySwapEvent">The probability swap event.</param>
        /// <param name="unordered">Set to <see langword="true"/> to leave points unordered.</param>
        /// <param name="bufferSize">The buffer size.</param>
        /// <param name="swap">Set to <see langword="true"/> to swap the points.</param>
        /// <param name="shuffle">Set to <see langword="true"/> to shuffle the points.</param>
        /// <param name="sort">Set to <see langword="true"/> to sort the points.</param>
        public void CopyToCloudOptimized(
            Stream output,
            int maximumDepth = -1,
            int minimumPointsPerOctant = 100,
            ulong maximumPointsPerOctant = 100000UL,
            float occupancyResolution = 50F,
            int rootGridSize = 256,
            float probabilitySwapEvent = 0.95F,
            bool unordered = false,
            int bufferSize = 1000000,
            bool swap = true,
            bool shuffle = true,
            bool sort = true)
        {
            using var writer = new LazWriter(output);
            reader.CopyToCloudOptimized(
                writer,
                maximumDepth,
                minimumPointsPerOctant,
                maximumPointsPerOctant,
                occupancyResolution,
                rootGridSize,
                probabilitySwapEvent,
                unordered,
                bufferSize,
                swap,
                shuffle,
                sort);
        }

        /// <summary>
        /// Copies the contents of the current reader as cloud optimized points, to the specified stream.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="maximumDepth">The maximum depth.</param>
        /// <param name="minimumPointsPerOctant">The minimum number of points per octant.</param>
        /// <param name="maximumPointsPerOctant">The maximum number of points per octant.</param>
        /// <param name="occupancyResolution">The occupancy resolution.</param>
        /// <param name="rootGridSize">The root grid size.</param>
        /// <param name="probabilitySwapEvent">The probability swap event.</param>
        /// <param name="unordered">Set to <see langword="true"/> to leave points unordered.</param>
        /// <param name="bufferSize">The buffer size.</param>
        /// <param name="swap">Set to <see langword="true"/> to swap the points.</param>
        /// <param name="shuffle">Set to <see langword="true"/> to shuffle the points.</param>
        /// <param name="sort">Set to <see langword="true"/> to sort the points.</param>
        public void CopyToCloudOptimized(
            LazWriter writer,
            int maximumDepth = -1,
            int minimumPointsPerOctant = 100,
            ulong maximumPointsPerOctant = 100000UL,
            float occupancyResolution = 50F,
            int rootGridSize = 256,
            float probabilitySwapEvent = 0.95F,
            bool unordered = false,
            int bufferSize = 1000000,
            bool swap = true,
            bool shuffle = true,
            bool sort = true)
        {
            const int LimitDepth = 10;

            if (reader.Header.Version.Minor < 4)
            {
                throw new InvalidOperationException("Source must be LAS1.4 or greater");
            }

            if ((reader.Header.GlobalEncoding & GlobalEncoding.Wkt) is GlobalEncoding.None)
            {
                throw new InvalidOperationException("Source must have WKT specified");
            }

            // remove any compression tags
            var variableLengthRecords = reader.VariableLengthRecords.Where(static vlr => !vlr.IsForCompression() && !vlr.IsForCloudOptimization()).ToList();
            var header = reader.Header;
            variableLengthRecords.Add(new CompressedTag(header, variableLengthRecords, Compressor.LayeredChunked) { ChunkSize = CompressedTag.VariableChunkSize });

            var tempMaxDepth = maximumDepth < 0
                ? Math.Min(EptOctree.ComputeMaxDepth(header, maximumPointsPerOctant), LimitDepth)
                : maximumDepth;

            var quantizer = new PointDataRecordQuantizer(header);
            var finalizer = new Finalizer(header, quantizer, 2 << tempMaxDepth);
            var occupancy = new Cloud.Internals.OccupancyGrid(quantizer, occupancyResolution);

            var gpsTimeMinimum = double.MaxValue;
            var gpsTimeMaximum = double.MinValue;
            var builder = new HeaderBlockBuilder(header);
            if (builder.Version.Minor < 4)
            {
                builder.Version = new(1, 4);
            }

            builder.SetCompressed();
            builder.Reset();
            builder.ScaleFactor = header.ScaleFactor;

            while (reader.ReadPointDataRecord() is { PointDataRecord: { } pointDataRecord })
            {
                if (pointDataRecord is IGpsPointDataRecord { GpsTime: var gpsTime })
                {
                    _ = ExchangeIfLessThan(ref gpsTimeMinimum, gpsTime);
                    _ = ExchangeIfGreaterThan(ref gpsTimeMaximum, gpsTime);
                }

                finalizer.Add(pointDataRecord);
                builder.Add(pointDataRecord);
                _ = occupancy.Add(pointDataRecord);
            }

            header = builder.HeaderBlock;
            var octree = new EptOctree(header) { GridSize = rootGridSize };

            if (maximumDepth < 0)
            {
                maximumDepth = Math.Min(EptOctree.ComputeMaxDepth(header, maximumPointsPerOctant), LimitDepth);
            }

            var area = occupancyResolution * occupancyResolution * occupancy.NumOccupied;
            var density = builder.NumberOfPointRecords / area;
            var voxelSizes = octree.Size / octree.GridSize;
            var swapProbabilities = new double[LimitDepth + 1];

            for (var i = 0; i <= LimitDepth; i++)
            {
                var expectedNumPoint = (int)(voxelSizes * voxelSizes * density);
                swapProbabilities[i] = (expectedNumPoint >= 5) ? 1D - Math.Pow(1D - probabilitySwapEvent, 1D / expectedNumPoint) : 0D;
                voxelSizes /= 2;
            }

            var copcInfo = new Cloud.CopcInfo
            {
                CentreX = octree.CentreX,
                CentreY = octree.CentreY,
                CentreZ = octree.CentreZ,
                GpsTimeMinimum = gpsTimeMinimum,
                GpsTimeMaximum = gpsTimeMaximum,
                HalfSize = octree.HalfSize,
                Spacing = octree.HalfSize * 2 / octree.GridSize,
            };

            variableLengthRecords.Insert(0, copcInfo);

            writer.Write(header, variableLengthRecords);
            builder.Reset();

            var entries = ProcessPoints(
                reader,
                writer,
                builder,
                header.NumberOfPointRecords,
                maximumDepth,
                minimumPointsPerOctant,
                unordered,
                swap,
                shuffle,
                sort,
                quantizer,
                finalizer,
                octree,
                swapProbabilities,
                bufferSize);

            // write out the COPC hierarchy
            var hierarchy = new Cloud.CopcHierarchy(entries);
            copcInfo = copcInfo with { RootHierOffset = Cloud.CopcHierarchy.HeaderSize, RootHierSize = hierarchy.Header.RecordLengthAfterHeader };
            writer.Write(hierarchy, position => copcInfo = copcInfo with { RootHierOffset = copcInfo.RootHierOffset + (ulong)position });

            // write out the EVLRs
            foreach (var extendedVariableLengthRecord in reader
                         .ExtendedVariableLengthRecords
                         .Where(static extendedVariableLengthRecord => !extendedVariableLengthRecord.IsForCloudOptimization())
                         .ToList())
            {
                writer.Write(extendedVariableLengthRecord);
            }

            writer.Flush();

            writer.BaseStream.Position = 0;
            writer.Write(builder.HeaderBlock, variableLengthRecords);

            static bool ExchangeIfLessThan(ref double location1, double value)
            {
                double snapshot;
                bool stillLess;
                do
                {
                    snapshot = location1;
                    stillLess = value < snapshot;
                }
                while (stillLess && !Interlocked.CompareExchange(ref location1, value, snapshot).Equals(snapshot));

                return stillLess;
            }

            static bool ExchangeIfGreaterThan(ref double location1, double value)
            {
                double snapshot;
                bool stillMore;
                do
                {
                    snapshot = location1;
                    stillMore = value > snapshot;
                }
                while (stillMore && !Interlocked.CompareExchange(ref location1, value, snapshot).Equals(snapshot));

                return stillMore;
            }

            static List<Cloud.CopcHierarchy.Entry> ProcessPoints(
                ILasReader reader,
                LazWriter writer,
                HeaderBlockBuilder builder,
                ulong numberOfPointRecords,
                int maxDepth,
                int minPointsPerOctant,
                bool unordered,
                bool swap,
                bool shuffle,
                bool sort,
                PointDataRecordQuantizer quantizer,
                Finalizer finalizer,
                EptOctree octree,
                double[] swapProbabilities,
                int bufferSize)
            {
                var entries = new List<Cloud.CopcHierarchy.Entry>();
                var unorderedKeys = EptKey.Root.GetChildren();

                var currentUnorderedKey = unorderedKeys[0];

                var buffer = new List<LasPointMemory>(bufferSize);

                var skip = false;
                var registry = new Dictionary<EptKey, Octant>();

                byte unorderedKeyId = default;
                ushort bufferId = default;
                ulong numPointsRead = default;
                var record = reader.ReadPointDataRecord(0);
                var point = record.PointDataRecord!;
                do
                {
                    numPointsRead++;
                    var endOfStream = numPointsRead == numberOfPointRecords;

                    // Optimization for non-spatially coherent files (typically TLS). We perform 8 reads, skipping
                    // points that are not in the current region of interest.
                    if (unordered)
                    {
                        skip = octree.GetKey(quantizer.Get(point), 1) != currentUnorderedKey;
                        if (endOfStream && unorderedKeyId < (unorderedKeys.Length - 1))
                        {
                            numPointsRead = 0;
                            point = reader.ReadPointDataRecord(0).PointDataRecord!;
                            currentUnorderedKey = unorderedKeys[++unorderedKeyId];
                        }
                    }

                    if (!skip)
                    {
                        buffer.Add(new(point.Clone(), record.ExtraBytes.ToArray()));
                    }

                    if (buffer.Count == buffer.Capacity || endOfStream)
                    {
                        var random = new Random();

                        // First, we shuffle the points
                        if (shuffle)
                        {
                            for (var i = 0; i < buffer.Count; i++)
                            {
                                var j = random.Next(short.MaxValue) % buffer.Count;
                                (buffer[j], buffer[i]) = (buffer[i], buffer[j]);
                            }
                        }

                        // We put the incoming points (coming in a random order) in the octree
                        foreach (var item in buffer)
                        {
                            var lasPoint = item;
                            point = lasPoint.PointDataRecord!;
                            var vector = quantizer.Get(point);
                            finalizer.Remove(vector);

                            // Search a place to insert the point
                            var level = 0;
                            int cell;

                            Octant? octant;
                            bool accepted;
                            do
                            {
                                var key = octree.GetKey(vector, level);
                                cell = level == maxDepth
                                    ? -1 // Do not build an occupancy grid for last level. Point must be inserted anyway.
                                    : octree.GetCell(vector, key);

                                if (!registry.TryGetValue(key, out octant))
                                {
                                    // create the octant
                                    octant = [];
                                    registry.Add(key, octant);
                                }

                                accepted = !octant.TryGetVoxel(cell, out var voxel) || (level == maxDepth);

                                if (swap
                                    && !accepted
                                    && voxel.BufferId != bufferId
                                    && ((float)random.Next(short.MaxValue) / short.MaxValue) < swapProbabilities[level])
                                {
                                    // only swap if the voxel buffer ID does not equal the current buffer ID: save the heavy cost (on disk) of swapping.
                                    // No need to swap two points from the same buffer: they are already shuffled.
                                    octant.Swap(ref lasPoint, voxel.PositionId);
                                    voxel.BufferId = bufferId;
                                }

                                level++;
                            }
                            while (!accepted);

                            // insert the point
                            octant.Add(lasPoint, cell, bufferId);

                            // Check if we finalized a cell of the finalizer.
                            // We can potentially write some chunks in the .copc.laz and free up memory
                            if (!finalizer.AreAnyFinalized)
                            {
                                continue;
                            }

                            var keys = new List<EptKey>(registry.Keys);
                            foreach (var currentKey in keys)
                            {
                                var currentOctant = registry[currentKey];

                                // Bounding box of the octant
                                var res = octree.Size / (1 << currentKey.Depth);
                                var minX = (res * currentKey.X) + octree.MinX;
                                var minY = (res * currentKey.Y) + octree.MinY;
                                var minZ = (res * currentKey.Z) + octree.MinZ;
                                var maxX = minX + res;
                                var maxY = minY + res;
                                var maxZ = minZ + res;

                                // If the octant is not finalized we can't do anything yet
                                if (!finalizer.IsFinalized(minX, minY, minZ, maxX, maxY, maxZ))
                                {
                                    continue;
                                }

                                // Check if the chunk is not too small. Otherwise, redistribute the points in the parent octant.
                                // There is no guarantee that parents still exist. They may have already been written and freed.
                                // (Requiring that chunks have more than min_points_per_octant is not a strong requirement,
                                // but producing a LAZ chunks with only 2 or 3 points is suboptimal).
                                if (currentOctant.Count <= minPointsPerOctant)
                                {
                                    var moved = false;
                                    var key = currentKey;
                                    while (key != EptKey.Root && !moved)
                                    {
                                        key = key.GetParent();
                                        if (!registry.TryGetValue(key, out var parent))
                                        {
                                            continue;
                                        }

                                        foreach (var p in currentOctant)
                                        {
                                            parent.Add(p, -1, bufferId);
                                        }

                                        currentOctant.Clean();

                                        // The octant must be inserted in the list because it may have children
                                        if (currentKey.Depth < maxDepth)
                                        {
                                            entries.Add(new(
                                                new(key.Depth, key.X, key.Y, key.Z),
                                                0,
                                                0,
                                                0));
                                        }

                                        _ = registry.Remove(currentKey);
                                        moved = true;
                                    }

                                    // Points were moved in another octant and the octant was deleted: we do not write this octant
                                    if (moved)
                                    {
                                        continue;
                                    }
                                }

                                if (sort)
                                {
                                    currentOctant.Sort();
                                }

                                // The octant is finalized: we can write the chunk and free up the memory
                                var entryOffset = (ulong)writer.BaseStream.Position;

                                // write the points as a chunk
                                writer.Write(currentOctant, currentOctant.Count);
                                builder.Add(currentOctant.Select(x => x.PointDataRecord!));

                                // Record the VLR entry
                                var entry = new Cloud.CopcHierarchy.Entry(
                                    new(currentKey.Depth, currentKey.X, currentKey.Y, currentKey.Z),
                                    entryOffset,
                                    (int)((ulong)writer.BaseStream.Position - entryOffset),
                                    currentOctant.Count);

                                entries.Add(entry);

                                currentOctant.Clean();
                                _ = registry.Remove(currentKey);
                            }
                        }

                        bufferId++;
                        buffer.Clear();
                    }

                    point = reader.ReadPointDataRecord().PointDataRecord;
                }
                while (point is not null);

                return entries;
            }
        }
    }

    /// <summary>
    /// The <see cref="LazReader"/> extensions
    /// </summary>
    extension<T>(T reader)
        where T : ILasReader, ILazReader
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

        private static IEnumerable<LasPointMemory> ReadPointDataRecords<TReader>(TReader lazReader, Cloud.CopcHierarchy hierarchy, int maximumDepth)
            where TReader : ILasReader, ILazReader => hierarchy
            .GetAllEntries()
            .Where(entry => entry.Key.Level <= maximumDepth)
            .OrderBy(static entry => entry.Offset)
            .SelectMany(entry => lazReader.ReadPointDataRecords(entry));

        private static IEnumerable<LasPointMemory> ReadPointDataRecords<TReader>(TReader lazReader, BoundingBox box, Cloud.CopcHierarchy hierarchy, int maximumDepth)
            where TReader : ILasReader, ILazReader
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
                        ? lazReader.ReadPointDataRecords(entry).Where(point => box.Contains(quantizer.Get(point.PointDataRecord!)))
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

        private static Cloud.CopcHierarchy ProcessCopcHierarchy(ExtendedVariableLengthRecordHeader header, IEnumerable<VariableLengthRecord> variableLengthRecords, long position, ReadOnlySpan<byte> data) =>
            new(header, variableLengthRecords.OfType<Cloud.CopcInfo>().Single(), (ulong)position, data);
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

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private readonly struct EptKey(int depth, int x, int y, int z) : IEquatable<EptKey>
    {
        public static readonly EptKey Root = new(0, 0, 0, 0);

        public EptKey()
            : this(-1, -1, -1, -1)
        {
        }

        public int Depth { get; } = depth;

        public int X { get; } = x;

        public int Y { get; } = y;

        public int Z { get; } = z;

        public static bool operator ==(EptKey a, EptKey b) => a.Depth == b.Depth && a.X == b.X && a.Y == b.Y && a.Z == b.Z;

        public static bool operator !=(EptKey a, EptKey b) => a.Depth != b.Depth || a.X != b.X || a.Y != b.Y || a.Z != b.Z;

        public static bool operator >=(EptKey a, EptKey b) => a.X >= b.X && a.Y >= b.Y && a.Z >= b.Z && a.Depth >= b.Depth;

        public static bool operator <=(EptKey a, EptKey b) => a.X <= b.X && a.Y <= b.Y && a.Z <= b.Z && a.Depth <= b.Depth;

        public override int GetHashCode() => HashCode.Combine(this.Depth, this.X, this.Y, this.Z);

        public EptKey[] GetChildren()
        {
            const int Zero = 0;
            const int One = 1;
            const int Two = 2;
            var children = new EptKey[8];
            for (var direction = 0; direction < 8; direction++)
            {
                var currentX = this.X * 2;
                var currentY = this.Y * 2;
                var currentZ = this.Z * 2;

                if ((direction & (One << Zero)) is not Zero)
                {
                    currentX++;
                }

                if ((direction & (One << One)) is not Zero)
                {
                    currentY++;
                }

                if ((direction & (One << Two)) is not Zero)
                {
                    currentZ++;
                }

                children[direction] = new(this.Depth + 1, currentX, currentY, currentZ);
            }

            return children;
        }

        public EptKey GetParent() => this >= Root && this.Depth is not 0
            ? new(this.Depth - 1, this.X >> 1, this.Y >> 1, this.Z >> 1)
            : new EptKey();

        public override bool Equals(object? obj) => obj is EptKey key && this.Equals(key);

        public bool Equals(EptKey other) => this == other;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct VoxelRecord(ushort bufferId, int positionId)
    {
        // The position of the point in the array
        public readonly int PositionId = positionId;

        // The ID of the buffer in which the point was inserted (used for optimization).
        public ushort BufferId = bufferId;
    }

    private sealed class Finalizer
    {
        private readonly Vector3D minimum;
        private readonly Vector3D maximum;
        private readonly Vector3D resolution;
        private readonly int columnCount;
        private readonly int rowCount;
        private readonly int layerCount;
        private readonly uint[] grid;
        private readonly PointDataRecordQuantizer converter;

        public Finalizer(in HeaderBlock header, PointDataRecordQuantizer converter, int division)
        {
            this.minimum = header.Min;
            this.maximum = header.Max;

            var difference = this.maximum - this.minimum;

            var gridSpacing = Math.Max(Math.Max(difference.X, difference.Y), difference.Z) / division;

            this.columnCount = (int)Math.Ceiling((difference.X) / gridSpacing);
            this.rowCount = (int)Math.Ceiling((difference.Y) / gridSpacing);
            this.layerCount = (int)Math.Ceiling((difference.Z) / gridSpacing);

            this.resolution = new(
                (difference.X) / this.columnCount,
                (difference.Y) / this.rowCount,
                (difference.Z) / this.layerCount);

            this.grid = new uint[this.columnCount * this.rowCount * this.layerCount];

            this.converter = converter;
        }

        public bool AreAnyFinalized { get; private set; }

        public void Add(IBasePointDataRecord point) => this.Add(this.converter.Get(point));

        public void Remove(Vector3D vector)
        {
            this.AreAnyFinalized = false;
            var cell = this.GetCell(vector);
            var gridValue = this.grid[cell];
            if (gridValue is 0)
            {
                throw new InvalidOperationException("internal error in the finalizer. Please report");
            }

            gridValue--;
            this.grid[cell] = gridValue;
            this.AreAnyFinalized = gridValue is 0;
        }

        public bool IsFinalized(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
        {
            var startX = Math.Max((int)Math.Floor((minX - this.minimum.X) / this.resolution.X), 0);
            var startY = Math.Max((int)Math.Floor((this.maximum.Y - maxY) / this.resolution.Y), 0);
            var startZ = Math.Max((int)Math.Floor((minZ - this.minimum.Z) / this.resolution.Z), 0);
            var endX = Math.Min((int)Math.Ceiling((maxX - this.minimum.X) / this.resolution.X), this.columnCount - 1);
            var endY = Math.Min((int)Math.Ceiling((this.maximum.Y - minY) / this.resolution.Y), this.rowCount - 1);
            var endZ = Math.Min((int)Math.Ceiling((maxZ - this.minimum.Z) / this.resolution.Z), this.layerCount - 1);

            for (var column = startX; column <= endX; column++)
            {
                for (var row = startY; row <= endY; row++)
                {
                    for (var layer = startZ; layer <= endZ; layer++)
                    {
                        var cell = (layer * this.rowCount * this.columnCount) + (row * this.columnCount) + column;
                        if (this.grid[cell] is not 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void Add(Vector3D vector) => this.grid[this.GetCell(vector)]++;

        private int GetCell(Vector3D vector)
        {
            var x = vector.X;
            var y = vector.Y;
            var z = vector.Z;

            var column = 0;
            if (x > this.minimum.X)
            {
                column = x < this.maximum.X
                    ? (int)Math.Floor((x - this.minimum.X) / this.resolution.X)
                    : this.columnCount - 1;
            }

            var row = 0;
            if (y > this.minimum.Y)
            {
                row = y < this.maximum.Y
                    ? (int)Math.Floor((this.maximum.Y - y) / this.resolution.Y)
                    : this.rowCount - 1;
            }

            var layer = 0;
            if (z > this.minimum.Z)
            {
                layer = z < this.maximum.Z
                    ? (int)Math.Floor((z - this.minimum.Z) / this.resolution.Z)
                    : this.layerCount - 1;
            }

            return (layer * this.rowCount * this.columnCount) + (row * this.columnCount) + column;
        }
    }

    private sealed class EptOctree
    {
        private readonly double maxX;

        public EptOctree(in HeaderBlock header)
        {
            var centerX = (header.Min.X + header.Max.X) / 2;
            var centerY = (header.Min.Y + header.Max.Y) / 2;
            var centerZ = (header.Min.Z + header.Max.Z) / 2;
            var halfSize = Math.Max(Math.Max(header.Max.X - header.Min.X, header.Max.Y - header.Min.Y), header.Max.Z - header.Min.Z) / 2;

            this.MinX = centerX - halfSize;
            this.MinY = centerY - halfSize;
            this.MinZ = centerZ - halfSize;
            this.maxX = centerX + halfSize;
            this.CentreY = centerY + halfSize;
            this.CentreZ = centerZ + halfSize;
        }

        public int GridSize { get; init; }

        public double Size => this.maxX - this.MinX;

        public double CentreX => (this.MinX + this.maxX) / 2;

        public double CentreY => (this.MinY + field) / 2;

        public double CentreZ => (this.MinZ + field) / 2;

        public double HalfSize => (this.maxX - this.MinX) / 2;

        public double MinX { get; }

        public double MinY { get; }

        public double MinZ { get; }

        public static int ComputeMaxDepth(in HeaderBlock header, ulong maxPointsPerOctant)
        {
            // strategy to regulate the maximum depth of the octree
            var sizeX = header.Max.X - header.Min.X;
            var sizeY = header.Max.Y - header.Min.Y;
            var sizeZ = header.Max.Z - header.Min.Z;
            var size = Math.Max(Math.Max(sizeX, sizeY), sizeZ);
            var pointCount = Math.Max(header.LegacyNumberOfPointRecords, header.NumberOfPointRecords);
            var computedMaxDepth = 0;

            while (pointCount > maxPointsPerOctant)
            {
                if (sizeX >= size)
                {
                    pointCount /= 2;
                }

                if (sizeY >= size)
                {
                    pointCount /= 2;
                }

                if (sizeZ >= size)
                {
                    pointCount /= 2;
                }

                size /= 2;
                computedMaxDepth++;
            }

            return computedMaxDepth;
        }

        public EptKey GetKey(Vector3D vector, int depth)
        {
            var gridSize = (int)Math.Pow(2, depth);
            var gridResolution = this.Size / gridSize;

            return new(
                depth,
                GetKeyValue(vector.X - this.MinX),
                GetKeyValue(vector.Y - this.MinY),
                GetKeyValue(vector.Z - this.MinZ));

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            int GetKeyValue(double value)
            {
                return Math.Min(Math.Max(0, (int)Math.Floor(value / gridResolution)), gridSize - 1);
            }
        }

        public int GetCell(Vector3D vector, EptKey key)
        {
            var halfSize = this.HalfSize;
            var resolution = this.Size / (1 << key.Depth);

            var gridResolution = resolution / this.GridSize;
            var cellX = GetCellValue(vector.X - (resolution * key.X) - (this.CentreX - halfSize));
            var cellY = GetCellValue(vector.Y - (resolution * key.Y) - (this.CentreY - halfSize));
            var cellZ = GetCellValue(vector.Z - (resolution * key.Z) - (this.CentreZ - halfSize));

            return (cellZ * this.GridSize * this.GridSize) + (cellY * this.GridSize) + cellX;

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            int GetCellValue(double value)
            {
                return Math.Min(Math.Max(0, (int)Math.Floor(value / gridResolution)), this.GridSize - 1);
            }
        }
    }

    private sealed class Octant(int capacity = 25000) : IEnumerable<LasPointMemory>
    {
        private readonly List<LasPointMemory> points = new(capacity);

        private readonly Dictionary<int, VoxelRecord> occupancy = [];

        public int Count => this.points.Count;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "This is public")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "This is public")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required")]
        public LasPointMemory this[int i] => this.points[i];

        public bool TryGetVoxel(int cell, out VoxelRecord voxel) => this.occupancy.TryGetValue(cell, out voxel);

        public void Add(LasPointMemory point, int cell, ushort chunk)
        {
            var index = this.points.Count;
            this.points.Add(point);

            if (cell >= 0)
            {
                this.occupancy.Add(cell, new(chunk, index));
            }
        }

        public void Swap(ref LasPointMemory point, int position) => (point, this.points[position]) = (this.points[position], point);

        public void Clean() => this.points.Clear();

        public void Sort()
        {
            this.points.Sort(static (first, second) => ComparePointDataRecord(first.PointDataRecord, second.PointDataRecord));

            static int ComparePointDataRecord(IBasePointDataRecord? first, IBasePointDataRecord? second)
            {
                if (first is null)
                {
                    return second is null ? 0 : 1;
                }

                if (second is null)
                {
                    return -1;
                }

                if (first is IGpsPointDataRecord rawFirst && second is IGpsPointDataRecord rawSecond)
                {
                    var comparison = rawFirst.GpsTime.CompareTo(rawSecond.GpsTime);
                    if (comparison is not 0)
                    {
                        return comparison;
                    }
                }

                if (first is IExtendedPointDataRecord extendedFirst && second is IExtendedPointDataRecord extendedSecond)
                {
                    var comparison = extendedFirst.ScannerChannel.CompareTo(extendedSecond.ScannerChannel);
                    if (comparison is not 0)
                    {
                        return comparison;
                    }
                }

                return first.ReturnNumber.CompareTo(second.ReturnNumber);
            }
        }

        IEnumerator<LasPointMemory> IEnumerable<LasPointMemory>.GetEnumerator() => this.points.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.points.GetEnumerator();
    }
}