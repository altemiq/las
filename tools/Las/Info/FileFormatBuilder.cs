// -----------------------------------------------------------------------
// <copyright file="FileFormatBuilder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The file <see cref="IFormatBuilder"/>.
/// </summary>
/// <param name="writer">The writer.</param>
public class FileFormatBuilder(TextWriter writer) : IFormatBuilder
{
    /// <inheritdoc />
    public IFormatBuilder AppendFormat(Style? style, string format, params object?[] args)
    {
        writer.Write(format, args);
        return this;
    }

    /// <inheritdoc />
    public IFormatBuilder Append(string value, Style? style = default)
    {
        writer.Write(value);
        return this;
    }

    /// <inheritdoc />
    public IFormatBuilder AppendLine(string? value = default, Style? style = default)
    {
        if (value is null)
        {
            writer.WriteLine();
        }
        else
        {
            writer.WriteLine(value);
        }

        return this;
    }
}