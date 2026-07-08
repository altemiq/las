// -----------------------------------------------------------------------
// <copyright file="PointDataRecordQuantizer.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

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
    public static Vector3D Get(int x, int y, int z, Vector3D scaleFactor, Vector3D offset) =>
#if NET9_0_OR_GREATER
        Vector256.FusedMultiplyAdd(new Vector3D(x, y, z).AsVector256Unsafe(), scaleFactor.AsVector256Unsafe(), offset.AsVector256Unsafe()).AsVector3D();
#elif NETCOREAPP3_0_OR_GREATER
        Fma.IsSupported
            ? Fma.MultiplyAdd(new Vector3D(x, y, z).AsVector256Unsafe(), scaleFactor.AsVector256Unsafe(), offset.AsVector256Unsafe()).AsVector3D()
            : (new Vector3D(x, y, z) * scaleFactor) + offset;
#else
        (new Vector3D(x, y, z) * scaleFactor) + offset;
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
    public static Vector2D Get(int x, int y, Vector3D scaleFactor, Vector3D offset) =>
#if NET9_0_OR_GREATER
        Vector128.FusedMultiplyAdd(new Vector2D(x, y).AsVector128Unsafe(), scaleFactor.AsVector2D().AsVector128Unsafe(), offset.AsVector2D().AsVector128Unsafe()).AsVector2D();
#elif NETCOREAPP3_0_OR_GREATER
        Fma.IsSupported
            ? Fma.MultiplyAdd(new Vector2D(x, y).AsVector128Unsafe(), scaleFactor.AsVector2D().AsVector128Unsafe(), offset.AsVector2D().AsVector128Unsafe()).AsVector2D()
            : (new Vector2D(x, y) * scaleFactor.AsVector2D()) + offset.AsVector2D();
#else
        (new Vector2D(x, y) * scaleFactor.AsVector2D()) + offset.AsVector2D();
#endif

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

#if NETCOREAPP3_0_OR_GREATER
    /// <summary>
    /// Converts the points.
    /// </summary>
    /// <param name="x">The x-coordinates.</param>
    /// <param name="y">The y-coordinates.</param>
    /// <param name="z">The z-coordinates.</param>
    /// <param name="destination">The destination.</param>
    public void Quantize(ReadOnlySpan<int> x, ReadOnlySpan<int> y, ReadOnlySpan<int> z, Span<Vector3D> destination)
    {
        var length = x.Length;
        var i = 0;
        var size = Vector256<double>.Count;
        var simdLimit = length / size * size;

        var scaleX = Vector256.Create(scaleFactor.X);
        var offsetX = Vector256.Create(offset.X);
        var scaleY = Vector256.Create(scaleFactor.Y);
        var offsetY = Vector256.Create(offset.Y);
        var scaleZ = Vector256.Create(scaleFactor.Z);
        var offsetZ = Vector256.Create(offset.Z);

        while (i < simdLimit)
        {
#if NET7_0_OR_GREATER
            var inputX = Vector128.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(x[i..]));
            var inputY = Vector128.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(y[i..]));
            var inputZ = Vector128.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(z[i..]));
#else
            var inputX = Vector128.Create(x[i], x[i + 1], x[i + 2], x[i + 3]);
            var inputY = Vector128.Create(y[i], y[i + 1], y[i + 2], y[i + 3]);
            var inputZ = Vector128.Create(z[i], z[i + 1], z[i + 2], z[i + 3]);
#endif

#if NET9_0_OR_GREATER
            var quantizedX = Vector256.FusedMultiplyAdd(WidenToDouble(inputX), scaleX, offsetX);
            var quantizedY = Vector256.FusedMultiplyAdd(WidenToDouble(inputY), scaleY, offsetY);
            var quantizedZ = Vector256.FusedMultiplyAdd(WidenToDouble(inputZ), scaleZ, offsetZ);
#else
            Vector256<double> quantizedX;
            Vector256<double> quantizedY;
            Vector256<double> quantizedZ;
            if (Fma.IsSupported)
            {
                quantizedX = Fma.MultiplyAdd(WidenToDouble(inputX), scaleX, offsetX);
                quantizedY = Fma.MultiplyAdd(WidenToDouble(inputY), scaleY, offsetY);
                quantizedZ = Fma.MultiplyAdd(WidenToDouble(inputZ), scaleZ, offsetZ);
            }
            else
            {
                quantizedX = Vector256.Add(Vector256.Multiply(WidenToDouble(inputX), scaleX), offsetX);
                quantizedY = Vector256.Add(Vector256.Multiply(WidenToDouble(inputY), scaleY), offsetY);
                quantizedZ = Vector256.Add(Vector256.Multiply(WidenToDouble(inputZ), scaleZ), offsetZ);
            }
