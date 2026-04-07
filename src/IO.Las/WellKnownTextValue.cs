// -----------------------------------------------------------------------
// <copyright file="WellKnownTextValue.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The well-known text value.
/// </summary>
[System.Runtime.CompilerServices.Union]
public readonly struct WellKnownTextValue :
#if NET7_0_OR_GREATER
    System.Runtime.CompilerServices.IUnion<WellKnownTextValue>,
#else
     System.Runtime.CompilerServices.IUnion,
#endif
    IEquatable<WellKnownTextNode>,
    IEquatable<double>,
    IEquatable<string>,
    IEquatable<WellKnownTextLiteral>
{
    private readonly WellKnownTextNode nodeValue;
    private readonly double doubleValue;
    private readonly string? stringValue;
    private readonly WellKnownTextLiteral literalValue;
    private readonly int tag;

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="node">The node.</param>
    public WellKnownTextValue(WellKnownTextNode node)
    {
        this.nodeValue = node;
        this.tag = 1;
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public WellKnownTextValue(double value)
    {
        this.doubleValue = value;
        this.tag = 2;
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public WellKnownTextValue(string value)
    {
        this.stringValue = value;
        this.tag = this.stringValue is not null ? 3 : default;
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public WellKnownTextValue(WellKnownTextLiteral value)
    {
        this.literalValue = value;
        this.tag = 4;
    }

    /// <inheritdoc />
    public object? Value => this.tag switch
    {
        1 => this.nodeValue,
        2 => this.doubleValue,
        3 => this.stringValue!,
        4 => this.literalValue,
        _ => null,
    };

    /// <summary>
    /// Gets a value indicating whether this instance has a value.
    /// </summary>
    public bool HasValue => this.tag is not 0;

    /// <summary>
    /// Converts the <see cref="WellKnownTextNode"/> to a <see cref="WellKnownTextValue"/>.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The <see cref="WellKnownTextValue"/>.</returns>
    public static implicit operator WellKnownTextValue(WellKnownTextNode node) => new(node);

    /// <summary>
    /// Converts the <see cref="double"/> to a <see cref="WellKnownTextValue"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="WellKnownTextValue"/>.</returns>
    public static implicit operator WellKnownTextValue(double value) => new(value);

    /// <summary>
    /// Converts the <see cref="string"/> to a <see cref="WellKnownTextValue"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="WellKnownTextValue"/>.</returns>
    public static implicit operator WellKnownTextValue(string value) => new(value);

    /// <summary>
    /// Converts the <see cref="WellKnownTextLiteral"/> to a <see cref="WellKnownTextValue"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="WellKnownTextValue"/>.</returns>
    public static implicit operator WellKnownTextValue(WellKnownTextLiteral value) => new(value);

    /// <summary>
    /// Implements the equals operator.
    /// </summary>
    /// <param name="left">The left hand side.</param>
    /// <param name="right">The right hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(WellKnownTextValue left, WellKnownTextValue right) => left.Equals(right);

    /// <summary>
    /// Implements the not-equals operator.
    /// </summary>
    /// <param name="left">The left hand side.</param>
    /// <param name="right">The right hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(WellKnownTextValue left, WellKnownTextValue right) => left.Equals(right);

#if NET6_0_OR_GREATER
    /// <inheritdoc />
    public static bool TryCreate(object? value, out WellKnownTextValue union)
    {
        (var returnValue, union) = value switch
        {
            WellKnownTextNode node => (true, node),
            double d => (true, d),
            string s => (true, s),
            WellKnownTextLiteral l => (true, l),
            _ => (false, default(WellKnownTextValue)),
        };

        return returnValue;
    }
#endif

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out WellKnownTextNode value)
    {
        value = this.nodeValue;
        return this.tag is 1;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out double value)
    {
        value = this.doubleValue;
        return this.tag is 2;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out string value)
    {
        value = this.stringValue;
        return this.tag is 3;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>A value indicating whether <paramref name="value"/> was successfully obtained.</returns>
    public bool TryGetValue(out WellKnownTextLiteral value)
    {
        value = this.literalValue;
        return this.tag is 4;
    }

    /// <inheritdoc />
    public bool Equals(WellKnownTextNode other) => this.TryGetValue(out WellKnownTextNode value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(double other) => this.TryGetValue(out double value) && value.Equals(other);

    /// <inheritdoc />
    public bool Equals(string? other) => this.TryGetValue(out string? value) && StringComparer.Ordinal.Equals(value, other);

    /// <inheritdoc />
    public bool Equals(WellKnownTextLiteral other) => this.TryGetValue(out WellKnownTextLiteral value) && value.Equals(other);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return (obj, this.tag) switch
        {
            (WellKnownTextValue var, var t) when var.tag == t => EqualsWellKnownTextValue(this, var),
            (WellKnownTextNode node, 1) => this.nodeValue.Equals(node),
            (double d, 2) => this.doubleValue.Equals(d),
            (string s, 3) => StringComparer.Ordinal.Equals(this.stringValue, s),
            (WellKnownTextLiteral literal, 4) => this.literalValue.Equals(literal),
            _ => false,
        };

        static bool EqualsWellKnownTextValue(WellKnownTextValue first, WellKnownTextValue second)
        {
            return first.tag switch
            {
                1 => first.Equals(second.nodeValue),
                2 => first.Equals(second.doubleValue),
                3 => StringComparer.Ordinal.Equals(first.stringValue, second.stringValue),
                4 => first.literalValue.Equals(second.literalValue),
                _ => true,
            };
        }
    }

    /// <inheritdoc />
    public override int GetHashCode() => this.tag switch
    {
        1 => this.nodeValue.GetHashCode(),
        2 => this.doubleValue.GetHashCode(),
        3 => StringComparer.Ordinal.GetHashCode(this.stringValue!),
        4 => this.literalValue.GetHashCode(),
        _ => 0,
    };

    /// <inheritdoc />
    public override string? ToString() => this.AppendTo(new()).ToString();

    /// <summary>
    /// Copies the contents of this instance into a destination <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
    /// <returns>The number of bytes written.</returns>
    public int CopyTo(Span<byte> destination)
    {
        return this.tag switch
        {
            1 => this.nodeValue.CopyTo(destination),
            2 => WriteDouble(destination, this.doubleValue),
            3 => WriteString(destination, this.stringValue!),
            4 => System.Text.Encoding.UTF8.GetBytes(this.literalValue.ToString(), destination),
            _ => default,
        };

        static int WriteString(Span<byte> buffer, string s)
        {
            var bytesWritten = 0;
            buffer[bytesWritten] = (byte)'\"';
            bytesWritten++;
            bytesWritten += System.Text.Encoding.UTF8.GetBytes(s, buffer[bytesWritten..]);
            buffer[bytesWritten] = (byte)'\"';
            return bytesWritten + 1;
        }

        static int WriteDouble(Span<byte> buffer, double d)
        {
            System.Buffers.Text.Utf8Formatter.TryFormat(d, buffer, out var bytesWritten, System.Buffers.StandardFormat.Parse("G15"));
            return bytesWritten;
        }
    }

    /// <summary>
    /// Gets the byte count.
    /// </summary>
    /// <returns>The byte count.</returns>
    public int GetByteCount()
    {
        return this.tag switch
        {
            1 => this.nodeValue.GetByteCount(),
            2 => GetDoubleByteCount(this.doubleValue),
            3 => System.Text.Encoding.UTF8.GetByteCount(this.stringValue!) + 2,
            4 => this.literalValue.GetByteCount(),
            _ => default,
        };

        static int GetDoubleByteCount(double d)
        {
            Span<byte> buffer = stackalloc byte[20];
            System.Buffers.Text.Utf8Formatter.TryFormat(d, buffer, out var bytesWritten, System.Buffers.StandardFormat.Parse("G15"));
            return bytesWritten;
        }
    }

    /// <summary>
    /// Appends a copy of this instance to the specified string builder.
    /// </summary>
    /// <param name="builder">The string builder.</param>
    /// <returns>A reference to the string builder after the append operation has completed.</returns>
    internal System.Text.StringBuilder AppendTo(System.Text.StringBuilder builder) =>
        this.tag switch
        {
            1 => this.nodeValue.AppendTo(builder),
#if NET6_0_OR_GREATER
            2 => builder.Append(System.Globalization.CultureInfo.InvariantCulture, $"{this.doubleValue:G15}"),
#else
             2 => builder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0:G15}", this.doubleValue),
#endif
            3 => builder.Append('\"').Append(this.stringValue).Append('\"'),
            4 => builder.Append(this.literalValue.ToString()),
            _ => builder,
        };
}