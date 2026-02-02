// -----------------------------------------------------------------------
// <copyright file="Color.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents an RGB (red, green, blue) color with each component being 16bit.
/// </summary>
[Serializable]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 3 * sizeof(ushort))]
public readonly partial struct Color : IEquatable<Color>
{
    /// <summary>
    /// An empty color structure.
    /// </summary>
    public static readonly Color Empty;

    private const int RGBRedShift = 32;
    private const int RGBGreenShift = 16;
    private const int RGBBlueShift = 0;

    /// <summary>
    /// Gets the red component value of this <see cref="Color" /> structure.
    /// </summary>
    [field: System.Runtime.InteropServices.FieldOffset(0)]
    public ushort R { get; init; }

    /// <summary>
    /// Gets the green component value of this <see cref="Color" /> structure.
    /// </summary>
    [field: System.Runtime.InteropServices.FieldOffset(2)]
    public ushort G { get; init; }

    /// <summary>
    /// Gets the blue component value of this <see cref="Color" /> structure.
    /// </summary>
    [field: System.Runtime.InteropServices.FieldOffset(4)]
    public ushort B { get; init; }

    /// <summary>
    /// Converts a <see cref="Color" /> to a <see cref="System.Drawing.Color" />.
    /// </summary>
    /// <param name="color">The color.</param>
    public static implicit operator System.Drawing.Color(Color color) => color.ToColor();

    /// <summary>
    /// Converts a <see cref="System.Drawing.Color" /> to a <see cref="Color" />.
    /// </summary>
    /// <param name="color">The color.</param>
    public static explicit operator Color(System.Drawing.Color color) => FromColor(color);

    /// <summary>Tests whether two specified <see cref="Color" /> structures are equivalent.</summary>
    /// <param name="left">The <see cref="Color" /> that is to the left of the equality operator. </param>
    /// <param name="right">The <see cref="Color" /> that is to the right of the equality operator. </param>
    /// <returns><see langword="true" /> if the two <see cref="Color" /> structures are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(Color left, Color right) => left.Equals(right);

    /// <summary>Tests whether two specified <see cref="Color" /> structures are different.</summary>
    /// <param name="left">The <see cref="Color" /> that is to the left of the inequality operator. </param>
    /// <param name="right">The <see cref="Color" /> that is to the right of the inequality operator. </param>
    /// <returns><see langword="true" /> if the two <see cref="Color" /> structures are different; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(Color left, Color right) => !(left == right);

    /// <summary>
    /// Creates a <see cref="Color" /> from its 48-bit component (red, green, and blue) values.
    /// </summary>
    /// <param name="red">The red component.</param>
    /// <param name="green">The green component.</param>
    /// <param name="blue">The blue component.</param>
    /// <returns>The color.</returns>
    public static Color FromRgb(int red, int green, int blue)
    {
        return new() { R = CheckUInt16(red), G = CheckUInt16(green), B = CheckUInt16(blue) };

        static ushort CheckUInt16(int value, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? name = default)
        {
            if (unchecked((uint)value) > ushort.MaxValue)
            {
                ThrowOutOfByteRange(value, name);
            }

            return (ushort)value;

#if NET8_0_OR_GREATER
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1863:Use 'CompositeFormat'", Justification = "This is a formatted string for an exception.")]
#endif
            [System.Diagnostics.CodeAnalysis.DoesNotReturn]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MA0183:string.Format should use a format string with placeholders", Justification = "False positive")]
            static void ThrowOutOfByteRange(int v, string? n)
            {
                throw new ArgumentException(string.Format(Properties.Resources.Culture, Properties.Resources.InvalidEx2BoundArgument, n, v, ushort.MinValue, ushort.MaxValue), n);
            }
        }
    }

    /// <summary>
    /// Creates a <see cref="Color" /> from its greyscale (intensity) value.
    /// </summary>
    /// <param name="intensity">The intensity component.</param>
    /// <returns>The color.</returns>
    public static Color FromIntensity(int intensity) => FromRgb(intensity, intensity, intensity);

    /// <summary>
    /// Converts a <see cref="System.Drawing.Color" /> to a <see cref="Color" />.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>An instance of <see cref="Color"/>.</returns>
    public static Color FromColor(System.Drawing.Color color) =>
        FromRgb(
            (color.R << 8) | color.R,
            (color.G << 8) | color.G,
            (color.B << 8) | color.B);

    /// <summary>
    /// Converts this instance to an RGB value.
    /// </summary>
    /// <returns>The RGB value.</returns>
    public long ToRgb() => GetValue(this.R, this.G, this.B);

    /// <summary>
    /// Converts this instance into a <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="alpha">The alpha to apply to the output.</param>
    /// <returns>An instance of <see cref="System.Drawing.Color"/>.</returns>
    public System.Drawing.Color ToColor(int alpha = byte.MaxValue) => System.Drawing.Color.FromArgb(alpha, this.R >> 8, this.G >> 8, this.B >> 8);

    /// <inheritdoc/>
    public bool Equals(Color other) => this.R == other.R && this.G == other.G && this.B == other.B;

    /// <inheritdoc/>
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is Color color ? this.Equals(color) : base.Equals(obj);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(this.R, this.G, this.B);

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Color)} [R={this.R}, G={this.G}, B={this.B}]";

    /// <summary>
    /// Gets the hue-saturation-lightness (HSL) lightness value for this <see cref="Color" /> structure.
    /// </summary>
    /// <returns>The lightness of this <see cref="Color" />. The lightness ranges from 0.0 through 1.0, where 0.0 represents black and 1.0 represents white.</returns>
    public float GetBrightness()
    {
        var (min, max) = MinMaxRgb(this.R, this.G, this.B);

        return (max + min) / (ushort.MaxValue * 2F);
    }

    /// <summary>
    /// Gets the hue-saturation-lightness (HSL) hue value, in degrees, for this <see cref="Color" /> structure.
    /// </summary>
    /// <returns>The hue, in degrees, of this <see cref="Color" />. The hue is measured in degrees, ranging from 0.0 through 360.0, in HSL color space.</returns>
    public float GetHue()
    {
        var (r, g, b) = (this.R, this.G, this.B);

        if (r == g && g == b)
        {
            return default;
        }

        var (min, max) = MinMaxRgb(r, g, b);

        float delta = max - min;
        var hue = (r, g) switch
        {
            var (red, _) when red == max => (g - b) / delta,
            var (_, green) when green == max => ((b - r) / delta) + 2F,
            _ => ((r - g) / delta) + 4F,
        };

        hue *= 60F;
        while (hue < 0F)
        {
            hue += 360F;
        }

        return hue;
    }

    /// <summary>
    /// Gets the hue-saturation-lightness (HSL) saturation value for this <see cref="Color" /> structure.
    /// </summary>
    /// <returns>The saturation of this <see cref="Color" />. The saturation ranges from 0.0 through 1.0, where 0.0 is grayscale and 1.0 is the most saturated.</returns>
    public float GetSaturation()
    {
        var (r, g, b) = (this.R, this.G, this.B);

        if (r == g && g == b)
        {
            return default;
        }

        var (min, max) = MinMaxRgb(r, g, b);

        var div = max + min;
        if (div > ushort.MaxValue)
        {
            div = (ushort.MaxValue * 2) - max - min;
        }

        return (max - min) / (float)div;
    }

    private static long GetValue(ushort red, ushort green, ushort blue) => (long)(((ulong)red << RGBRedShift) | ((ulong)green << RGBGreenShift) | ((ulong)blue << RGBBlueShift));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static (ushort Min, ushort Max) MinMaxRgb(ushort r, ushort g, ushort b)
    {
        var (min, max) = r > g ? (g, r) : (r, g);

        return b switch
        {
            var blue when blue > max => (min, blue),
            var blue when blue < min => (blue, max),
            _ => (min, max),
        };
    }
}