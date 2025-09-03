// -----------------------------------------------------------------------
// <copyright file="LazyFormattable.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// A lazy <see cref="IFormattable"/>.
/// </summary>
internal sealed class LazyFormattable : IFormattable
{
    private readonly Func<IFormatProvider?, string> function;

    private LazyFormattable(Func<IFormatProvider?, string> format) => this.function = format;

    /// <summary>
    /// Creates a lazy formattable around the <see cref="FormattableString"/>.
    /// </summary>
    /// <param name="formattableString">The formattable string.</param>
    /// <returns>The lazy formattable.</returns>
    public static IFormattable Create(FormattableString formattableString) => new LazyFormattable(formattableString.ToString);

    /// <summary>
    /// Creates a lazy formattable around the format and arguments.
    /// </summary>
    /// <inheritdoc cref="string.Format(string, object[])"/>
    /// <returns>The lazy formattable.</returns>
    public static IFormattable Create(string format, params object[] args) => new LazyFormattable(formatProvider => string.Format(formatProvider, format, args));

    /// <summary>
    /// Creates a lazy formattable around the value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The lazy formattable.</returns>
    public static IFormattable Create(string value) => new LazyFormattable(_ => value);

    /// <summary>
    /// Creates a lazy formattable around the enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="values">The values.</param>
    /// <param name="format">The format to apply to each element.</param>
    /// <returns>The lazy formattable.</returns>
    public static IFormattable Create<T>(IEnumerable<T> values, Func<IFormatProvider?, T, string> format) => new LazyFormattable(formatProvider => string.Join(' ', values.Select(value => format(formatProvider, value))));

    /// <summary>
    /// Creates a lazy formattable around the function.
    /// </summary>
    /// <param name="format">The format function.</param>
    /// <returns>The lazy formattable.</returns>
    public static IFormattable Create(Func<IFormatProvider?, string> format) => new LazyFormattable(format);

    /// <summary>
    /// Creates a lazy formattable around the enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="values">The values.</param>
    /// <returns>The lazy formattable.</returns>
    public static IFormattable Create<T>(IEnumerable<T> values)
        where T : IConvertible => Create(values, (formatProvider, value) => value.ToString(formatProvider));

    /// <inheritdoc/>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => this.function(formatProvider);
}