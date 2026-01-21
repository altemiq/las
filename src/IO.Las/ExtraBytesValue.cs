// -----------------------------------------------------------------------
// <copyright file="ExtraBytesValue.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="ExtraBytes"/> value.
/// </summary>
public readonly struct ExtraBytesValue :
#if NET6_0_OR_GREATER
    System.Runtime.CompilerServices.IUnion<ExtraBytesValue>,
#else
    System.Runtime.CompilerServices.IUnion,
#endif
    IEquatable<byte[]>,
    IEquatable<byte>,
    IEquatable<sbyte>,
    IEquatable<ushort>,
    IEquatable<short>,
    IEquatable<uint>,
    IEquatable<int>,
    IEquatable<ulong>,
    IEquatable<long>,
    IEquatable<float>,
    IEquatable<double>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified array of <see cref="byte"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(byte[] value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="byte"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(byte value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="sbyte"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(sbyte value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="ushort"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(ushort value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="short"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(short value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="uint"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(uint value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="int"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(int value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="ulong"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(ulong value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(long value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="float"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(float value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="double"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(double value) => this.Value = value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="object"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    internal ExtraBytesValue(object? value) => this.Value = value;

    /// <inheritdoc />
    public object? Value { get; }

    /// <summary>
    /// Gets a value indicating whether this instance has a value.
    /// </summary>
    public bool HasValue => this.Value != null;

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified array of <see cref="byte"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(byte[] value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified <see cref="byte"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(byte value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified <see cref="sbyte"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(sbyte value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified <see cref="ushort"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(ushort value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified <see cref="short"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(short value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified <see cref="uint"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(uint value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified <see cref="int"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(int value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified <see cref="ulong"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(ulong value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(long value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified <see cref="float"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(float value) => new(value);

    /// <summary>
    /// Creates a new <see cref="ExtraBytesItem"/> from the specified <see cref="double"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="ExtraBytesItem"/> around <paramref name="value"/>.</returns>
    public static implicit operator ExtraBytesValue(double value) => new(value);

    /// <summary>
    /// Implements the equals operator.
    /// </summary>
    /// <param name="left">The left hand side.</param>
    /// <param name="right">The right hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(ExtraBytesValue left, ExtraBytesValue right) => left.Equals(right);

    /// <summary>
    /// Implements the not-equals operator.
    /// </summary>
    /// <param name="left">The left hand side.</param>
    /// <param name="right">The right hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(ExtraBytesValue left, ExtraBytesValue right) => left.Equals(right);

#if NET6_0_OR_GREATER
    /// <inheritdoc />
    public static bool TryCreate(object? value, out ExtraBytesValue union)
    {
        return value switch
        {
            byte[] // ExtraBytesDataType.Undocumented
                or byte // ExtraBytesDataType.UnsignedChar
                or sbyte // ExtraBytesDataType.Char
                or ushort // ExtraBytesDataType.UnsignedShort
                or short // ExtraBytesDataType.Short
                or uint // ExtraBytesDataType.UnsignedLong
                or int // ExtraBytesDataType.Long
                or ulong // ExtraBytesDataType.UnsignedLongLong
                or long // ExtraBytesDataType.LongLong
                or float // ExtraBytesDataType.Float
                or double // ExtraBytesDataType.Double
                => SetAndReturnTrue(value, out union),
            _ => SetDefaultAndReturnFalse(out union),
        };

        static bool SetAndReturnTrue(object? value, out ExtraBytesValue union)
        {
            union = new(value);
            return true;
        }

        static bool SetDefaultAndReturnFalse(out ExtraBytesValue union)
        {
            union = default;
            return false;
        }
    }
#endif

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out byte[] value)
    {
        if (this.Value is byte[] bytes)
        {
            value = bytes;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out byte value)
    {
        if (this.Value is byte @byte)
        {
            value = @byte;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out sbyte value)
    {
        if (this.Value is sbyte @sbyte)
        {
            value = @sbyte;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out ushort value)
    {
        if (this.Value is ushort @ushort)
        {
            value = @ushort;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out short value)
    {
        if (this.Value is short @short)
        {
            value = @short;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out uint value)
    {
        if (this.Value is uint @uint)
        {
            value = @uint;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out int value)
    {
        if (this.Value is int @int)
        {
            value = @int;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out ulong value)
    {
        if (this.Value is ulong @ulong)
        {
            value = @ulong;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out long value)
    {
        if (this.Value is long @long)
        {
            value = @long;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out float value)
    {
        if (this.Value is float @float)
        {
            value = @float;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out double value)
    {
        if (this.Value is double @double)
        {
            value = @double;
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(byte[]? other) => this.TryGetValue(out byte[]? value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(byte other) => this.TryGetValue(out byte value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(sbyte other) => this.TryGetValue(out sbyte value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(ushort other) => this.TryGetValue(out ushort value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(short other) => this.TryGetValue(out short value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(uint other) => this.TryGetValue(out uint value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(int other) => this.TryGetValue(out int value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(ulong other) => this.TryGetValue(out ulong value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(long other) => this.TryGetValue(out long value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(float other) => this.TryGetValue(out float value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(double other) => this.TryGetValue(out double value) && value.Equals(other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => this.Value is { } value ? value.Equals(obj) : obj is null;

    /// <inheritdoc />
    public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;

    /// <inheritdoc />
    public override string? ToString() => this.Value?.ToString();
}