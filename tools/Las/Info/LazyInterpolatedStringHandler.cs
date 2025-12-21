// -----------------------------------------------------------------------
// <copyright file="LazyInterpolatedStringHandler.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The lazy interpolated string handler.
/// </summary>
/// <param name="literalLength">The literal length.</param>
/// <param name="formattedCount">The formatted count.</param>
[System.Runtime.CompilerServices.InterpolatedStringHandler]
public struct LazyInterpolatedStringHandler(int literalLength, int formattedCount)
{
    private readonly SortedList<int, string?> literals = [];
    private readonly SortedList<int, (IFormattable, string?)> formattable = new(formattedCount);
    private int current = 0;

    /// <summary>
    /// Appends the literal.
    /// </summary>
    /// <param name="s">The literal string.</param>
    public void AppendLiteral(string s) => this.literals.Add(this.current++, s);

    /// <summary>
    /// Appends the value.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="value">The value.</param>
    public void AppendFormatted<T>(T value)
    {
        if (value is IFormattable f)
        {
            this.AppendFormatted(f, default);
        }
        else
        {
            this.literals.Add(this.current++, value?.ToString());
        }
    }

    /// <summary>
    /// Appends the value and format.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    public void AppendFormatted<T>(T value, string? format)
        where T : IFormattable => this.formattable.Add(this.current++, (value, format));

    /// <summary>
    /// Gets the formatted string.
    /// </summary>
    /// <param name="provider">The format provider.</param>
    /// <returns>The formatted string.</returns>
    public readonly string ToString(IFormatProvider? provider)
    {
        var builder = new System.Text.StringBuilder(literalLength);

        for (int i = 0; i < this.current; i++)
        {
            if (this.literals.TryGetValue(i, out var literal))
            {
                builder.Append(literal);
            }
            else if (this.formattable.TryGetValue(i, out var value))
            {
                builder.Append(value.Item1.ToString(value.Item2, provider));
            }
        }

        return builder.ToString();
    }
}