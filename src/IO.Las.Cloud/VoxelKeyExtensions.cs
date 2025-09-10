// -----------------------------------------------------------------------
// <copyright file="VoxelKeyExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Cloud;

/// <summary>
/// <see cref="CopcHierarchy.VoxelKey"/> extensions.
/// </summary>
public static class VoxelKeyExtensions
{
    /// <summary>
    /// Gets the parent of the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The parent of the <paramref name="key"/>.</returns>
    public static CopcHierarchy.VoxelKey Parent(this in CopcHierarchy.VoxelKey key) => new(key.Level - 1, key.X >> 1, key.Y >> 1, key.Z >> 1);

    /// <summary>
    /// Gets a value indicating whether the specified key is within the bounding box.
    /// </summary>
    /// <param name="key">The voxel key.</param>
    /// <param name="boundingBox">The bounding box to test against.</param>
    /// <param name="header">The header.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> is within <paramref name="boundingBox"/>; otherwise <see langword="false"/>.</returns>
    public static bool Within(this in CopcHierarchy.VoxelKey key, in BoundingBox boundingBox, in HeaderBlock header) => boundingBox.Contains(key, header);

    /// <summary>
    /// Gets a value indicating whether the specified key in contained within the bounding box.
    /// </summary>
    /// <param name="boundingBox">The bounding box to test against.</param>
    /// <param name="key">The voxel key.</param>
    /// <param name="header">The header.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> is within <paramref name="boundingBox"/>; otherwise <see langword="false"/>.</returns>
    public static bool Contains(this in BoundingBox boundingBox, in CopcHierarchy.VoxelKey key, in HeaderBlock header) => boundingBox.Contains(key.ToBoundingBox(header));

    /// <summary>
    /// Gets a value indicating whether the specified key intersects with the bounding box.
    /// </summary>
    /// <param name="key">The voxel key.</param>
    /// <param name="boundingBox">The bounding box to test against.</param>
    /// <param name="header">The header.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> is within <paramref name="boundingBox"/>; otherwise <see langword="false"/>.</returns>
    public static bool IntersectsWith(this in CopcHierarchy.VoxelKey key, in BoundingBox boundingBox, in HeaderBlock header) => boundingBox.IntersectsWith(key, header);

    /// <summary>
    /// Gets a value indicating whether the specified key intersects with the bounding box.
    /// </summary>
    /// <param name="boundingBox">The bounding box to test against.</param>
    /// <param name="key">The voxel key.</param>
    /// <param name="header">The header.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> is within <paramref name="boundingBox"/>; otherwise <see langword="false"/>.</returns>
    public static bool IntersectsWith(this in BoundingBox boundingBox, in CopcHierarchy.VoxelKey key, in HeaderBlock header) => boundingBox.IntersectsWith(key.ToBoundingBox(header));

    /// <summary>
    /// Creates a bounding box from the voxel key.
    /// </summary>
    /// <param name="key">The voxel key.</param>
    /// <param name="header">The LAS header block.</param>
    /// <returns>The bounding box.</returns>
    public static BoundingBox ToBoundingBox(this in CopcHierarchy.VoxelKey key, in HeaderBlock header) => key.ToBoundingBox(header.Min, header.Max);

    /// <summary>
    /// Creates a bounding box from the voxel key.
    /// </summary>
    /// <param name="key">The voxel key.</param>
    /// <param name="min">The minimum.</param>
    /// <param name="max">The maximum.</param>
    /// <returns>The bounding box.</returns>
    public static BoundingBox ToBoundingBox(this in CopcHierarchy.VoxelKey key, in Vector3D min, in Vector3D max)
    {
        var size = Math.Max(Math.Max(max.X - min.X, max.Y - min.Y), max.Z - min.Z);
        return BoundingBox.FromXYZWHD(
            (size * key.X) + min.X,
            (size * key.Y) + min.Y,
            (size * key.Z) + min.Z,
            size,
            size,
            size);
    }
}