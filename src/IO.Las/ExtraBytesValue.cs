// -----------------------------------------------------------------------
// <copyright file="ExtraBytesValue.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="ExtraBytes"/> value.
/// </summary>
[System.Runtime.CompilerServices.Union]
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
    private readonly byte[]? bytes;
    private readonly ulong encoded;
    private readonly ExtraBytesDataType type;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified array of <see cref="byte"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(byte[] value)
    {
        this.bytes = value;
        this.type = ExtraBytesDataType.Undocumented;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="byte"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(byte value)
        : this(value, ExtraBytesDataType.UnsignedChar)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="sbyte"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(sbyte value)
        : this((ulong)value, ExtraBytesDataType.Char)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="ushort"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(ushort value)
        : this(value, ExtraBytesDataType.UnsignedShort)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="short"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(short value)
        : this((ulong)value, ExtraBytesDataType.Short)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="uint"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(uint value)
        : this(value, ExtraBytesDataType.UnsignedLong)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="int"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(int value)
        : this((ulong)value, ExtraBytesDataType.Long)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="ulong"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(ulong value)
        : this(value, ExtraBytesDataType.UnsignedLongLong)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(long value)
        : this((ulong)value, ExtraBytesDataType.LongLong)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="float"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(float value)
        : this(BitConverter.SingleToUInt32Bits(value), ExtraBytesDataType.Float)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="double"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ExtraBytesValue(double value)
        : this(BitConverter.DoubleToUInt64Bits(value), ExtraBytesDataType.Double)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesValue"/> struct from the specified <see cref="double"/> value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type.</param>
    internal ExtraBytesValue(ulong value, ExtraBytesDataType type) => (this.encoded, this.type) = (value, type);

    /// <inheritdoc />
    public object? Value =>
        this.type switch
        {
            ExtraBytesDataType.Undocumented => this.bytes,
            ExtraBytesDataType.UnsignedChar => (byte)this.encoded,
            ExtraBytesDataType.Char => (sbyte)this.encoded,
            ExtraBytesDataType.UnsignedShort => (ushort)this.encoded,
            ExtraBytesDataType.Short => (short)this.encoded,
            ExtraBytesDataType.UnsignedLong => (uint)this.encoded,
            ExtraBytesDataType.Long => (int)this.encoded,
            ExtraBytesDataType.UnsignedLongLong => this.encoded,
            ExtraBytesDataType.LongLong => (long)this.encoded,
            ExtraBytesDataType.Float => BitConverter.UInt32BitsToSingle((uint)this.encoded),
            ExtraBytesDataType.Double => BitConverter.UInt64BitsToDouble(this.encoded),
            _ => throw new System.Diagnostics.UnreachableException(),
        };

    /// <summary>
    /// Gets a value indicating whether this instance has a value.
    /// </summary>
    public bool HasValue => this.type is not ExtraBytesDataType.Undocumented || this.bytes is not null;

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
            // ExtraBytesDataType.Undocumented
            byte[] b => SetAndReturnTrue(new(b), out union),

            // ExtraBytesDataType.UnsignedChar
            byte b => SetAndReturnTrue(new(b), out union),

            // ExtraBytesDataType.Char
            sbyte b => SetAndReturnTrue(new(b), out union),

            // ExtraBytesDataType.UnsignedShort
            ushort b => SetAndReturnTrue(new(b), out union),

            // ExtraBytesDataType.Short
            short b => SetAndReturnTrue(new(b), out union),

            // ExtraBytesDataType.UnsignedLong
            uint b => SetAndReturnTrue(new(b), out union),

            // ExtraBytesDataType.Long
            int b => SetAndReturnTrue(new(b), out union),

            // ExtraBytesDataType.UnsignedLongLong
            ulong b => SetAndReturnTrue(new(b), out union),

            // ExtraBytesDataType.LongLong
            long b => SetAndReturnTrue(new(b), out union),

            // ExtraBytesDataType.Float
            float b => SetAndReturnTrue(new(b), out union),

            // ExtraBytesDataType.Double
            double b => SetAndReturnTrue(new(b), out union),
            _ => SetDefaultAndReturnFalse(out union),
        };

        static bool SetAndReturnTrue(ExtraBytesValue v, out ExtraBytesValue union)
        {
            union = v;
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
        if (this.type is ExtraBytesDataType.Undocumented)
        {
            value = this.bytes!;
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
        if (this.type is ExtraBytesDataType.UnsignedChar)
        {
            value = (byte)this.encoded;
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
        if (this.type is ExtraBytesDataType.Char)
        {
            value = (sbyte)this.encoded;
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
        if (this.type is ExtraBytesDataType.UnsignedShort)
        {
            value = (ushort)this.encoded;
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
        if (this.type is ExtraBytesDataType.Short)
        {
            value = (short)this.encoded;
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
        if (this.type is ExtraBytesDataType.UnsignedLong)
        {
            value = (uint)this.encoded;
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
        if (this.type is ExtraBytesDataType.Long)
        {
            value = (int)this.encoded;
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
        if (this.type is ExtraBytesDataType.UnsignedLongLong)
        {
            value = this.encoded;
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
        if (this.type is ExtraBytesDataType.LongLong)
        {
            value = (long)this.encoded;
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
        if (this.type is ExtraBytesDataType.Float)
        {
            value = BitConverter.UInt32BitsToSingle((uint)this.encoded);
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
        if (this.type is ExtraBytesDataType.Double)
        {
            value = BitConverter.UInt64BitsToDouble(this.encoded);
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

#pragma warning disable IDE0072
    /// <inheritdoc />
    public override int GetHashCode() => this.type switch
    {
        ExtraBytesDataType.Undocumented => this.bytes!.GetHashCode(),
        _ => this.encoded.GetHashCode(),
    };
#pragma warning restore IDE0072

    /// <inheritdoc />
    public override string? ToString() => this.Value?.ToString();
}