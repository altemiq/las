// -----------------------------------------------------------------------
// <copyright file="ConsoleFormatBuilder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The <see cref="IAnsiConsole"/> format builder.
/// </summary>
/// <param name="console">The console.</param>
/// <param name="formatProvider">The format provider.</param>
public class ConsoleFormatBuilder(IAnsiConsole console, IFormatProvider formatProvider) : IFormatBuilder
{
    /// <inheritdoc />
    public IFormatBuilder AppendFormat(Style? style, string format, object? arg0)
    {
        console.Write(string.Format(formatProvider, format, arg0), style);
        return this;
    }

    /// <inheritdoc />
    public IFormatBuilder AppendFormat(Style? style, string format, object? arg0, object? arg1, object? arg2)
    {
        console.Write(string.Format(formatProvider, format, arg0, arg1, arg2), style);
        return this;
    }

    /// <inheritdoc />
    public IFormatBuilder AppendFormat(Style? style, string format, params object?[] args)
    {
        console.Write(string.Format(formatProvider, format, args), style);
        return this;
    }

    /// <inheritdoc />
    public IFormatBuilder Append(string value, Style? style = default)
    {
        console.Write(value, style);
        return this;
    }

    /// <inheritdoc />
    public IFormatBuilder AppendLine(string? value = default, Style? style = default)
    {
        if (value is null)
        {
            console.WriteLine();
        }
        else
        {
            console.WriteLine(value, style);
        }

        return this;
    }
}