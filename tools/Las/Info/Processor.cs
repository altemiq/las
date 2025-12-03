// -----------------------------------------------------------------------
// <copyright file="Processor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The information processor.
/// </summary>
internal static class Processor
{
    /// <summary>
    /// Processes the specified file.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="console">The console.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <param name="noMinMax">No min/max calculate.</param>
    /// <param name="noReturns">No returns.</param>
    /// <param name="boundingBox">The bounding box.</param>
    public static void Process(Stream stream, IAnsiConsole console, IFormatProvider formatProvider, bool noMinMax, bool noReturns, BoundingBox? boundingBox)
    {
        using var reader = LazReader.Create(stream);
        var formatter = new DefaultLasReaderFormatter(new ConsoleFormatBuilder(console, new LasFormatProvider(formatProvider, reader)));
        formatter.Format(reader, noMinMax, noReturns, boundingBox);
    }
}