// -----------------------------------------------------------------------
// <copyright file="CopcInfo.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Cloud;

/// <summary>
/// Represents the <c>COPC</c> information.
/// </summary>
/// <remarks>
/// <para>The info VLR <i>MUST</i> exist.</para>
/// <para>The info VLR <i>MUST</i> be the first VLR in the file (must begin at offset 375 from the beginning of the file).</para>
/// <para>The info VLR is 160 bytes described by the following structure. The reserved elements <i>MUST</i> be set to 0.</para>
/// </remarks>
public sealed record CopcInfo : VariableLengthRecord
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 1;

    private const int CentreXOffset = 0;
    private const int CentreYOffset = CentreXOffset + sizeof(double);
    private const int CentreZOffset = CentreYOffset + sizeof(double);
    private const int HalfSizeOffset = CentreZOffset + sizeof(double);
    private const int SpacingOffset = HalfSizeOffset + sizeof(double);
    private const int RootHierOffsetOffset = SpacingOffset + sizeof(double);
    private const int RootHierSizeOffset = RootHierOffsetOffset + sizeof(ulong);
    private const int GpsTimeMinimumOffset = RootHierSizeOffset + sizeof(ulong);
    private const int GpsTimeMaximumOffset = GpsTimeMinimumOffset + sizeof(double);
    private const int ReservedOffset = GpsTimeMaximumOffset + sizeof(double);
    private const int TotalSize = ReservedOffset + (11 * sizeof(ulong));

    /// <summary>
    /// Initializes a new instance of the <see cref="CopcInfo"/> class.
    /// </summary>
    public CopcInfo()
        : base(new VariableLengthRecordHeader
            {
                UserId = CopcConstants.UserId,
                RecordId = TagRecordId,
                RecordLengthAfterHeader = TotalSize,
                Description = "copc info",
            })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CopcInfo"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal CopcInfo(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : base(header)
    {
        this.CentreX = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data[..CentreYOffset]);
        this.CentreY = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data[CentreYOffset..CentreZOffset]);
        this.CentreZ = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data[CentreZOffset..HalfSizeOffset]);
        this.HalfSize = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data[HalfSizeOffset..SpacingOffset]);
        this.Spacing = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data[SpacingOffset..RootHierOffsetOffset]);
        this.RootHierOffset = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(data[RootHierOffsetOffset..RootHierSizeOffset]);
        this.RootHierSize = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(data[RootHierSizeOffset..GpsTimeMinimumOffset]);
        this.GpsTimeMinimum = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data[GpsTimeMinimumOffset..GpsTimeMaximumOffset]);
        this.GpsTimeMaximum = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data[GpsTimeMinimumOffset..ReservedOffset]);
    }

    /// <summary>
    /// Gets the actual (unscaled) X coordinate of center of octree.
    /// </summary>
    public double CentreX { get; init; }

    /// <summary>
    /// Gets the actual (unscaled) Y coordinate of center of octree.
    /// </summary>
    public double CentreY { get; init; }

    /// <summary>
    /// Gets the actual (unscaled) Z coordinate of center of octree.
    /// </summary>
    public double CentreZ { get; init; }

    /// <summary>
    /// Gets the perpendicular distance from the center to any side of the root node.
    /// </summary>
    public double HalfSize { get; init; }

    /// <summary>
    /// Gets Space between points at the root node.
    /// </summary>
    /// <remarks>This value is halved at each octree level.</remarks>
    public double Spacing { get; init; }

    /// <summary>
    /// Gets the file offset to the first hierarchy page.
    /// </summary>
    public ulong RootHierOffset { get; internal set; }

    /// <summary>
    /// Gets the size of the first hierarchy page in bytes.
    /// </summary>
    public ulong RootHierSize { get; internal set; }

    /// <summary>
    /// Gets the minimum of GPSTime.
    /// </summary>
    public double GpsTimeMinimum { get; init; }

    /// <summary>
    /// Gets the maximum of GPSTime.
    /// </summary>
    public double GpsTimeMaximum { get; init; }

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[..CentreYOffset], this.CentreX);
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[CentreYOffset..CentreZOffset], this.CentreY);
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[CentreZOffset..HalfSizeOffset], this.CentreZ);
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[HalfSizeOffset..SpacingOffset], this.HalfSize);
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[SpacingOffset..RootHierOffsetOffset], this.Spacing);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination[RootHierOffsetOffset..RootHierSizeOffset], this.RootHierOffset);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination[RootHierSizeOffset..GpsTimeMinimumOffset], this.RootHierSize);
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[GpsTimeMinimumOffset..GpsTimeMaximumOffset], this.GpsTimeMinimum);
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[GpsTimeMinimumOffset..ReservedOffset], this.GpsTimeMaximum);
        destination[ReservedOffset..TotalSize].Clear();
        return TotalSize;
    }
}