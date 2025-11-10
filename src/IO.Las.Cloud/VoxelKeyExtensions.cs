// -----------------------------------------------------------------------
// <copyright file="VoxelKeyExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Cloud;

/// <summary>
/// <see cref="CopcHierarchy.VoxelKey"/> extensions.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "False positive")]
public static class VoxelKeyExtensions
{
    /// <summary>
    /// The <see cref="CopcHierarchy.VoxelKey"/> extensions.
    /// </summary>
    extension(in CopcHierarchy.VoxelKey key)
    {
        /// <summary>
        /// Gets the parent of the specified key.
        /// </summary>
        /// <returns>The parent of the key.</returns>
        public CopcHierarchy.VoxelKey Parent() => new(key.Level - 1, key.X >> 1, key.Y >> 1, key.Z >> 1);

        /// <summary>
        /// Gets a value indicating whether the specified key is within the bounding box.
        /// </summary>
        /// <param name="boundingBox">The bounding box to test against.</param>
        /// <param name="header">The header.</param>
        /// <returns><see langword="true"/> if the key is within <paramref name="boundingBox"/>; otherwise <see langword="false"/>.</returns>
        public bool Within(in BoundingBox boundingBox, in HeaderBlock header) => boundingBox.Contains(key, header);

        /// <summary>
        /// Gets a value indicating whether the specified key intersects with the bounding box.
        /// </summary>
        /// <param name="boundingBox">The bounding box to test against.</param>
        /// <param name="header">The header.</param>
        /// <returns><see langword="true"/> if the key intersects with <paramref name="boundingBox"/>; otherwise <see langword="false"/>.</returns>
        public bool IntersectsWith(in BoundingBox boundingBox, in HeaderBlock header) => boundingBox.IntersectsWith(key, header);

        /// <summary>
        /// Creates a bounding box from the voxel key.
        /// </summary>
        /// <param name="header">The LAS header block.</param>
        /// <returns>The bounding box.</returns>
        public BoundingBox ToBoundingBox(in HeaderBlock header) => key.ToBoundingBox(header.Min, header.Max);

        /// <summary>
        /// Creates a bounding box from the voxel key.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>The bounding box.</returns>
        public BoundingBox ToBoundingBox(in Vector3D min, in Vector3D max)
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

    /// <summary>
    /// The <see cref="BoundingBox"/> extensions.
    /// </summary>
    extension(in BoundingBox boundingBox)
    {
        /// <summary>
        /// Gets a value indicating whether the bounding box contains the specified key.
        /// </summary>
        /// <param name="key">The voxel key.</param>
        /// <param name="header">The header.</param>
        /// <returns><see langword="true"/> if the bounding box contains <paramref name="key"/>; otherwise <see langword="false"/>.</returns>
        public bool Contains(in CopcHierarchy.VoxelKey key, in HeaderBlock header) => boundingBox.Contains(key.ToBoundingBox(header));

        /// <summary>
        /// Gets a value indicating whether the bounding box intersects with the specified key.
        /// </summary>
        /// <param name="key">The voxel key.</param>
        /// <param name="header">The header.</param>
        /// <returns><see langword="true"/> if the bounding box intersects with <paramref name="key"/>; otherwise <see langword="false"/>.</returns>
        public bool IntersectsWith(in CopcHierarchy.VoxelKey key, in HeaderBlock header) => boundingBox.IntersectsWith(key.ToBoundingBox(header));
    }
}