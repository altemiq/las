// -----------------------------------------------------------------------
// <copyright file="StringBuilderLasReaderFormatter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The <see cref="System.Text.StringBuilder"/> <see cref="ILasReaderFormatter"/>.
/// </summary>
internal class StringBuilderLasReaderFormatter : DefaultLasReaderFormatter
{
    private readonly StringBuilderFormatBuilder formatBuilder;

    /// <summary>
    /// Initialises a new instance of the <see cref="StringBuilderLasReaderFormatter"/> class.
    /// </summary>
    /// <param name="formatProvider">The format provider.</param>
    public StringBuilderLasReaderFormatter(IFormatProvider? formatProvider)
        : this(new StringBuilderFormatBuilder(formatProvider))
    {
    }

    private StringBuilderLasReaderFormatter(StringBuilderFormatBuilder formatBuilder)
        : base(formatBuilder) => this.formatBuilder = formatBuilder;

    /// <inheritdoc />
    public override string ToString() => this.formatBuilder.ToString();

    private sealed class StringBuilderFormatBuilder(System.Text.StringBuilder stringBuilder, IFormatProvider? formatProvider) : IFormatBuilder
    {
        public StringBuilderFormatBuilder(IFormatProvider? formatProvider)
            : this(new(), formatProvider)
        {
        }

        public IFormatBuilder AppendFormat(Style? style, string format, params object?[] args)
        {
            stringBuilder.AppendFormat(formatProvider, format, args);
            return this;
        }

        public IFormatBuilder Append(string value, Style? style = default)
        {
             stringBuilder.Append(value);
             return this;
        }

        public IFormatBuilder AppendLine(string? value = default, Style? style = default)
        {
            stringBuilder.AppendLine(value);
            return this;
        }

        /// <inheritdoc />
        public override string ToString() => stringBuilder.ToString();
    }
}