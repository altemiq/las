// -----------------------------------------------------------------------
// <copyright file="ProjContext.PrimeMeridian.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <content>
/// Prime Meridian methods.
/// </content>
public partial class ProjContext
{
    private const string PrimeMeridianName = "prime_meridian";

    private const string PrimeMeridianAlias = "pm";

    private const string PrimeMeridianClause = $"{PrimeMeridianAlias}.{DeprecatedField} = 0";

    private const string PrimeMeridianFields = $"""
                                                {PrimeMeridianAlias}.{NameField},
                                                {PrimeMeridianAlias}.{AuthNameField},
                                                {PrimeMeridianAlias}.{CodeField},
                                                {PrimeMeridianAlias}.longitude,
                                                {PrimeMeridianAlias}.uom_auth_name,
                                                {PrimeMeridianAlias}.uom_code
                                                """;

    private const string GetPrimeMeridianCommandText = $"""
                                                        SELECT
                                                            {PrimeMeridianFields}
                                                        FROM {PrimeMeridianName} {PrimeMeridianAlias}
                                                        WHERE {PrimeMeridianClause}
                                                        """;

    private static WellKnownTextNode GetPrimeMeridian(Microsoft.Data.Sqlite.SqliteCommand command, string authority, int code, WellKnownTextVersion version)
    {
        command.CommandText = GetPrimeMeridianCommandText;
        AddAuthClause(command, authority, code, limit: 1);

        IList<WellKnownTextValue> values = [];
        string uomAuthName;
        int uomCode;
        using (var reader = command.ExecuteReader())
        {
            values.Add(reader.GetString(0));
            values.Add(reader.GetDouble(3));
            uomAuthName = reader.GetString(4);
            uomCode = reader.GetInt32(5);
        }

        switch (version)
        {
            case WellKnownTextVersion.Wkt1:
                values.Add(CreateAuthorityNode(authority, code, version));
                break;
            case WellKnownTextVersion.Wkt2_2015 or WellKnownTextVersion.Wkt2_2019:
                values.Add(GetUnitNode(command, uomAuthName, uomCode, version));
                break;
        }

        return new("PRIMEM", values);
    }
}