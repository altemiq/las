﻿// -----------------------------------------------------------------------
// <copyright file="Processor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.To.Copc;

/// <summary>
/// The las2copc processor.
/// </summary>
internal static class Processor
{
    /// <summary>
    /// Processes the input to output.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="input">The input URI.</param>
    /// <param name="output">The output file.</param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <param name="maxPointsPerOctant">The maximum number of points per octant.</param>
    /// <param name="occupancyResolution">The occupancy resolution.</param>
    public static void Process(
        IServiceProvider? serviceProvider,
        Uri input,
        FileInfo output,
        int maxDepth = -1,
        ulong maxPointsPerOctant = 100000UL,
        float occupancyResolution = 50F) => Process(File.OpenRead(input, serviceProvider), output, maxDepth, maxPointsPerOctant, occupancyResolution);

    /// <summary>
    /// Processes the input to output.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="output">The output file.</param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <param name="maxPointsPerOctant">The maximum number of points per octant.</param>
    /// <param name="occupancyResolution">The occupancy resolution.</param>
    public static void Process(
        Stream stream,
        FileInfo output,
        int maxDepth = -1,
        ulong maxPointsPerOctant = 100000UL,
        float occupancyResolution = 50F)
    {
        using var reader = LazReader.Create(stream);

        Process(reader, output.OpenWrite(), maxDepth, maxPointsPerOctant: maxPointsPerOctant, occupancyResolution: occupancyResolution, swap: false, shuffle: false);
    }

