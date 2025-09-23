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
    private const ushort UserDefinedValue = (ushort)short.MaxValue;

    /// <summary>
    /// Gets the WKT from the list of <c>GeoTIFF</c> tags.
    /// </summary>
    /// <param name="records">The list of <c>GeoTIFF</c> tags.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> constaining the <c>WKT</c> tags if possible.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="records"/> is <see langword="null"/>.</exception>
    public static IEnumerable<VariableLengthRecord> ToWkt(this IReadOnlyCollection<VariableLengthRecord> records)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(records);
#else
        if (records is null)
        {
            throw new ArgumentNullException(nameof(records));
        }
#endif

        // get the geo key
        if (records.OfType<GeoKeyDirectoryTag>().FirstOrDefault() is { } directory)
        {
            if (directory.TryGetValue(GeoKey.ProjectedCRSGeoKey, out var projectedKey) && projectedKey is { ValueOffset: not UserDefinedValue and var projectedSrid })
            {
                // get the projected coordinate references system
                return GetWellKnownTextFromSrid(projectedSrid);
            }

            if (directory.TryGetValue(GeoKey.GeodeticCRSGeoKey, out var geodeticKey) && geodeticKey is { ValueOffset: not UserDefinedValue and var geodeticSrid })
            {
                // get the geodetic coordinate references system
                return GetWellKnownTextFromSrid(geodeticSrid);
            }
        }

        return [];
    }

    /// <summary>
    /// Writes the header.
    /// </summary>
    /// <param name="writer">The LAS writer.</param>
    /// <param name="header">The header.</param>
    /// <param name="srid">The SRID value.</param>
    public static void Write(this ILasWriter writer, in HeaderBlock header, ushort srid)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(writer);
#else
        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }
#endif

#if LAS1_4_OR_GREATER
        writer.Write(header, ToVariableLengthRecords(srid, header.GlobalEncoding));
#else
        writer.Write(header, ToVariableLengthRecords(srid));
#endif
    }

#if LAS1_4_OR_GREATER
    private static IEnumerable<VariableLengthRecord> ToVariableLengthRecords(ushort srid, GlobalEncoding globalEncoding = GlobalEncoding.Wkt)
        => globalEncoding.HasFlag(GlobalEncoding.Wkt) ? GetWellKnownTextFromSrid(srid) : GetGeoTiffFromSrid(srid);
#else
    public static IEnumerable<VariableLengthRecord> ToVariableLengthRecords(this short epsgSrid) => GetGeoTiffFromSrid(srid);
#endif

    private static IEnumerable<VariableLengthRecord> GetWellKnownTextFromSrid(ushort srid)
    {
        using var context = new ProjContext();
        context.Open();
        var wkt = context.GetWkt(srid);
        yield return new OgcCoordinateSystemWkt(wkt);
        context.Close();
    }

    private static IEnumerable<VariableLengthRecord> GetGeoTiffFromSrid(ushort srid)
    {
        using (var context = new ProjContext())
        {
            context.Open();
            if (context.IsGeodeticCoordinateReferenceSystem(srid))
            {
                yield return new GeoKeyDirectoryTag(new GeoKeyEntry { KeyId = GeoKey.GeodeticCRSGeoKey, ValueOffset = srid, Count = 1 });
                yield break;
            }

            if (context.IsProjectedCoordinateReferenceSystem(srid))
            {
                yield return new GeoKeyDirectoryTag(new GeoKeyEntry { KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = srid, Count = 1 });
                yield break;
            }
        }

        throw new KeyNotFoundException();
    }
}