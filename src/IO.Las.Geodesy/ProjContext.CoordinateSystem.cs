// -----------------------------------------------------------------------
// <copyright file="ProjContext.CoordinateSystem.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <content>
/// Coordinate System methods.
/// </content>
public partial class ProjContext
{
    private const string CoordinateSystemName = "coordinate_system";

    private const string CoordinateSystemAlias = "cs";

    private const string CoordinateSystemFields = $"{CoordinateSystemAlias}.type, {CoordinateSystemAlias}.dimension";

    private const string GetCoordinateSystemCommandText = $"SELECT {CoordinateSystemFields} FROM {CoordinateSystemName} {CoordinateSystemAlias} WHERE 1=1";

    private const string AxisName = "axis";

    private const string AxisAlias = "a";

    private const string AxisFields = $"{AxisAlias}.name, {AxisAlias}.abbrev, {AxisAlias}.orientation, {AxisAlias}.{CoordinateSystemName}_order, {AxisAlias}.uom_{AuthNameField}, {AxisAlias}.uom_{CodeField}";

    private const string GetAxisCommandText = $"""
                                               SELECT
                                                  {AxisFields},
                                                  {UnitOfMeasureFields}
                                               FROM {AxisName} {AxisAlias}
                                               JOIN {UnitOfMeasureName} {UnitOfMeasureAlias}
                                               ON {AxisAlias}.uom_{AuthNameField} = {UnitOfMeasureAlias}.{AuthNameField}
                                               AND {AxisAlias}.uom_{CodeField} = {UnitOfMeasureAlias}.{CodeField}
                                               WHERE {UnitOfMeasureClause}
                                               """;

    private const string UnitOfMeasureName = "unit_of_measure";

    private const string UnitOfMeasureAlias = "uom";

    private const string UnitOfMeasureClause = $"{UnitOfMeasureAlias}.{DeprecatedField} = 0";

    private const string UnitOfMeasureFields = $"{UnitOfMeasureAlias}.name, {UnitOfMeasureAlias}.type, {UnitOfMeasureAlias}.conv_factor";

    private static IEnumerable<WellKnownTextNode> GetCoordinateSystemUnit(Microsoft.Data.Sqlite.SqliteCommand command, string authority, int code, WellKnownTextVersion version)
    {
        if (version is WellKnownTextVersion.Wkt1)
        {
            command.CommandText = GetAxisCommandText;
            AddAuthClause(command, authority, code, prefix: "coordinate_system_", orderBy: $"{AxisAlias}.coordinate_system_order");

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                throw new KeyNotFoundException();
            }

            yield return GetUnitNode(
                reader.GetString(7),
                reader.GetString(6),
                reader.GetDouble(8),
                reader.GetString(4),
                reader.GetInt32(5),
                version);
        }
    }

    private static IEnumerable<WellKnownTextNode> GetCoordinateSystem(Microsoft.Data.Sqlite.SqliteCommand command, string authority, int code, WellKnownTextVersion version)
    {
        switch (version)
        {
            case WellKnownTextVersion.Wkt1:
            {
                command.CommandText = GetAxisCommandText;
                AddAuthClause(command, authority, code, prefix: "coordinate_system_", orderBy: $"{AxisAlias}.coordinate_system_order");

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    throw new KeyNotFoundException();
                }

                var first = true;
                do
                {
                    // ensure we output the units for the axis first
                    if (first)
                    {
                        yield return GetUnitNode(
                            reader.GetString(7),
                            reader.GetString(6),
                            reader.GetDouble(8),
                            reader.GetString(4),
                            reader.GetInt32(5),
                            version);
                        first = false;
                    }

                    var orientation = reader.GetString(2);
                    var name = reader.GetString(1) switch
                    {
                        "Lat" => "Latitude",
                        "Lon" => "Longitude",
                        _ => reader.GetString(0),
                    };

                    yield return new("AXIS", name, new WellKnownTextLiteral(orientation.ToUpperInvariant()));
                }
                while (reader.Read());

                break;
            }

            case WellKnownTextVersion.Wkt2_2015 or WellKnownTextVersion.Wkt2_2019:
            {
                command.CommandText = GetCoordinateSystemCommandText;
                AddAuthClause(command, authority, code, limit: 1);

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new KeyNotFoundException();
                    }

                    yield return new("CS", new WellKnownTextLiteral(reader.GetString(0)), reader.GetInt32(1));
                }

                command.CommandText = GetAxisCommandText;
                AddAuthClause(command, authority, code, prefix: "coordinate_system_", orderBy: $"{AxisAlias}.coordinate_system_order");
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new KeyNotFoundException();
                    }

                    do
                    {
                        var name = reader.GetString(0).ToLowerInvariant();
                        var abbreviation = reader.GetString(1);
                        var orientation = reader.GetString(2);

                        var units = GetUnitNode(
                            reader.GetString(7),
                            reader.GetString(6),
                            reader.GetDouble(8),
                            reader.GetString(4),
                            reader.GetInt32(5),
                            version);

                        var axisName = name.StartsWith(orientation, StringComparison.OrdinalIgnoreCase)
                            ? $"({abbreviation})"
                            : $"{name} ({abbreviation})";

                        yield return new(
                            "AXIS",
                            axisName,
                            new WellKnownTextLiteral(orientation),
                            new WellKnownTextNode("ORDER", reader.GetInt32(3)),
                            units);
                    }
                    while (reader.Read());
                }

                break;
            }
        }
    }
}