    /// <summary>
    /// Writes the input reader to the output stream.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="output">The output.</param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <param name="maxPointsPerOctant">The maximum number of points per octant.</param>
    /// <param name="minPointsPerOctant">The minimum number of points per octant.</param>
    /// <param name="occupancyResolution">The occupancy resolution.</param>
    /// <param name="rootGridSize">The root grid size.</param>
    /// <param name="probaSwapEvent">The probablity swap event.</param>
    /// <param name="unordered">Set to <see langword="true"/> to leave points unordered.</param>
    /// <param name="bufferSize">The buffer size.</param>
    /// <param name="swap">Set to <see langword="true"/> to swap the points.</param>
    /// <param name="shuffle">Set to <see langword="true"/> to shuffle the points.</param>
    /// <param name="sort">Set to <see langword="true"/> to sort the points.</param>
    public static void Process(
        LasReader reader,
        Stream output,
        int maxDepth = -1,
        ulong maxPointsPerOctant = 100000UL,
        int minPointsPerOctant = 100,
        float occupancyResolution = 50F,
        int rootGridSize = 256,
        float probaSwapEvent = 0.95F,
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

        var tempMaxDepth = maxDepth;
        if (tempMaxDepth < 0)
        {
            tempMaxDepth = Math.Min(EptOctree.ComputeMaxDepth(header, maxPointsPerOctant), LimitDepth);
        }

        var quantizer = new PointDataRecordQuantizer(header);
        var finalizer = new Finalizer(header, quantizer, 2 << tempMaxDepth);
        var occupancy = new Internals.OccupancyGrid(quantizer, occupancyResolution);

        var gpsTimeMinimum = double.MaxValue;
        var gpsTimeMaximum = double.MinValue;
        var builder = new HeaderBlockBuilder(header);
        if (builder.Version.Minor < 4)
        {
            builder.Version = new(1, 4);
        }

        builder.GeneratingSoftware = ToolConstants.GeneratingSoftware;
        builder.SystemIdentifier = ToolConstants.SystemIdentifier;
        builder.SetCompressed();
        builder.Reset();
        builder.ScaleFactor = header.ScaleFactor;

        while (reader.ReadPointDataRecord() is { PointDataRecord: { } pointDataRecord })
        {
            if (pointDataRecord is IGpsPointDataRecord gpsPointDataRecord)
            {
                UpdateMinMax(ref gpsTimeMinimum, ref gpsTimeMaximum, gpsPointDataRecord.GpsTime);

                static void UpdateMinMax(ref double minimum, ref double maximum, double value)
                {
                    _ = Internals.InterlockedExtension.ExchangeIfLessThan(ref minimum, value);
                    _ = Internals.InterlockedExtension.ExchangeIfGreaterThan(ref maximum, value);
                }
            }

            finalizer.Add(pointDataRecord);
            builder.Add(pointDataRecord);
            _ = occupancy.Add(pointDataRecord);
        }

        header = builder.HeaderBlock;
        var octree = new EptOctree(header) { GridSize = rootGridSize };

        if (maxDepth < 0)
        {
            maxDepth = Math.Min(EptOctree.ComputeMaxDepth(header, maxPointsPerOctant), LimitDepth);
        }

        var area = occupancyResolution * occupancyResolution * occupancy.NumOccupied;
        var density = builder.NumberOfPointRecords / area;
        var voxelSizes = octree.Size / octree.GridSize;
        var swapProbabilities = new double[LimitDepth + 1];
        for (var i = 0; i <= LimitDepth; i++)
        {
            var expectedNumPoint = (int)(voxelSizes * voxelSizes * density);
            swapProbabilities[i] = (expectedNumPoint >= 5) ? 1D - Math.Pow(1D - probaSwapEvent, 1D / expectedNumPoint) : 0D;
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

        using var writer = new LazWriter(output);
        writer.Write(header, variableLengthRecords);
        builder.Reset();

        var entries = ProcessPoints(
            reader,
            writer,
            output,
            builder,
            header.NumberOfPointRecords,
            maxDepth,
            minPointsPerOctant,
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

        output.Position = 0;
        writer.Write(builder.HeaderBlock, variableLengthRecords);

        static List<Cloud.CopcHierarchy.Entry> ProcessPoints(
            LasReader reader,
            LazWriter writer,
            Stream output,
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
                    var (x, y, z) = quantizer.Get(point);
                    skip = octree.GetKey(x, y, z, 1) != currentUnorderedKey;
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
                    for (var i = 0; i < buffer.Count; i++)
                    {
                        var lasPoint = buffer[i];
                        point = lasPoint.PointDataRecord!;
                        var (x, y, z) = quantizer.Get(point);
                        finalizer.Remove(x, y, z);

                        // Search a place to insert the point
                        var level = 0;
                        int cell;

                        Octant? octant;
                        bool accepted;
                        do
                        {
                            var key = octree.GetKey(x, y, z, level);
                            cell = level == maxDepth
                                ? -1 // Do not build an occupancy grid for last level. Point must be inserted anyway.
                                : octree.GetCell(x, y, z, key);

                            if (!registry.TryGetValue(key, out octant))
                            {
                                // create the octant
                                octant = new();
                                registry.Add(key, octant);
                            }

                            accepted = !octant.TryGetVoxel(cell, out var voxel) || (level == maxDepth);

                            if (swap
                                && !accepted
                                && voxel.BufferId != bufferId
                                && ((float)random.Next(short.MaxValue) / short.MaxValue) < swapProbabilities[level])
                            {
                                // only swap if bufid != id_buffer: save the heavy cost (on disk) of swapping.
                                // No need to swap two points from the same buffer: they are already shuffled.
                                octant.Swap(ref lasPoint, voxel.PositionId);
                                voxel.BufferId = bufferId;
                            }

                            level++;
                        }
                        while (!accepted);

                        // insert the point
                        octant!.Add(lasPoint, cell, bufferId);

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
                            var entryOffset = (ulong)output.Position;

                            // write the points as a chunk
                            writer.Write(currentOctant, currentOctant.Count);
                            builder.Add(currentOctant.Select(x => x.PointDataRecord!));

                            // Record the VLR entry
                            var entry = new Cloud.CopcHierarchy.Entry(
                                new(currentKey.Depth, currentKey.X, currentKey.Y, currentKey.Z),
                                entryOffset,
                                (int)((ulong)output.Position - entryOffset),
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
        private readonly double minimumX;
        private readonly double minimumY;
        private readonly double minimumZ;
        private readonly double maximumX;
        private readonly double maximumY;
        private readonly double maximumZ;
        private readonly double resolutionX;
        private readonly double resolutionY;
        private readonly double resolutionZ;
        private readonly int columnCount;
        private readonly int rowCount;
        private readonly int layerCount;
        private readonly uint[] grid;
        private readonly PointDataRecordQuantizer converter;

        public Finalizer(in HeaderBlock header, PointDataRecordQuantizer converter, int division)
        {
            this.minimumX = header.Min.X;
            this.maximumX = header.Max.X;
            this.minimumY = header.Min.Y;
            this.maximumY = header.Max.Y;
            this.minimumZ = header.Min.Z;
            this.maximumZ = header.Max.Z;

            var gridSpacing = Math.Max(Math.Max(this.maximumX - this.minimumX, this.maximumY - this.minimumY), this.maximumZ - this.minimumZ) / division;

            this.columnCount = (int)Math.Ceiling((this.maximumX - this.minimumX) / gridSpacing);
            this.rowCount = (int)Math.Ceiling((this.maximumY - this.minimumY) / gridSpacing);
            this.layerCount = (int)Math.Ceiling((this.maximumZ - this.minimumZ) / gridSpacing);

            this.resolutionX = (this.maximumX - this.minimumX) / this.columnCount;
            this.resolutionY = (this.maximumY - this.minimumY) / this.rowCount;
            this.resolutionZ = (this.maximumZ - this.minimumZ) / this.layerCount;

            this.grid = new uint[this.columnCount * this.rowCount * this.layerCount];

            this.converter = converter;
        }

        public bool AreAnyFinalized { get; private set; }

        public void Add(IBasePointDataRecord point)
        {
            var (x, y, z) = this.converter.Get(point);
            this.Add(x, y, z);
        }

        public void Add(double x, double y, double z) => this.grid[this.CellFromXyz(x, y, z)]++;

        public void Remove(double x, double y, double z)
        {
            this.AreAnyFinalized = false;
            var cell = this.CellFromXyz(x, y, z);
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
            var startX = Math.Max((int)Math.Floor((minX - this.minimumX) / this.resolutionX), 0);
            var startY = Math.Max((int)Math.Floor((this.maximumY - maxY) / this.resolutionY), 0);
            var startZ = Math.Max((int)Math.Floor((minZ - this.minimumZ) / this.resolutionZ), 0);
            var endX = Math.Min((int)Math.Ceiling((maxX - this.minimumX) / this.resolutionX), this.columnCount - 1);
            var endY = Math.Min((int)Math.Ceiling((this.maximumY - minY) / this.resolutionY), this.rowCount - 1);
            var endZ = Math.Min((int)Math.Ceiling((maxZ - this.minimumZ) / this.resolutionZ), this.layerCount - 1);

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

        private int CellFromXyz(double x, double y, double z)
        {
            int colum = default;
            if (x > this.minimumX)
            {
                colum = x < this.maximumX
                    ? (int)Math.Floor((x - this.minimumX) / this.resolutionX)
                    : this.columnCount - 1;
            }

            int row = default;
            if (y > this.minimumY)
            {
                row = y < this.maximumY
                    ? (int)Math.Floor((this.maximumY - y) / this.resolutionY)
                    : this.rowCount - 1;
            }

            int layer = default;
            if (z > this.minimumZ)
            {
                layer = z < this.maximumZ
                    ? (int)Math.Floor((z - this.minimumZ) / this.resolutionZ)
                    : this.layerCount - 1;
            }

            return (layer * this.rowCount * this.columnCount) + (row * this.columnCount) + colum;
        }
    }

    private sealed class EptOctree
    {
        private readonly double maxX;
        private readonly double maxY;
        private readonly double maxZ;

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
            this.maxY = centerY + halfSize;
            this.maxZ = centerZ + halfSize;
        }

        public int GridSize { get; init; }

        public double Size => this.maxX - this.MinX;

        public double CentreX => (this.MinX + this.maxX) / 2;

        public double CentreY => (this.MinY + this.maxY) / 2;

        public double CentreZ => (this.MinZ + this.maxZ) / 2;

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

        public EptKey GetKey(double x, double y, double z, int depth)
        {
            var gridSize = (int)Math.Pow(2, depth);
            var gridResolution = this.Size / gridSize;

            return new(
                depth,
                GetKeyValue(x - this.MinX),
                GetKeyValue(y - this.MinY),
                GetKeyValue(z - this.MinZ));

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            int GetKeyValue(double value)
            {
                return Math.Min(Math.Max(0, (int)Math.Floor(value / gridResolution)), gridSize - 1);
            }
        }

        public int GetCell(double x, double y, double z, EptKey key)
        {
            var halfSize = this.HalfSize;
            var resolution = this.Size / (1 << key.Depth);

            var gridResolution = resolution / this.GridSize;
            var cellX = GetCellValue(x - (resolution * key.X) - (this.CentreX - halfSize));
            var cellY = GetCellValue(y - (resolution * key.Y) - (this.CentreY - halfSize));
            var cellZ = GetCellValue(z - (resolution * key.Z) - (this.CentreZ - halfSize));

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
            this.points.Sort(CompareBuffers);

            static int CompareBuffers(LasPointMemory first, LasPointMemory second) => ComparePointDataRecord(first.PointDataRecord, second.PointDataRecord);

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