#endif

            for (var j = 0; j < size; j++)
            {
                destination[i + j] =
#if NET7_0_OR_GREATER
                    new(quantizedX[j], quantizedY[j], quantizedZ[j]);
#else
                    new(quantizedX.GetElement(j), quantizedY.GetElement(j), quantizedZ.GetElement(j));
#endif
            }

            i += size;
        }

        for (; i < length; i++)
        {
            destination[i] = Get(x[i], y[i], z[i], scaleFactor, offset);
        }
    }

    /// <summary>
    /// Converts the points.
    /// </summary>
    /// <param name="x">The x-coordinates.</param>
    /// <param name="y">The y-coordinates.</param>
    /// <param name="z">The z-coordinates.</param>
    /// <param name="destination">The destination.</param>
    public void Quantize(ReadOnlySpan<int> x, ReadOnlySpan<int> y, ReadOnlySpan<int> z, Span<System.Numerics.Vector3> destination)
    {
        var length = x.Length;
        var i = 0;
        var size = Vector256<float>.Count;
        var simdLimit = length / size * size;

        var scaleFactorF = new System.Numerics.Vector3((float)scaleFactor.X, (float)scaleFactor.Y, (float)scaleFactor.Z);
        var offsetF = new System.Numerics.Vector3((float)offset.X, (float)offset.Y, (float)offset.Z);

        var scaleX = Vector256.Create(scaleFactorF.X);
        var offsetX = Vector256.Create(offsetF.X);
        var scaleY = Vector256.Create(scaleFactorF.Y);
        var offsetY = Vector256.Create(offsetF.Y);
        var scaleZ = Vector256.Create(scaleFactorF.Z);
        var offsetZ = Vector256.Create(offsetF.Z);

        while (i < simdLimit)
        {
#if NET7_0_OR_GREATER
            var inputX = Vector256.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(x[i..]));
            var inputY = Vector256.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(y[i..]));
            var inputZ = Vector256.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(z[i..]));
#else
            var inputX =
                Vector256.Create(
                    Vector128.Create(x[i], x[i + 1], x[i + 2], x[i + 3]),
                    Vector128.Create(x[i + 4], x[i + 5], x[i + 6], x[i + 7]));
            var inputY =
                Vector256.Create(
                    Vector128.Create(y[i], y[i + 1], y[i + 2], y[i + 3]),
                    Vector128.Create(y[i + 4], y[i + 5], y[i + 6], y[i + 7]));
            var inputZ =
                Vector256.Create(
                    Vector128.Create(z[i], z[i + 1], z[i + 2], z[i + 3]),
                    Vector128.Create(z[i + 4], z[i + 5], z[i + 6], z[i + 7]));
#endif

#if NET9_0_OR_GREATER
            var quantizedX = Vector256.FusedMultiplyAdd(WidenToSingle(inputX), scaleX, offsetX);
            var quantizedY = Vector256.FusedMultiplyAdd(WidenToSingle(inputY), scaleY, offsetY);
            var quantizedZ = Vector256.FusedMultiplyAdd(WidenToSingle(inputZ), scaleZ, offsetZ);
#elif NET7_0_OR_GREATER
            Vector256<float> quantizedX;
            Vector256<float> quantizedY;
            Vector256<float> quantizedZ;
            if (Fma.IsSupported)
            {
                quantizedX = Fma.MultiplyAdd(WidenToSingle(inputX), scaleX, offsetX);
                quantizedY = Fma.MultiplyAdd(WidenToSingle(inputY), scaleY, offsetY);
                quantizedZ = Fma.MultiplyAdd(WidenToSingle(inputZ), scaleZ, offsetZ);
            }
            else
            {
                quantizedX = Vector256.Add(Vector256.Multiply(WidenToSingle(inputX), scaleX), offsetX);
                quantizedY = Vector256.Add(Vector256.Multiply(WidenToSingle(inputY), scaleY), offsetY);
                quantizedZ = Vector256.Add(Vector256.Multiply(WidenToSingle(inputZ), scaleZ), offsetZ);
            }
