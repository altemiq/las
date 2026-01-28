// -----------------------------------------------------------------------
// <copyright file="WellKnownTextLiteral.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="WellKnownTextNode"/> literal value.
/// </summary>
/// <param name="value">The literal value.</param>
public readonly struct WellKnownTextLiteral(string value) : IEquatable<WellKnownTextLiteral>, IEquatable<string>
{
    /// <summary>
    /// Implements the equals operator.
    /// </summary>
    /// <param name="left">The left hand side.</param>
    /// <param name="right">The right hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(WellKnownTextLiteral left, WellKnownTextLiteral right) => left.Equals(right);

    /// <summary>
    /// Implements the not-equals operator.
    /// </summary>
    /// <param name="left">The left hand side.</param>
    /// <param name="right">The right hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(WellKnownTextLiteral left, WellKnownTextLiteral right) => left.Equals(right);

    /// <summary>
    /// Creates a <see cref="WellKnownTextLiteral"/> from the <see cref="Enum"/> value.
    /// </summary>
    /// <typeparam name="T">The type of enum.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <returns>The <see cref="WellKnownTextLiteral"/> from <paramref name="value"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MA0011:IFormatProvider is missing", Justification = "The IFormatProvider methods are obsolete.")]
    public static WellKnownTextLiteral FromEnum<T>(T value)
        where T : Enum => new(value.ToString());

    /// <inheritdoc/>
    public override string ToString() => value;

    /// <summary>
    /// Gets the byte count.
    /// </summary>
    /// <returns>The byte count.</returns>
    public int GetByteCount() => System.Text.Encoding.UTF8.GetByteCount(value);

    /// <inheritdoc />
    public bool Equals(WellKnownTextLiteral other) => other.Equals(value);

    /// <inheritdoc />
    public bool Equals(string? other) => StringComparer.Ordinal.Equals(value, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj switch
    {
        WellKnownTextLiteral other => this.Equals(other),
        string other => this.Equals(other),
        _ => false,
    };

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(value);
}