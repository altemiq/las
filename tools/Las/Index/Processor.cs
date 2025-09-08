// -----------------------------------------------------------------------
// <copyright file="Processor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Index;

/// <summary>
/// The index processor.
/// </summary>
internal static class Processor
{
#if LAS1_4_OR_GREATER
    /// <summary>
    /// Processes ths index.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="inputs">The inputs.</param>
    /// <param name="tileSize">The tile size.</param>
    /// <param name="maximumIntervals">The maximum interval.</param>
    /// <param name="minimumPoints">The minimum points.</param>
    /// <param name="threshold">The threshold.</param>
    /// <param name="append">Set to <see langword="true"/> to append to the LAZ file.</param>
    public static void Process(IServiceProvider? services, Uri[] inputs, float tileSize = Indexing.LasIndex.DefaultTileSize, int maximumIntervals = Indexing.LasIndex.DefaultMaximumIntervals, uint minimumPoints = Indexing.LasIndex.DefaultMinimumPoints, int threshold = Indexing.LasIndex.DefaultThreshold, bool append = default)
#else
    /// <summary>
    /// Processes ths index.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="inputs">The inputs.</param>
    /// <param name="tileSize">The tile size.</param>
    /// <param name="maximumIntervals">The maximum interval.</param>
    /// <param name="minimumPoints">The minimum points.</param>
    /// <param name="threshold">The threshold.</param>
    public static void Process(IServiceProvider? services, Uri[] inputs, float tileSize = Indexing.LasIndex.DefaultTileSize, int maximumIntervals = Indexing.LasIndex.DefaultMaximumIntervals, uint minimumPoints = Indexing.LasIndex.DefaultMinimumPoints, int threshold = Indexing.LasIndex.DefaultThreshold)
#endif
    {
        foreach (var file in inputs)
        {
            Indexing.LasIndex index;
            using (var reader = LazReader.Create(File.OpenRead(file, services)))
            {
                index = Indexing.LasIndex.Create(reader, tileSize, maximumIntervals, minimumPoints, threshold);
            }

#if LAS1_4_OR_GREATER
            if (append)
            {
                // open the file for reading
                using var reader = LazReader.Create(File.Open(file, FileMode.Open, services));
                if (reader is LazReader lazReader)
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                using var stream = File.OpenWrite(Path.ChangeExtension(file.LocalPath, ".lax"));
                index.WriteTo(stream);
            }
#else
            using var stream = File.OpenWrite(Path.ChangeExtension(file, ".lax"), services);
            index.WriteTo(stream);
#endif
        }
    }
}