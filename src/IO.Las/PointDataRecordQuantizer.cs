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
    public static (double X, double Y, double Z) Get(IBasePointDataRecord point, Vector3D scaleFactor, Vector3D offset) => Get(point.X, point.Y, point.Z, scaleFactor, offset);

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
    public static (double X, double Y, double Z) Get(int x, int y, int z, Vector3D scaleFactor, Vector3D offset)
#if NET7_0_OR_GREATER
    {
        var result = (System.Runtime.Intrinsics.Vector256.Create((double)x, y, z, default) * scaleFactor.AsVector256()) + offset.AsVector256();
        return (result[0], result[1], result[2]);
    }
#else
        => ((x * scaleFactor.X) + offset.X, (y * scaleFactor.Y) + offset.Y, (z * scaleFactor.Z) + offset.Z);
#endif

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="scaleFactor">The scale factor.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>The converted point.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static (double X, double Y) Get(int x, int y, Vector3D scaleFactor, Vector3D offset)
#if NET7_0_OR_GREATER
    {
        var result = (System.Runtime.Intrinsics.Vector256.Create(System.Runtime.Intrinsics.Vector128.Create((double)x, y), System.Runtime.Intrinsics.Vector128<double>.Zero) * scaleFactor.AsVector256()) + offset.AsVector256();
        return (result[0], result[1]);
    }
#else
        => ((x * scaleFactor.X) + offset.X, (y * scaleFactor.Y) + offset.Y);
#endif

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The converted point.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public (double X, double Y, double Z) Get(IBasePointDataRecord point) => this.Get(point.X, point.Y, point.Z);

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <returns>The converted point.</returns>
    public (double X, double Y, double Z) Get(int x, int y, int z) => Get(x, y, z, scaleFactor, offset);

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The converted point.</returns>
    public (double X, double Y) Get(int x, int y) => Get(x, y, scaleFactor, offset);

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <returns>The converted point.</returns>
    public (int X, int Y, int Z) Get(double x, double y, double z) => (this.GetX(x), this.GetY(y), this.GetZ(z));

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The converted point.</returns>
    public (int X, int Y) Get(double x, double y)
#if NET7_0_OR_GREATER
    {
        var vector = (System.Runtime.Intrinsics.Vector256.Create(
            System.Runtime.Intrinsics.Vector128.Create(x, y),
            System.Runtime.Intrinsics.Vector128<double>.Zero) - offset.AsVector256()) / scaleFactor.AsVector256();
#if NET9_0_OR_GREATER
        vector = System.Runtime.Intrinsics.Vector256.Round(vector, MidpointRounding.AwayFromZero);
        return ((int)vector[0], (int)vector[1]);
#else
        return ((int)Math.Round(vector[0], MidpointRounding.AwayFromZero), (int)Math.Round(vector[1], MidpointRounding.AwayFromZero));
#endif
    }
#else
        => (this.GetX(x), this.GetY(y));
#endif

    /// <summary>
    /// Converts the x-coordinate.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <returns>The converted x-coordinate.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int GetX(double x) => ConvertSingle(x, offset.X, scaleFactor.X);

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
    public int GetY(double y) => ConvertSingle(y, offset.Y, scaleFactor.Y);

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
    public int GetZ(double z) => ConvertSingle(z, offset.Z, scaleFactor.Z);

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

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static int ConvertSingle(double value, double offset, double scaleFactor) => value >= offset ? (int)(((value - offset) / scaleFactor) + 0.5) : (int)(((value - offset) / scaleFactor) - 0.5);
}