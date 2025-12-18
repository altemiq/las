// -----------------------------------------------------------------------
// <copyright file="Processor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.To.Laz;

/// <summary>
/// The las2laz processor.
/// </summary>
internal static class Processor
{
    /// <summary>
    /// Processes the input to output.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="input">The input URI.</param>
    /// <param name="output">The output file.</param>
    public static void Process(IServiceProvider? serviceProvider, Uri input, FileInfo output)
    {
        using var reader = LazReader.Create(File.OpenRead(input, serviceProvider));
        using var writer = new LazWriter(output.OpenWrite());
#if LAS1_4_OR_GREATER
        Cloud.CloudExtensions.CopyTo(reader, writer);
#else
        reader.CopyTo(writer);
#endif
    }
}