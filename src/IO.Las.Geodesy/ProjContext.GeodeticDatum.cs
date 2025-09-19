// -----------------------------------------------------------------------
// <copyright file="ProjContext.GeodeticDatum.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <content>
/// Geodetic Datum methods.
/// </content>
public partial class ProjContext
{
    private const string DatumName = "datum";

    private const string GeodeticDatumName = $"geodetic_{DatumName}";

    private const string GeodeticDatumAlias = "gd";

    private static IEnumerable<WellKnownTextNode> GetGeodeticDatum(Microsoft.Data.Sqlite.SqliteCommand command, string auth, int code, WellKnownTextVersion version)
    {
        command.CommandText = $"""
                               SELECT
                                   {GeodeticDatumAlias}.{NameField},
                                   {GeodeticDatumAlias}.{EllipsoidName}_{AuthNameField},
                                   {GeodeticDatumAlias}.{EllipsoidName}_{CodeField},
                                   {GeodeticDatumAlias}.{PrimeMeridianName}_{AuthNameField},
                                   {GeodeticDatumAlias}.{PrimeMeridianName}_{CodeField}
                               FROM {GeodeticDatumName} {GeodeticDatumAlias}
                               WHERE {GeodeticDatumAlias}.{DeprecatedField} = 0
                               """;
        AddAuthClause(command, auth, code, alias: GeodeticDatumAlias, limit: 1);
        IList<WellKnownTextValue> values = [];
        string ellipsoidAuthName;
        int ellipsoidCode;
        string primeMeridianAuthName;
        int primeMeridianCode;
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read())
            {
                throw new KeyNotFoundException();
            }

            var name = reader.GetString(0);
            values.Add(version is WellKnownTextVersion.Wkt1 ? name.Replace(' ', '_') : name);
            ellipsoidAuthName = reader.GetString(1);
            ellipsoidCode = reader.GetInt32(2);
            primeMeridianAuthName = reader.GetString(3);
            primeMeridianCode = reader.GetInt32(4);
        }

        values.Add(GetEllipsoid(command, ellipsoidAuthName, ellipsoidCode, version));

        if (version is WellKnownTextVersion.Wkt1)
        {
            values.Add(CreateAuthorityNode(auth, code, version));
        }

        yield return new("DATUM", values);
        yield return GetPrimeMeridian(command, primeMeridianAuthName, primeMeridianCode, version);
    }
}