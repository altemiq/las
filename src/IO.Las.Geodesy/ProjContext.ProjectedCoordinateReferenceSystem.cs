// -----------------------------------------------------------------------
// <copyright file="ProjContext.ProjectedCoordinateReferenceSystem.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <content>
/// Projected Coordinate Reference System methods.
/// </content>
public partial class ProjContext
{
    private const string ProjectedCoordinateSystemName = "projected_crs";

    private const string ProjectedCoordinateSystemAlias = "pcrs";

    private const string ProjectedCoordinateSystemCommandText = $"""
                                                                 SELECT
                                                                      {ProjectedCoordinateSystemAlias}.{NameField},
                                                                      {ProjectedCoordinateSystemAlias}.{CoordinateSystemName}_{AuthNameField},
                                                                      {ProjectedCoordinateSystemAlias}.{CoordinateSystemName}_{CodeField},
                                                                      {ProjectedCoordinateSystemAlias}.{GeodeticCoordinateSystemName}_{AuthNameField},
                                                                      {ProjectedCoordinateSystemAlias}.{GeodeticCoordinateSystemName}_{CodeField},
                                                                      {ProjectedCoordinateSystemAlias}.conversion_{AuthNameField},
                                                                      {ProjectedCoordinateSystemAlias}.conversion_{CodeField}
                                                                 FROM {ProjectedCoordinateSystemName} {ProjectedCoordinateSystemAlias}
                                                                 WHERE {ProjectedCoordinateSystemAlias}.{DeprecatedField} = 0
                                                                 """;

    private const string ConversionName = "conversion";

    private const string ConversionAlias = "c";

    private const string ConversionClause = $"{ConversionAlias}.{DeprecatedField} = 0";

    private const string ConversionMetadataFields = $"{ConversionAlias}.{NameField}";

    private const string ConversionFields = $"""
                                             {ConversionMetadataFields},
                                             {ConversionAlias}.method_auth_name,
                                             {ConversionAlias}.method_code,
                                             {ConversionAlias}.method_name,
                                             {ConversionAlias}.param1_auth_name,
                                             {ConversionAlias}.param1_code,
                                             {ConversionAlias}.param1_name,
                                             {ConversionAlias}.param1_value,
                                             {ConversionAlias}.param1_uom_auth_name,
                                             {ConversionAlias}.param1_uom_code,
                                             {ConversionAlias}.param2_auth_name,
                                             {ConversionAlias}.param2_code,
                                             {ConversionAlias}.param2_name,
                                             {ConversionAlias}.param2_value,
                                             {ConversionAlias}.param2_uom_auth_name,
                                             {ConversionAlias}.param2_uom_code,
                                             {ConversionAlias}.param3_auth_name,
                                             {ConversionAlias}.param3_code,
                                             {ConversionAlias}.param3_name,
                                             {ConversionAlias}.param3_value,
                                             {ConversionAlias}.param3_uom_auth_name,
                                             {ConversionAlias}.param3_uom_code,
                                             {ConversionAlias}.param4_auth_name,
                                             {ConversionAlias}.param4_code,
                                             {ConversionAlias}.param4_name,
                                             {ConversionAlias}.param4_value,
                                             {ConversionAlias}.param4_uom_auth_name,
                                             {ConversionAlias}.param4_uom_code,
                                             {ConversionAlias}.param5_auth_name,
                                             {ConversionAlias}.param5_code,
                                             {ConversionAlias}.param5_name,
                                             {ConversionAlias}.param5_value,
                                             {ConversionAlias}.param5_uom_auth_name,
                                             {ConversionAlias}.param5_uom_code,
                                             {ConversionAlias}.param6_auth_name,
                                             {ConversionAlias}.param6_code,
                                             {ConversionAlias}.param6_name,
                                             {ConversionAlias}.param6_value,
                                             {ConversionAlias}.param6_uom_auth_name,
                                             {ConversionAlias}.param6_uom_code,
                                             {ConversionAlias}.param7_auth_name,
                                             {ConversionAlias}.param7_code,
                                             {ConversionAlias}.param7_name,
                                             {ConversionAlias}.param7_value,
                                             {ConversionAlias}.param7_uom_auth_name,
                                             {ConversionAlias}.param7_uom_code
                                             """;

    private const string ConversionCommandText = $"""
                                                  SELECT
                                                      {ConversionFields}
                                                  FROM
                                                      {ConversionName} {ConversionAlias}
                                                  WHERE
                                                      {ConversionClause}
                                                  """;

