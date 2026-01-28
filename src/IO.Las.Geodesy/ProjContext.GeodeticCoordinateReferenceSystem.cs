// -----------------------------------------------------------------------
// <copyright file="ProjContext.GeodeticCoordinateReferenceSystem.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <content>
/// Geodetic Coordinate Reference System methods.
/// </content>
public partial class ProjContext
{
    private const string GeodeticCoordinateSystemName = "geodetic_crs";

    private const string GeodeticCoordinateSystemAlias = "gcrs";

    private const string GetGeodeticCoordinateSystemCommandText = $"""
                                                                    SELECT
                                                                         {GeodeticCoordinateSystemAlias}.{NameField},
                                                                         {GeodeticCoordinateSystemAlias}.{CoordinateSystemName}_{AuthNameField},
                                                                         {GeodeticCoordinateSystemAlias}.{CoordinateSystemName}_{CodeField},
                                                                         {GeodeticCoordinateSystemAlias}.{DatumName}_{AuthNameField},
                                                                         {GeodeticCoordinateSystemAlias}.{DatumName}_{CodeField}
                                                                    FROM {GeodeticCoordinateSystemName} {GeodeticCoordinateSystemAlias}
                                                                    WHERE {GeodeticCoordinateSystemAlias}.{DeprecatedField} = 0
                                                                    """;

    private static bool IsGeodeticCoordinateReferenceSystem(Microsoft.Data.Sqlite.SqliteCommand command, string auth, int code)
    {
        command.CommandText = $"SELECT {GeodeticCoordinateSystemAlias}.{CodeField} FROM {GeodeticCoordinateSystemName} {GeodeticCoordinateSystemAlias} WHERE {GeodeticCoordinateSystemAlias}.{DeprecatedField} = 0";
        AddAuthClause(command, auth, code, limit: 1);
        using var reader = command.ExecuteReader();
        return reader.Read();
    }

    private static WellKnownTextNode GetGeodeticCoordinateReferenceSystem(Microsoft.Data.Sqlite.SqliteCommand command, string auth, int code, WellKnownTextVersion version, bool @base = false)
    {
        command.CommandText = GetGeodeticCoordinateSystemCommandText;
        AddAuthClause(command, auth, code, alias: GeodeticCoordinateSystemAlias, limit: 1);
        IList<WellKnownTextValue> values = [];
        string coordinateSystemAuthName;
        int coordinateSystemCode;
        string datumAuthName;
        int datumCode;
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read())
            {
                throw new KeyNotFoundException();
            }

            values.Add(reader.GetString(0));
            coordinateSystemAuthName = reader.GetString(1);
            coordinateSystemCode = reader.GetInt32(2);
            datumAuthName = reader.GetString(3);
            datumCode = reader.GetInt32(4);
        }

        foreach (var item in GetGeodeticDatum(command, datumAuthName, datumCode, version))
        {
            values.Add(item);
        }

        if (!@base)
        {
            // get the coordinate system
            foreach (var item in GetCoordinateSystem(command, coordinateSystemAuthName, coordinateSystemCode, version))
            {
                values.Add(item);
            }

            // get the usage
            foreach (var item in GetUsage(command, GeodeticCoordinateSystemName, auth, code, version))
            {
                values.Add(item);
            }

            values.Add(CreateAuthorityNode(auth, code, version));
        }
        else
        {
            switch (version)
            {
                case WellKnownTextVersion.Wkt1:
                    foreach (var item in GetCoordinateSystemUnit(command, coordinateSystemAuthName, coordinateSystemCode, version))
                    {
                        values.Add(item);
                    }

                    values.Add(CreateAuthorityNode(auth, code, version));
                    break;
                case WellKnownTextVersion.Wkt2_2019:
                    values.Add(CreateAuthorityNode(auth, code, version));
                    break;
                case WellKnownTextVersion.Wkt2_2015:
                default:
                    break;
            }
        }

        var nodeName = (version, @base) switch
        {
            (WellKnownTextVersion.Wkt1, _) => "GEOGCS",
            (WellKnownTextVersion.Wkt2_2015, true) => "BASEGEODCRS",
            (WellKnownTextVersion.Wkt2_2015, false) => "GEODCRS",
            (WellKnownTextVersion.Wkt2_2019, true) => "BASEGEOGCRS",
            (WellKnownTextVersion.Wkt2_2019, false) => "GEOGCRS",
            _ => throw new ArgumentOutOfRangeException(nameof(version)),
        };

        return new(nodeName, values);
    }

    private static bool TryGetGeodeticCoordinateReferenceSystem(Microsoft.Data.Sqlite.SqliteCommand command, string auth, int code, WellKnownTextVersion version, out WellKnownTextNode node)
    {
        if (IsGeodeticCoordinateReferenceSystem(command, auth, code))
        {
            node = GetGeodeticCoordinateReferenceSystem(command, auth, code, version);
            return true;
        }

        node = default;
        return false;
    }
}