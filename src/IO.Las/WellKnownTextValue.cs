// -----------------------------------------------------------------------
// <copyright file="WellKnownTextValue.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The well-known text value.
/// </summary>
public readonly struct WellKnownTextValue
{
    private readonly object? value;

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="node">The node.</param>
    public WellKnownTextValue(WellKnownTextNode node) => this.value = node;

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public WellKnownTextValue(double value) => this.value = value;

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public WellKnownTextValue(string value) => this.value = value;

    /// <summary>
    /// Initialize a new instance of the <see cref="WellKnownTextValue"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public WellKnownTextValue(WellKnownTextLiteral value) => this.value = value;

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

    /// <inheritdoc />
    public override string? ToString() =>
        this.value switch
        {
            string s => $"\"{s}\"",
            double d => d.ToString("G15", System.Globalization.CultureInfo.InvariantCulture),
            not null => this.value.ToString(),
            _ => null,
        };

    /// <summary>
    /// Copies the contents of this instance into a destination <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
    /// <returns>The number of bytes written.</returns>
    public int CopyTo(Span<byte> destination)
    {
        return this.value switch
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
        return this.value switch
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