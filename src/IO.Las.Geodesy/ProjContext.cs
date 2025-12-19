// -----------------------------------------------------------------------
// <copyright file="ProjContext.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

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

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<VersionedUnitOfMeasure, WellKnownTextNode> UnitNodes = new();

    private readonly Microsoft.Data.Sqlite.SqliteConnection connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjContext"/> class.
    /// </summary>
    public ProjContext()
        : this(FileManager.FindProjDb())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjContext"/> class.
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
    }

    /// <inheritdoc cref="System.Data.Common.DbConnection.State" />
    public System.Data.ConnectionState State => this.connection.State;

    /// <inheritdoc cref="System.Data.Common.DbConnection.Open" />
    public void Open() => this.connection.Open();

    /// <inheritdoc cref="System.Data.Common.DbConnection.OpenAsync()" />
    public Task OpenAsync() => this.connection.OpenAsync();

    /// <inheritdoc cref="System.Data.Common.DbConnection.OpenAsync(CancellationToken)" />
    public Task OpenAsync(CancellationToken cancellationToken) => this.connection.OpenAsync(cancellationToken);

    /// <inheritdoc cref="System.Data.Common.DbConnection.Close" />
    public void Close() => this.connection.Close();

    /// <summary>
    /// Gets the <see cref="WellKnownTextNode"/> for the specified SRID.
    /// </summary>
    /// <param name="srid">The SRID.</param>
    /// <param name="version">The version.</param>
    /// <returns>The <see cref="WellKnownTextNode"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="srid"/> was not found.</exception>
    public WellKnownTextNode GetWellKnownTextNode(ushort srid, WellKnownTextVersion version = WellKnownTextVersion.Wkt1) =>
        this.TryGetWellKnownTextNode(srid, out var node, version)
            ? node
            : throw new KeyNotFoundException();

    /// <summary>
    /// Tries to get the <see cref="WellKnownTextNode"/> for the specified SRID.
    /// </summary>
    /// <param name="srid">The SRID.</param>
    /// <param name="node">The <see cref="WellKnownTextNode"/>.</param>
    /// <param name="version">The version.</param>
    /// <returns><see langword="true"/> if the SRID was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetWellKnownTextNode(ushort srid, out WellKnownTextNode node, WellKnownTextVersion version = WellKnownTextVersion.Wkt1)
    {
        const string DefaultAuthName = "EPSG";
        using var command = this.connection.CreateCommand();
        return TryGetGeodeticCoordinateReferenceSystem(command, DefaultAuthName, srid, version, out node)
               || TryGetProjectedCoordinateReferenceSystem(command, DefaultAuthName, srid, version, out node);
    }

    /// <summary>
    /// Gets the WKT for the specified SRID.
    /// </summary>
    /// <param name="srid">The SRID.</param>
    /// <param name="version">The version.</param>
    /// <returns>The WKT.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="srid"/> was not found.</exception>
    public string GetWkt(ushort srid, WellKnownTextVersion version = WellKnownTextVersion.Wkt1) =>
        this.TryGetWkt(srid, out var wkt, version)
            ? wkt
            : throw new KeyNotFoundException();

    /// <summary>
    /// Tries to get the WKT for the specified SRID.
    /// </summary>
    /// <param name="srid">The SRID.</param>
    /// <param name="wkt">The WKT.</param>
    /// <param name="version">The version.</param>
    /// <returns><see langword="true"/> if the SRID was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetWkt(ushort srid, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? wkt, WellKnownTextVersion version = WellKnownTextVersion.Wkt1)
    {
        if (this.TryGetWellKnownTextNode(srid, out var node, version))
        {
            wkt = node.ToString();
            return true;
        }

        wkt = default;
        return false;
    }

    /// <summary>
    /// Gets a value indicating whether the specified SRID represents a Geodetic Coordinate Reference System.
    /// </summary>
    /// <param name="srid">The SRID.</param>
    /// <returns><see langword="true"/> is <paramref name="srid"/> represents a Geodetic Coordinate Reference System; otherwise <see langword="false"/>.</returns>
    public bool IsGeodeticCoordinateReferenceSystem(ushort srid)
    {
        const string DefaultAuthName = "EPSG";
        using var command = this.connection.CreateCommand();
        return IsGeodeticCoordinateReferenceSystem(command, DefaultAuthName, srid);
    }

    /// <summary>
    /// Gets a value indicating whether the specified SRID represents a Projected Coordinate Reference System.
    /// </summary>
    /// <param name="srid">The SRID.</param>
    /// <returns><see langword="true"/> is <paramref name="srid"/> represents a Projected Coordinate Reference System; otherwise <see langword="false"/>.</returns>
    public bool IsProjectedCoordinateReferenceSystem(ushort srid)
    {
        const string DefaultAuthName = "EPSG";
        using var command = this.connection.CreateCommand();
        return IsProjectedCoordinateReferenceSystem(command, DefaultAuthName, srid);
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
        if (alias?.Contains('.', StringComparison.Ordinal) is false)
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
            command.CommandText += string.Create(System.Globalization.CultureInfo.InvariantCulture, $" LIMIT {limit}");
        }

        _ = AddOrUpdate(command.Parameters, $"${prefix}{AuthNameField}", authName);
        _ = AddOrUpdate(command.Parameters, $"${prefix}{CodeField}", code);

        static Microsoft.Data.Sqlite.SqliteParameter AddOrUpdate(Microsoft.Data.Sqlite.SqliteParameterCollection parameters, string parameterName, object value)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (!string.Equals(parameter.ParameterName, parameterName, StringComparison.Ordinal))
                {
                    continue;
                }

                parameter.Value = value;
                return parameter;
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
        UnitNodes.GetOrAdd(
            new(uom, version),
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

    private static string GetUnitName(string name)
    {
        var index = name.IndexOf('(', StringComparison.Ordinal);
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

            return name[..(i + 1)];
        }

        return string.Empty;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private readonly record struct VersionedUnitOfMeasure(int Code, WellKnownTextVersion Version);
}