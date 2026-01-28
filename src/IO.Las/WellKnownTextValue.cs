// -----------------------------------------------------------------------
// <copyright file="WellKnownTextValue.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The well-known text value.
/// </summary>
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
    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="node">The node.</param>
    public WellKnownTextValue(WellKnownTextNode node) => this.Value = node;

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public WellKnownTextValue(double value) => this.Value = value;

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public WellKnownTextValue(string value) => this.Value = value;

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public WellKnownTextValue(WellKnownTextLiteral value) => this.Value = value;

    /// <inheritdoc />
    public object? Value { get; }

    /// <summary>
    /// Gets a value indicating whether this instance has a value.
    /// </summary>
    public bool HasValue => this.Value != null;

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
            WellKnownTextNode node => (true, new WellKnownTextValue(node)),
            double d => (true, d),
            string s => (true, s),
            WellKnownTextLiteral l => (true, l),
            _ => (false, default),
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
        if (this.Value is WellKnownTextNode node)
        {
            value = node;
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
        if (this.Value is double d)
        {
            value = d;
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
    public bool TryGetValue([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out string value)
    {
        if (this.Value is string s)
        {
            value = s;
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
    public bool TryGetValue(out WellKnownTextLiteral value)
    {
        if (this.Value is WellKnownTextLiteral literal)
        {
            value = literal;
            return true;
        }

        value = default;
        return false;
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
    public override bool Equals(object? obj) => this.Value is { } value ? value.Equals(obj) : obj is null;

    /// <inheritdoc />
    public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;

    /// <inheritdoc />
    public override string? ToString() =>
        this.Value switch
        {
            string s => $"\"{s}\"",
            double d => d.ToString("G15", System.Globalization.CultureInfo.InvariantCulture),
            not null => this.Value.ToString(),
            _ => null,
        };

    /// <summary>
    /// Copies the contents of this instance into a destination <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
    /// <returns>The number of bytes written.</returns>
    public int CopyTo(Span<byte> destination)
    {
        return this.Value switch
        {
            string s => WriteString(destination, s),
            double d => WriteDouble(destination, d),
            WellKnownTextLiteral literal => System.Text.Encoding.UTF8.GetBytes(literal.ToString(), destination),
            WellKnownTextNode node => node.CopyTo(destination),
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
            System.Buffers.Text.Utf8Formatter.TryFormat(d, buffer, out var bytesWritten, System.Buffers.StandardFormat.Parse("G13"));
            return bytesWritten;
        }
    }

    /// <summary>
    /// Gets the byte count.
    /// </summary>
    /// <returns>The byte count.</returns>
    public int GetByteCount()
    {
        return this.Value switch
        {
            string s => System.Text.Encoding.UTF8.GetByteCount(s) + 2,
            double d => GetDoubleByteCount(d),
            WellKnownTextLiteral literal => literal.GetByteCount(),
            WellKnownTextNode node => node.GetByteCount(),
            _ => default,
        };

        static int GetDoubleByteCount(double d)
        {
            Span<byte> buffer = stackalloc byte[20];
            System.Buffers.Text.Utf8Formatter.TryFormat(d, buffer, out var bytesWritten, System.Buffers.StandardFormat.Parse("G13"));
            return bytesWritten;
        }
    }
}