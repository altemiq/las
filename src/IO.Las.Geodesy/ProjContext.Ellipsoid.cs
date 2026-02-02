// -----------------------------------------------------------------------
// <copyright file="ProjContext.Ellipsoid.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <content>
/// Ellipsoid methods.
/// </content>
public partial class ProjContext
{
    private const string EllipsoidName = "ellipsoid";

    private const string EllipsoidAlias = "e";

    private const string EllipsoidClause = $"{EllipsoidAlias}.{DeprecatedField} = 0";

    private const string EllipsoidMetadataFields = $"{EllipsoidAlias}.{NameField} {EllipsoidName}_{NameField}, {EllipsoidAlias}.{AuthNameField} {EllipsoidName}_{AuthNameField}, {EllipsoidAlias}.{CodeField} {EllipsoidName}_{CodeField}";

    private const string EllipsoidFields = $"{EllipsoidMetadataFields}, {EllipsoidAlias}.semi_major_axis, {EllipsoidAlias}.inv_flattening, {EllipsoidAlias}.semi_minor_axis, {EllipsoidAlias}.uom_auth_name, {EllipsoidAlias}.uom_code";

    private const string GetEllipsoidCommandText = $"SELECT {EllipsoidFields} FROM {EllipsoidName} {EllipsoidAlias} WHERE {EllipsoidClause}";

    private static WellKnownTextNode GetEllipsoid(Microsoft.Data.Sqlite.SqliteCommand command, string authority, int code, WellKnownTextVersion version)
    {
        command.CommandText = GetEllipsoidCommandText;
        AddAuthClause(command, authority, code, limit: 1);

        IList<WellKnownTextValue> values = [];
        string authName;
        int authCode;
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read())
            {
                throw new KeyNotFoundException();
            }

            var semiMajorAxis = reader.GetDouble(3);
            var inverseFlattening = reader.IsDBNull(4)
                ? semiMajorAxis / (semiMajorAxis - reader.GetDouble(5))
                : reader.GetDouble(4);
            values.Add(reader.GetString(0));
            values.Add(semiMajorAxis);
            values.Add(inverseFlattening);
            authName = reader.GetString(6);
            authCode = reader.GetInt32(7);
        }

        string nodeName;
        switch (version)
        {
            case WellKnownTextVersion.Wkt1:
                nodeName = "SPHEROID";
                values.Add(CreateAuthorityNode(authority, code, WellKnownTextVersion.Wkt1));
                break;
            case WellKnownTextVersion.Wkt2_2015 or WellKnownTextVersion.Wkt2_2019:
                nodeName = "ELLIPSOID";
                values.Add(GetUnitNode(command, authName, authCode, WellKnownTextVersion.Wkt2_2015));
                break;
            default:
                throw new System.Diagnostics.UnreachableException();
        }

        return new(nodeName, values);
    }
}