#else
            var quantizedX = ScaleAndOffset(inputX, scaleX, offsetX);
            var quantizedY = ScaleAndOffset(inputY, scaleY, offsetY);
            var quantizedZ = ScaleAndOffset(inputZ, scaleZ, offsetZ);
#endif

            for (var j = 0; j < size; j++)
            {
                destination[i + j] =
#if NET7_0_OR_GREATER
                    new(quantizedX[j], quantizedY[j], quantizedZ[j]);
#else
                    new(quantizedX.GetElement(j), quantizedY.GetElement(j), quantizedZ.GetElement(j));
#endif
            }

            i += size;
        }

        for (; i < length; i++)
        {
            destination[i] = (new System.Numerics.Vector3(x[i], y[i], z[i]) * scaleFactorF) + offsetF;
        }
    }

    /// <summary>
    /// Converts the point.
    /// </summary>
    /// <param name="x">The x-coordinates.</param>
    /// <param name="y">The y-coordinates.</param>
    /// <param name="destination">The destination.</param>
    public void Quantize(ReadOnlySpan<int> x, ReadOnlySpan<int> y, Span<Vector2D> destination)
    {
        var length = x.Length;
        var i = 0;
        var size = Vector256<double>.Count;
        var simdLimit = length / size * size;

        var scaleX = Vector256.Create(scaleFactor.X);
        var offsetX = Vector256.Create(offset.X);
        var scaleY = Vector256.Create(scaleFactor.Y);
        var offsetY = Vector256.Create(offset.Y);

        while (i < simdLimit)
        {
#if NET7_0_OR_GREATER
            var inputX = Vector128.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(x[i..]));
            var inputY = Vector128.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(y[i..]));
#else
            var inputX = Vector128.Create(x[i], x[i + 1], x[i + 2], x[i + 3]);
            var inputY = Vector128.Create(y[i], y[i + 1], y[i + 2], y[i + 3]);
#endif

#if NET9_0_OR_GREATER
            var quantizedX = Vector256.FusedMultiplyAdd(WidenToDouble(inputX), scaleX, offsetX);
            var quantizedY = Vector256.FusedMultiplyAdd(WidenToDouble(inputY), scaleY, offsetY);
#else
            Vector256<double> quantizedX;
            Vector256<double> quantizedY;
            if (Fma.IsSupported)
            {
                quantizedX = Fma.MultiplyAdd(WidenToDouble(inputX), scaleX, offsetX);
                quantizedY = Fma.MultiplyAdd(WidenToDouble(inputY), scaleY, offsetY);
            }
            else
            {
                quantizedX = Vector256.Add(Vector256.Multiply(WidenToDouble(inputX), scaleX), offsetX);
                quantizedY = Vector256.Add(Vector256.Multiply(WidenToDouble(inputY), scaleY), offsetY);
            }
#endif

            for (var j = 0; j < size; j++)
            {
                destination[i + j] =
#if NET7_0_OR_GREATER
                    new(quantizedX[j], quantizedY[j]);
#else
                    new(quantizedX.GetElement(j), quantizedY.GetElement(j));
#endif
            }

            i += size;
        }

        for (; i < length; i++)
        {
            destination[i] = Get(x[i], y[i], scaleFactor, offset);
        }
    }

    /// <summary>
    /// Converts the points.
    /// </summary>
    /// <param name="x">The x-coordinates.</param>
    /// <param name="y">The y-coordinates.</param>
    /// <param name="destination">The destination.</param>
    public void Quantize(ReadOnlySpan<int> x, ReadOnlySpan<int> y, Span<System.Numerics.Vector2> destination)
    {
        var length = x.Length;
        var i = 0;
        var size = Vector256<float>.Count;
        var simdLimit = length / size * size;

        var scaleFactorF = new System.Numerics.Vector2((float)scaleFactor.X, (float)scaleFactor.Y);
        var offsetF = new System.Numerics.Vector2((float)offset.X, (float)offset.Y);

        var scaleX = Vector256.Create(scaleFactorF.X);
        var offsetX = Vector256.Create(offsetF.X);
        var scaleY = Vector256.Create(scaleFactorF.Y);
        var offsetY = Vector256.Create(offsetF.Y);

        while (i < simdLimit)
        {
#if NET7_0_OR_GREATER
            var inputX = Vector256.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(x[i..]));
            var inputY = Vector256.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(y[i..]));
