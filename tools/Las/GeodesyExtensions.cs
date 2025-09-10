// -----------------------------------------------------------------------
// <copyright file="GeodesyExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The Geodesy extensions.
/// </summary>
public static class GeodesyExtensions
{
    /// <summary>
    /// Gets a value indicating whether the specified variable length record is a <c>GeoTIFF</c> tag.
    /// </summary>
    /// <param name="record">The variable length record.</param>
    /// <returns><see langword="true"/> is <paramref name="record"/> is a <c>GeoTIFF</c> tag; otherwise <see langword="false"/>.</returns>
    public static bool IsGeoTiff(this VariableLengthRecord record) => record is GeoAsciiParamsTag or GeoDoubleParamsTag or GeoKeyDirectoryTag;

    /// <summary>
    /// Removes the <c>GeoTIFF</c> tags from the list.
    /// </summary>
    /// <param name="records">The list of variable length records.</param>
    public static void RemoveGeoTiff(this IList<VariableLengthRecord> records)
    {
        for (var i = records.Count - 1; i >= 0; i--)
        {
            if (records[i].IsGeoTiff())
            {
                records.RemoveAt(0);
            }
        }
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets a value indicating whether the specified variable length record is a <c>WKT</c> tag.
    /// </summary>
    /// <param name="record">The variable length record.</param>
    /// <returns><see langword="true"/> is <paramref name="record"/> is a <c>WKT</c> tag; otherwise <see langword="false"/>.</returns>
    public static bool IsWkt(this VariableLengthRecord record) => record is OgcCoordinateSystemWkt or OgcMathTransformWkt;

    /// <summary>
    /// Removes the <c>WKT</c> tags from the list.
    /// </summary>
    /// <param name="records">The list of variable length records.</param>
    public static void RemoveWkt(this IList<VariableLengthRecord> records)
    {
        for (var i = records.Count - 1; i >= 0; i--)
        {
            if (records[i].IsWkt())
            {
                records.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Gets the WKT from the list of <c>GeoTIFF</c> tags.
    /// </summary>
    /// <param name="records">The list of <c>GeoTIFF</c> tags.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> constaining the <c>WKT</c> tags if possible.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="records"/> is <see langword="null"/>.</exception>
    public static IEnumerable<VariableLengthRecord> ToWkt(this IEnumerable<VariableLengthRecord> records)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(records);
#else
        if (records is null)
        {
            throw new ArgumentNullException(nameof(records));
        }
#endif

        return [];
    }
#endif
}