// -----------------------------------------------------------------------
// <copyright file="GeodesyExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <summary>
/// The <c>Geodesy</c> extensions.
/// </summary>
public static class GeodesyExtensions
{
#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets the WKT from the list of <c>GeoTIFF</c> tags.
    /// </summary>
    /// <param name="records">The list of <c>GeoTIFF</c> tags.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> constaining the <c>WKT</c> tags if possible.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="records"/> is <see langword="null"/>.</exception>
    public static IEnumerable<VariableLengthRecord> ToWkt(this IReadOnlyCollection<VariableLengthRecord> records)
    {
        const ushort UserDefinedValue = (ushort)short.MaxValue;
        ArgumentNullException.ThrowIfNull(records);

        return records.OfType<GeoKeyDirectoryTag>().FirstOrDefault() switch
        {
            // get the geo key
            { } directory when directory.TryGetValue(GeoKey.ProjectedCRSGeoKey, out var projectedKey) && projectedKey is { ValueOffset: not UserDefinedValue and var projectedSrid } => GetWellKnownTextFromSrid(projectedSrid),
            { } directory when directory.TryGetValue(GeoKey.GeodeticCRSGeoKey, out var geodeticKey) && geodeticKey is { ValueOffset: not UserDefinedValue and var geodeticSrid } => GetWellKnownTextFromSrid(geodeticSrid),
            _ => [],
        };
    }
#endif

    /// <summary>
    /// Writes the header.
    /// </summary>
    /// <param name="writer">The LAS writer.</param>
    /// <param name="header">The header.</param>
    /// <param name="srid">The SRID value.</param>
    public static void Write(this ILasWriter writer, in HeaderBlock header, ushort srid)
    {
        ArgumentNullException.ThrowIfNull(writer);

#if LAS1_4_OR_GREATER
        writer.Write(header, GetVariableLengthRecords(srid, header.GlobalEncoding));
#else
        writer.Write(header, GetVariableLengthRecords(srid));
#endif
    }

#if LAS1_4_OR_GREATER
    private static IEnumerable<VariableLengthRecord> GetVariableLengthRecords(ushort srid, GlobalEncoding globalEncoding = GlobalEncoding.Wkt)
        => globalEncoding.HasFlag(GlobalEncoding.Wkt) ? GetWellKnownTextFromSrid(srid) : GetGeoTiffFromSrid(srid);

    private static IEnumerable<VariableLengthRecord> GetWellKnownTextFromSrid(ushort srid)
    {
        using var context = new ProjContext();
        context.Open();
        var wkt = context.GetWkt(srid);
        yield return new OgcCoordinateSystemWkt(wkt);
        context.Close();
    }
#else
    private static IEnumerable<VariableLengthRecord> GetVariableLengthRecords(this ushort srid) => GetGeoTiffFromSrid(srid);
#endif

    private static IEnumerable<VariableLengthRecord> GetGeoTiffFromSrid(ushort srid)
    {
        using var context = new ProjContext();
        context.Open();
        return context switch
        {
            _ when context.IsGeodeticCoordinateReferenceSystem(srid) => [new GeoKeyDirectoryTag(new GeoKeyEntry { KeyId = GeoKey.GeodeticCRSGeoKey, ValueOffset = srid, Count = 1 })],
            _ when context.IsProjectedCoordinateReferenceSystem(srid) => [new GeoKeyDirectoryTag(new GeoKeyEntry { KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = srid, Count = 1 })],
            _ => throw new KeyNotFoundException(),
        };
    }
}