    private static bool IsProjectedCoordinateReferenceSystem(Microsoft.Data.Sqlite.SqliteCommand command, string auth, int code)
    {
        command.CommandText = $"SELECT {ProjectedCoordinateSystemAlias}.{CodeField} FROM {ProjectedCoordinateSystemName} {ProjectedCoordinateSystemAlias} WHERE {ProjectedCoordinateSystemAlias}.{DeprecatedField} = 0";
        AddAuthClause(command, auth, code, limit: 1);
        using var reader = command.ExecuteReader();
        return reader.Read();
    }

    private static WellKnownTextNode GetProjectedCoordinateReferenceSystem(Microsoft.Data.Sqlite.SqliteCommand command, string auth, int code, WellKnownTextVersion version)
    {
        command.CommandText = ProjectedCoordinateSystemCommandText;
        AddAuthClause(command, auth, code, limit: 1);
        string coordinateSystemAuthName;
        int coordinateSystemCode;
        string geodeticCoordinateSystemAuthName;
        int geodeticCoordinateSystemCode;
        string conversionAuthName;
        int conversionCode;
        IList<WellKnownTextValue> values = [];
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read())
            {
                throw new KeyNotFoundException();
            }

            values.Add(reader.GetString(0));
            coordinateSystemAuthName = reader.GetString(1);
            coordinateSystemCode = reader.GetInt32(2);
            geodeticCoordinateSystemAuthName = reader.GetString(3);
            geodeticCoordinateSystemCode = reader.GetInt32(4);
            conversionAuthName = reader.GetString(5);
            conversionCode = reader.GetInt32(6);
        }

        // get the base GCS
        values.Add(GetGeodeticCoordinateReferenceSystem(command, geodeticCoordinateSystemAuthName, geodeticCoordinateSystemCode, version, @base: true));

        // get the conversion
        foreach (var item in GetConversion(command, conversionAuthName, conversionCode, version))
        {
            values.Add(item);
        }

        // get the coordinate system
        foreach (var item in GetCoordinateSystem(command, coordinateSystemAuthName, coordinateSystemCode, version))
        {
            values.Add(item);
        }

        if (version is WellKnownTextVersion.Wkt2_2015 or WellKnownTextVersion.Wkt2_2019)
        {
            foreach (var item in GetUsage(command, ProjectedCoordinateSystemName, auth, code, version))
            {
                values.Add(item);
            }
        }

        values.Add(CreateAuthorityNode(auth, code, version));

        return new(version is WellKnownTextVersion.Wkt1 ? "PROJCS" : "PROJCRS", values);
    }

    private static IEnumerable<WellKnownTextNode> GetConversion(Microsoft.Data.Sqlite.SqliteCommand command, string auth, int code, WellKnownTextVersion version)
    {
        command.CommandText = ConversionCommandText;
        AddAuthClause(command, auth, code);

        switch (version)
        {
            case WellKnownTextVersion.Wkt1:
                {
                    using var reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        throw new KeyNotFoundException();
                    }

                    var projectionName = reader.GetString(3) switch
                    {
                        "Transverse Mercator" or "Transverse Mercator (3D)" => "Transverse_Mercator",
                        "Transverse Mercator (South Orientated)" => "Transverse_Mercator_South_Orientated",
                        "Albers Equal Area" => "Albers_Conic_Equal_Area",
                        "Lambert Conic Conformal (1SP)" => "Lambert_Conformal_Conic_1SP",
                        "Lambert Conic Conformal (2SP)" => "Lambert_Conformal_Conic_2SP",
                        "Lambert Conic Conformal (2SP Belgium)" => "Lambert_Conformal_Conic_2SP_Belgium",
                        "Modified Azimuthal Equidistant" => "Azimuthal_Equidistant",
                        "Lambert Cylindrical Equal Area" or "Lambert Cylindrical Equal Area (Spherical)" => "Cylindrical_Equal_Area",
                        "Cassini-Soldner" => "Cassini_Soldner",
                        "Equidistant Cylindrical" or "Equidistant Cylindrical (Spherical)" => "Equirectangular",
                        "Hotine Oblique Mercator (variant A)" => "Hotine_Oblique_Mercator",
                        "Hotine Oblique Mercator (variant B)" => "Hotine_Oblique_Mercator_Azimuth_Center",
                        "Krovak (North Orientated)" => "Krovak",
                        "Lambert Azimuthal Equal Area (Spherical)" => "Lambert_Azimuthal_Equal_Area",
                        "Mercator (variant A)" => "Mercator_1SP",
                        "Mercator (variant B)" => "Mercator_2SP",
                        "Local Orthographic" => "Local Orthographic",
                        "American Polyconic" => "Polyconic",
                        "Polar Stereographic (variant A)" or "Polar Stereographic (variant B)" => "Polar_Stereographic",
                        var name => name.Replace(' ', '_'),
                    };

                    yield return new("PROJECTION", projectionName);

                    foreach (var parameter in GetParameters(reader))
                    {
                        var parameterName = parameter switch
                        {
                            { Code: 8801 or 8821 or 8832 } => "latitude_of_origin",
                            { Code: 8802 or 8822 } => "central_meridian",
                            { Code: 8805 or 8815 or 8819 } => "scale_factor",
                            { Code: 8806 or 8826 or 8816 } => "false_easting",
                            { Code: 8807 or 8827 or 8817 } => "false_northing",
                            { Code: 8823 } => "standard_parallel_1",
                            { Code: 8824 } => "standard_parallel_2",
                            { Code: 8811 } => "latitude_of_center",
                            { Code: 8812 } => "longitude_of_center",
                            { Code: 1036 or 8813 } => "azimuth",
                            { Code: 8814 } => "rectified_grid_angle",
                            { Code: 8818 } => "pseudo_standard_parallel_1",
                            { Code: 8833 } => "longitude_of_center",
                            { Name: var n } => n.ToLowerInvariant().Replace(' ', '_'),
                        };

                        yield return new("PARAMETER", parameterName, parameter.Value);
                    }

                    break;
                }

            case WellKnownTextVersion.Wkt2_2015 or WellKnownTextVersion.Wkt2_2019:
                {
                    IList<WellKnownTextValue> values = [];
                    IEnumerable<Parameter> parameters;
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new KeyNotFoundException();
                        }

                        values.Add(reader.GetString(0));

                        // get the method
                        values.Add(new WellKnownTextNode(
                            "METHOD",
                            reader.GetString(3),
                            CreateAuthorityNode(reader.GetString(1), reader.GetInt32(2), version)));

                        parameters = GetParameters(reader);
                    }

                    foreach (var parameter in parameters)
                    {
                        values.Add(parameter.GetNode(command, version));
                    }

                    yield return new("CONVERSION", values);
                    break;
                }
        }

        static IEnumerable<Parameter> GetParameters(Microsoft.Data.Sqlite.SqliteDataReader reader)
        {
            // get each value
            const int Offset = 4;
            IList<Parameter> parameters = [];

            for (var i = 0; i < 7; i++)
            {
                var start = Offset + (i * 6);
                if (reader.IsDBNull(start))
                {
                    break;
                }

                parameters.Add(new(
                    reader.GetString(start + 2),
                    reader.GetDouble(start + 3),
                    reader.GetString(start),
                    reader.GetInt32(start + 1),
                    new(reader.GetString(start + 4), reader.GetInt32(start + 5))));
            }

            return parameters;
        }
    }

    private static bool TryGetProjectedCoordinateReferenceSystem(Microsoft.Data.Sqlite.SqliteCommand command, string auth, int code, WellKnownTextVersion version, out WellKnownTextNode node)
    {
        if (IsProjectedCoordinateReferenceSystem(command, auth, code))
        {
            node = GetProjectedCoordinateReferenceSystem(command, auth, code, version);
            return true;
        }

        node = default;
        return false;
    }

    private sealed record Parameter(string Name, double Value, string AuthName, int Code, UnitOfMeasure UnitOfMeasure)
    {
        public WellKnownTextNode GetNode(Microsoft.Data.Sqlite.SqliteCommand command, WellKnownTextVersion version) =>
            new(
                "PARAMETER",
                this.Name,
                this.Value,
                this.UnitOfMeasure.GetNode(command, version),
                CreateAuthorityNode(this.AuthName, this.Code, version));
    }

    private sealed record UnitOfMeasure(string AuthName, int Code)
    {
        public WellKnownTextNode GetNode(Microsoft.Data.Sqlite.SqliteCommand command, WellKnownTextVersion version) => GetUnitNode(command, this.AuthName, this.Code, version);
    }
}