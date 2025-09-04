// -----------------------------------------------------------------------
// <copyright file="Options.Indexing.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// Indexing options.
/// </content>
internal static partial class Options
{
    /// <summary>
    /// Indexing options.
    /// </summary>
    public static class Indexing
    {
        /// <summary>
        /// The tile size option.
        /// </summary>
        public static readonly Option<float> TileSize = new("--tile-size") { Description = "The smallest spatial area that can be indexed in [n]x[n] units", DefaultValueFactory = static _ => default };

        /// <summary>
        /// The maximum intervals option.
        /// </summary>
        public static readonly Option<int> MaximumIntervals = new("--maximum") { Description = "The maximum number of intervals per spatial area", DefaultValueFactory = static _ => -20 };

        /// <summary>
        /// The minimum points option.
        /// </summary>
        public static readonly Option<uint> MinimumPoints = new("--minimum") { Description = "The minimum number of points in the input to index", DefaultValueFactory = static _ => 100000 };

        /// <summary>
        /// The threshold option.
        /// </summary>
        public static readonly Option<int> Threshold = new("--threshold") { Description = "The threshold.", DefaultValueFactory = static _ => 1000 };

        /// <summary>
        /// The append option.
        /// </summary>
        public static readonly Option<bool> Append = new("--append") { Description = "Whether to append the index to the LA(S/Z) file. For LAZ files, this is recorded in the Compressed TAG. For LAS files, this is stored as an EVLR", DefaultValueFactory = static _ => default };
    }
}