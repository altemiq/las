// -----------------------------------------------------------------------
// <copyright file="Processor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.To.Copc;

/// <summary>
/// The las2copc processor.
/// </summary>
internal static class Processor
{
    /// <summary>
    /// Processes the input to output.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="input">The input URI.</param>
    /// <param name="output">The output file.</param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <param name="maxPointsPerOctant">The maximum number of points per octant.</param>
    /// <param name="occupancyResolution">The occupancy resolution.</param>
    public static void Process(
        IServiceProvider? serviceProvider,
        Uri input,
        FileInfo output,
        int maxDepth = -1,
        ulong maxPointsPerOctant = 100000UL,
        float occupancyResolution = 50F)
    {
        using var reader = LazReader.Create(File.OpenRead(input, serviceProvider));

        reader.CopyToCloudOptimized(
            output.OpenWrite(),
            maximumDepth: maxDepth,
            maximumPointsPerOctant: maxPointsPerOctant,
            occupancyResolution: occupancyResolution,
            swap: false,
            shuffle: false);
    }

    /// <summary>
    /// Processes the input to output.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="output">The output file.</param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <param name="maxPointsPerOctant">The maximum number of points per octant.</param>
    /// <param name="occupancyResolution">The occupancy resolution.</param>
    public static void Process(
        Stream stream,
        FileInfo output,
        int maxDepth = -1,
        ulong maxPointsPerOctant = 100000UL,
        float occupancyResolution = 50F)
    {
        using var reader = LazReader.Create(stream);

        reader.CopyToCloudOptimized(
            output.OpenWrite(),
            maximumDepth: maxDepth,
            maximumPointsPerOctant: maxPointsPerOctant,
            occupancyResolution: occupancyResolution,
            swap: false,
            shuffle: false);
    }

    /// <summary>
    /// Writes the input reader to the output stream.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="output">The output.</param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <param name="minPointsPerOctant">The minimum number of points per octant.</param>
    /// <param name="maxPointsPerOctant">The maximum number of points per octant.</param>
    /// <param name="occupancyResolution">The occupancy resolution.</param>
    /// <param name="rootGridSize">The root grid size.</param>
    /// <param name="probabilitySwapEvent">The probability swap event.</param>
    /// <param name="unordered">Set to <see langword="true"/> to leave points unordered.</param>
    /// <param name="bufferSize">The buffer size.</param>
    /// <param name="swap">Set to <see langword="true"/> to swap the points.</param>
    /// <param name="shuffle">Set to <see langword="true"/> to shuffle the points.</param>
    /// <param name="sort">Set to <see langword="true"/> to sort the points.</param>
    public static void Process(
        LasReader reader,
        Stream output,
        int maxDepth = -1,
        int minPointsPerOctant = 100,
        ulong maxPointsPerOctant = 100000UL,
        float occupancyResolution = 50F,
        int rootGridSize = 256,
        float probabilitySwapEvent = 0.95F,
        bool unordered = false,
        int bufferSize = 1000000,
        bool swap = true,
        bool shuffle = true,
        bool sort = true) =>
        reader.CopyToCloudOptimized(
            output,
            maxDepth,
            minPointsPerOctant,
            maxPointsPerOctant,
            occupancyResolution,
            rootGridSize,
            probabilitySwapEvent,
            unordered,
            bufferSize,
            swap,
            shuffle,
            sort);
}