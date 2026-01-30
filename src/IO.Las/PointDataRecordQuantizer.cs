// -----------------------------------------------------------------------
// <copyright file="PointDataRecordQuantizer.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The point converter.
/// </summary>
/// <param name="scaleFactor">The scale factor.</param>
/// <param name="offset">The offset.</param>
/// <param name="gpsOffset">The GPS offset.</param>
public sealed class PointDataRecordQuantizer(Vector3D scaleFactor, Vector3D offset, double gpsOffset)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointDataRecordQuantizer"/> class.
    /// </summary>
    /// <param name="header">The header block.</param>
    public PointDataRecordQuantizer(in HeaderBlock header)
        : this(header.ScaleFactor, header.Offset, GpsTime.GetOffset(header))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointDataRecordQuantizer"/> class.
    /// </summary>
    public PointDataRecordQuantizer()
        : this(new(0.01, 0.01, 0.01), Vector3D.Zero, GpsTime.StandardOffset)
    {
    }

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="scaleFactor">The scale factor.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>The converted point.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Vector3D Get(IBasePointDataRecord point, Vector3D scaleFactor, Vector3D offset) => Get(point.X, point.Y, point.Z, scaleFactor, offset);

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <param name="scaleFactor">The scale factor.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>The converted point.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Vector3D Get(int x, int y, int z, Vector3D scaleFactor, Vector3D offset) => (new Vector3D(x, y, z) * scaleFactor) + offset;

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="scaleFactor">The scale factor.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>The converted point.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Vector2D Get(int x, int y, Vector3D scaleFactor, Vector3D offset) => (new Vector2D(x, y) * scaleFactor.AsVector2D()) + offset.AsVector2D();

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The converted point.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Vector3D Get(IBasePointDataRecord point) => this.Get(point.X, point.Y, point.Z);

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <returns>The converted point.</returns>
    public Vector3D Get(int x, int y, int z) => Get(x, y, z, scaleFactor, offset);

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The converted point.</returns>
    public Vector2D Get(int x, int y) => Get(x, y, scaleFactor, offset);

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <returns>The converted point.</returns>
    public (int X, int Y, int Z) Get(double x, double y, double z) => this.Get(new Vector3D(x, y, z));

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="vector">The coordinate.</param>
    /// <returns>The converted point.</returns>
    public (int X, int Y, int Z) Get(Vector3D vector)
    {
        var rounded = Vector3D.Round((vector - offset) / scaleFactor, MidpointRounding.AwayFromZero);
        return ((int)rounded.X, (int)rounded.Y, (int)rounded.Z);
    }

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The converted point.</returns>
    public (int X, int Y) Get(double x, double y) => this.Get(new Vector2D(x, y));

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="vector">The coordinate.</param>
    /// <returns>The converted point.</returns>
    public (int X, int Y) Get(Vector2D vector)
    {
        var rounded = Vector2D.Round((vector - offset.AsVector2D()) / scaleFactor.AsVector2D(), MidpointRounding.AwayFromZero);
        return ((int)rounded.X, (int)rounded.Y);
    }

    /// <summary>
    /// Converts the x-coordinate.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <returns>The converted x-coordinate.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int GetX(double x) => (int)Math.Round((x - offset.X) / scaleFactor.X, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Converts the x-coordinate.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <returns>The converted x-coordinate.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public double GetX(int x) => (x * scaleFactor.X) + offset.X;

    /// <summary>
    /// Converts the y-coordinate.
    /// </summary>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The converted y-coordinate.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int GetY(double y) => (int)Math.Round((y - offset.Y) / scaleFactor.Y, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Converts the y-coordinate.
    /// </summary>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The converted y-coordinate.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public double GetY(int y) => (y * scaleFactor.Y) + offset.Y;

    /// <summary>
    /// Converts the z-coordinate.
    /// </summary>
    /// <param name="z">The z-coordinate.</param>
    /// <returns>The converted z-coordinate.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int GetZ(double z) => (int)Math.Round((z - offset.Z) / scaleFactor.Z, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Converts the z-coordinate.
    /// </summary>
    /// <param name="z">The z-coordinate.</param>
    /// <returns>The converted z-coordinate.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public double GetZ(int z) => (z * scaleFactor.Z) + offset.Z;

    /// <summary>
    /// Converts the GPS Time.
    /// </summary>
    /// <param name="gpsTime">The GPS time.</param>
    /// <returns>The date time.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public DateTime GetDateTime(double gpsTime) => GpsTime.GpsTimeToDateTime(gpsTime, gpsOffset);

    /// <summary>
    /// Converts the GPS Time.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <returns>The GPS time.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public double GetGpsTime(DateTime dateTime) => GpsTime.DateTimeToGpsTime(dateTime, gpsOffset);
}