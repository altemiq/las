// -----------------------------------------------------------------------
// <copyright file="ProjContext.Usage.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <content>
/// Usage methods.
/// </content>
public partial class ProjContext
{
    private static IEnumerable<WellKnownTextNode> GetUsage(Microsoft.Data.Sqlite.SqliteCommand command, string table, string auth, int code, WellKnownTextVersion version)
    {
        const string ObjectTableName = "$objectTableName";
        command.CommandText = $"""
                               SELECT
                                   s.scope,
                                   e.description,
                                   e.south_lat,
                                   e.west_lon,
                                   e.north_lat,
                                   e.east_lon
                               FROM
                                   usage u
                               JOIN
                                   scope s
                               ON
                                   u.scope_{AuthNameField} = s.{AuthNameField}
                               AND
                                   u.scope_{CodeField} = s.{CodeField}
                               JOIN
                                   extent e
                               ON
                                   u.extent_{AuthNameField} = e.{AuthNameField}
                               AND
                                   u.extent_{CodeField} = e.{CodeField}
                               WHERE
                                   u.object_table_name = {ObjectTableName}
                               """;
        command.Parameters.Clear();
        command.Parameters.AddWithValue(ObjectTableName, table);
        AddAuthClause(command, auth, code, prefix: "object_", limit: 1);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            throw new KeyNotFoundException();
        }

        var scope = new WellKnownTextNode("SCOPE", reader.GetString(0));
        var area = new WellKnownTextNode("AREA", reader.GetString(1));
        var bbox = new WellKnownTextNode("BBOX", reader.GetDouble(2), reader.GetDouble(3), reader.GetDouble(4), reader.GetDouble(5));

        if (version is WellKnownTextVersion.Wkt2_2015)
        {
            yield return scope;
            yield return area;
            yield return bbox;
        }
        else if (version is WellKnownTextVersion.Wkt2_2019)
        {
            yield return new("USAGE", scope, area, bbox);
        }
    }
}