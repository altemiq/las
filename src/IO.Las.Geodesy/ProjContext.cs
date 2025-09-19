// -----------------------------------------------------------------------
// <copyright file="ProjContext.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

using System.Collections.Concurrent;

/// <summary>
/// The PROJ context.
/// </summary>
public sealed partial class ProjContext :
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
    IAsyncDisposable,
#endif
    IDisposable
{
    private const string NameField = "name";

    private const string AuthNameField = "auth_name";

    private const string CodeField = "code";

    private const string DeprecatedField = "deprecated";

    private static readonly ConcurrentDictionary<(int Code, WellKnownTextVersion Version), WellKnownTextNode> UnitNodes = new();

    private readonly Microsoft.Data.Sqlite.SqliteConnection connection;

    /// <summary>
    /// Initialises a new instance of the <see cref="ProjContext"/> class.
    /// </summary>
    public ProjContext()
        : this(FileManager.FindProjDb())
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ProjContext"/> class.
    /// </summary>
    /// <param name="path">The PROJ database path.</param>
    public ProjContext(string path)
    {
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly,
            Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Private,
        };

        this.connection = new(builder.ConnectionString);
        this.connection.Open();
    }

    /// <summary>
    /// Gets the WKT for the specified SRID.
    /// </summary>
    /// <param name="srid">The SRID.</param>
    /// <param name="version">The version.</param>
    /// <returns>The WKT.</returns>
    public string GetWkt(ushort srid, WellKnownTextVersion version = WellKnownTextVersion.Wkt1)
    {
        return TryGetCoordinateReferenceSystem(out var node)
            ? node.ToString()
            : throw new KeyNotFoundException();

        bool TryGetCoordinateReferenceSystem(out WellKnownTextNode output)
        {
            const string DefaultAuthName = "EPSG";
            using var command = this.connection.CreateCommand();
            return TryGetGeodeticCoordinateReferenceSystem(command, DefaultAuthName, srid, version, out output)
                   || TryGetProjectedCoordinateReferenceSystem(command, DefaultAuthName, srid, version, out output);
        }
    }

    /// <inheritdoc />
    public void Dispose() => this.connection.Dispose();

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
    /// <inheritdoc />
    public ValueTask DisposeAsync() => this.connection.DisposeAsync();
#endif

    private static WellKnownTextNode CreateAuthorityNode(string auth, int code, WellKnownTextVersion version) => CreateAuthorityNode(auth, code.ToString(System.Globalization.CultureInfo.InvariantCulture), version);

    private static WellKnownTextNode CreateAuthorityNode(string auth, string value, WellKnownTextVersion version) =>
        version switch
        {
            WellKnownTextVersion.Wkt1 => new("AUTHORITY", auth, value),
            WellKnownTextVersion.Wkt2_2015 or WellKnownTextVersion.Wkt2_2019 => new("ID", auth, new WellKnownTextLiteral(value)),
            _ => throw new ArgumentOutOfRangeException(nameof(version)),
        };

    private static void AddAuthClause(Microsoft.Data.Sqlite.SqliteCommand command, string authName, object code, string? alias = default, string? prefix = default, string? orderBy = default, int limit = default)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (alias?.Contains('.', StringComparison.Ordinal) is false)
#else
        if (alias?.Contains('.') is false)
#endif
        {
            alias += ".";
        }

        command.CommandText += $" AND {alias}{prefix}{AuthNameField} = ${prefix}{AuthNameField} AND {alias}{prefix}{CodeField} = ${prefix}{CodeField}";
        if (orderBy is not null)
        {
            command.CommandText += $" ORDER BY {alias}{orderBy}";
        }

        if (limit != default)
        {
#if NET6_0_OR_GREATER
            command.CommandText += string.Create(System.Globalization.CultureInfo.InvariantCulture, $" LIMIT {limit}");
#else
            command.CommandText += FormattableString.Invariant($" LIMIT {limit}");
#endif
        }

        _ = AddOrUpdate(command.Parameters, $"${prefix}{AuthNameField}", authName);
        _ = AddOrUpdate(command.Parameters, $"${prefix}{CodeField}", code);

        static Microsoft.Data.Sqlite.SqliteParameter AddOrUpdate(Microsoft.Data.Sqlite.SqliteParameterCollection parameters, string parameterName, object value)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (string.Equals(parameter.ParameterName, parameterName, StringComparison.Ordinal))
                {
                    parameter.Value = value;
                    return parameter;
                }
            }

            return parameters.AddWithValue(parameterName, value);
        }
    }

    private static WellKnownTextNode GetUnitNode(string type, string name, double value, string auth, int code, WellKnownTextVersion version)
    {
        if (version is WellKnownTextVersion.Wkt1)
        {
            return new("UNIT", GetUnitName(name), value, CreateAuthorityNode(auth, code, WellKnownTextVersion.Wkt1));
        }

        return new($"{type.ToUpperInvariant()}UNIT", GetUnitName(name), value);
    }

    private static WellKnownTextNode GetUnitNode(Microsoft.Data.Sqlite.SqliteCommand command, string auth, int uom, WellKnownTextVersion version) =>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET472_OR_GREATER
        UnitNodes.GetOrAdd(
            (uom, version),
            static (key, a) =>
            {
                a.Command.CommandText = "SELECT type, name, conv_factor, auth_name, code FROM unit_of_measure WHERE deprecated = 0";
                AddAuthClause(a.Command, a.Auth, key.Code, limit: 1);
                using var reader = a.Command.ExecuteReader();
                return reader.Read()
                    ? GetUnitNode(reader.GetString(0), reader.GetString(1), reader.GetDouble(2), reader.GetString(3), reader.GetInt32(4), key.Version)
                    : default;
            },
            (Command: command, Auth: auth));
#else
        UnitNodes.GetOrAdd(
            (uom, version),
            key =>
            {
                command.CommandText = "SELECT type, name, conv_factor, auth_name, code FROM unit_of_measure WHERE deprecated = 0";
                AddAuthClause(command, auth, key.Code, limit: 1);
                using var reader = command.ExecuteReader();
                return reader.Read()
                    ? GetUnitNode(reader.GetString(0), reader.GetString(1), reader.GetDouble(2), reader.GetString(3), reader.GetInt32(4), key.Version)
                    : default;
            });
#endif

    private static string GetUnitName(string name)
    {
        var index =
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            name.IndexOf('(', StringComparison.Ordinal);
#else
            name.IndexOf('(');
#endif
        if (index is -1)
        {
            return name;
        }

        for (var i = index - 1; i >= 0; i--)
        {
            if (char.IsWhiteSpace(name[i]))
            {
                continue;
            }

            return name.Substring(0, i + 1);
        }

        return string.Empty;
    }
}