#else
            var inputX =
                Vector256.Create(
                    Vector128.Create(x[i], x[i + 1], x[i + 2], x[i + 3]),
                    Vector128.Create(x[i + 4], x[i + 5], x[i + 6], x[i + 7]));
            var inputY =
                Vector256.Create(
                    Vector128.Create(y[i], y[i + 1], y[i + 2], y[i + 3]),
                    Vector128.Create(y[i + 4], y[i + 5], y[i + 6], y[i + 7]));
#endif

#if NET9_0_OR_GREATER
            var quantizedX = Vector256.FusedMultiplyAdd(WidenToSingle(inputX), scaleX, offsetX);
            var quantizedY = Vector256.FusedMultiplyAdd(WidenToSingle(inputY), scaleY, offsetY);
#elif NET7_0_OR_GREATER
            Vector256<float> quantizedX;
            Vector256<float> quantizedY;
            if (Fma.IsSupported)
            {
                quantizedX = Fma.MultiplyAdd(WidenToSingle(inputX), scaleX, offsetX);
                quantizedY = Fma.MultiplyAdd(WidenToSingle(inputY), scaleY, offsetY);
            }
            else
            {
                quantizedX = Vector256.Add(Vector256.Multiply(WidenToSingle(inputX), scaleX), offsetX);
                quantizedY = Vector256.Add(Vector256.Multiply(WidenToSingle(inputY), scaleY), offsetY);
            }
#else
            var quantizedX = ScaleAndOffset(inputX, scaleX, offsetX);
            var quantizedY = ScaleAndOffset(inputY, scaleY, offsetY);
#endif

            for (var j = 0; j < size; j++)
            {
                destination[i + j] =
#if NET7_0_OR_GREATER
                    new(quantizedX[j], quantizedY[j]);
#else
                    new(quantizedX.GetElement(j), quantizedY.GetElement(j));
#endif
            }

            i += size;
        }

        for (; i < length; i++)
        {
            destination[i] = (new System.Numerics.Vector2(x[i], y[i]) * scaleFactorF) + offsetF;
        }
    }
#endif

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

#if NETCOREAPP3_0_OR_GREATER
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static Vector256<double> WidenToDouble(Vector128<int> input)
    {
        if (Avx.IsSupported)
        {
            return Avx.ConvertToVector256Double(input);
        }

        if (Sse2.IsSupported)
        {
            return Vector256.Create(
                Sse2.ConvertToVector128Double(input),
                Sse2.ConvertToVector128Double(Sse2.Shuffle(input, 0x32)));
        }

#if NET7_0_OR_GREATER
        return Vector256.Create((double)input[0], input[1], input[2], input[3]);
#else
        return Vector256.Create((double)input.GetElement(0), input.GetElement(1), input.GetElement(2), input.GetElement(3));
#endif
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static Vector256<float> WidenToSingle(Vector256<int> input)
    {
        if (Avx.IsSupported)
        {
            return Avx.ConvertToVector256Single(input);
        }

        if (Sse2.IsSupported)
        {
            return Vector256.Create(
                Sse2.ConvertToVector128Single(input.GetLower()),
                Sse2.ConvertToVector128Single(input.GetUpper()));
        }

#if NET7_0_OR_GREATER
        return Vector256.Create(
            Vector128.Create((float)input[0], input[1], input[2], input[3]),
            Vector128.Create((float)input[4], input[5], input[6], input[7]));
#else
        return Vector256.Create(
            Vector128.Create((float)input.GetElement(0), input.GetElement(1), input.GetElement(2), input.GetElement(3)),
            Vector128.Create((float)input.GetElement(4), input.GetElement(5), input.GetElement(6), input.GetElement(7)));
#endif
    }

#if !NET7_0_OR_GREATER
    private static Vector256<float> ScaleAndOffset(Vector256<int> input, Vector256<float> scale, Vector256<float> offset)
    {
#pragma warning disable IDE0046
        var widened = WidenToSingle(input);
        if (Fma.IsSupported)
        {
            return Fma.MultiplyAdd(widened, scale, offset);
        }

        if (Avx.IsSupported)
        {
            return Avx.Add(Avx.Multiply(widened, scale), offset);
        }

        if (Sse.IsSupported)
        {
            return Vector256.Create(
                Sse.Add(Sse.Multiply(widened.GetLower(), scale.GetLower()), offset.GetLower()),
                Sse.Add(Sse.Multiply(widened.GetUpper(), scale.GetUpper()), offset.GetUpper()));
        }

        return ((widened.AsVector() * scale.AsVector()) + offset.AsVector()).AsVector256();
#pragma warning restore IDE0046
    }
#endif
#endif
}