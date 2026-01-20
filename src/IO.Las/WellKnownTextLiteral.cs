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
public readonly struct WellKnownTextLiteral(string value)
{
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
}