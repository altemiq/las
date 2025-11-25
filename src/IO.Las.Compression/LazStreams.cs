// -----------------------------------------------------------------------
// <copyright file="LazStreams.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The LAZ <see cref="Stream"/> names.
/// </summary>
internal static class LazStreams
{
    /// <summary>
    /// The chunk name.
    /// </summary>
    public const string Chunk = "chunk";

    /// <summary>
    /// The chunk table header name.
    /// </summary>
    public const string ChunkTablePosition = "chunk-table-header";

    /// <summary>
    /// The chunk table name.
    /// </summary>
    public const string ChunkTable = "chunk-table";

#if LAS1_3_OR_GREATER
    /// <summary>
    /// The special extended variable length record name.
    /// </summary>
    public const string SpecialExtendedVariableLengthRecord = "special-" + LasStreams.ExtendedVariableLengthRecord;
#endif

    /// <summary>
    /// The LAZ stream comparer.
    /// </summary>
    public static readonly StringComparer Comparer = new LazStreamComparer();

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, string> ChunkNameCache = [];

    /// <summary>
    /// Formats a chunk index.
    /// </summary>
    /// <param name="index">The chunk index.</param>
    /// <returns>The formatted chunk index.</returns>
    public static string FormatChunk(int index) => ChunkNameCache.GetOrAdd(index, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{Chunk}_{index}"));

    /// <summary>
    /// Converts the string representation of a chunk to its 32-bit signed integer equivalent.
    /// </summary>
    /// <param name="s">The string containing the chunk to convert.</param>
    /// <returns>A 32-bit signed integer equivalent to the chunk contained in <paramref name="s"/>.</returns>
    /// <exception cref="InvalidOperationException">The input is not a chunk number.</exception>
    public static int ParseChunkNumber(string s) => s.Split('_') switch
    {
        [Chunk, var v] => int.Parse(v, System.Globalization.CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException(),
    };

    /// <summary>
    /// Converts the string representation of a chunk to its 32-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A string containing a number to convert.</param>
    /// <param name="result">When this method returns, contains the 32-bit signed integer value equivalent of the chunk contained in <paramref name="s"/>, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s"/> parameter is <see langword="null"/> or <see cref="string.Empty"/>, is not of the correct format, or represents a number less than <see cref="int.MinValue"/> or greater than <see cref="int.MaxValue"/>. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseChunkNumber(string s, out int result)
    {
        const string Prefix = $"{Chunk}_";
        if (s.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return int.TryParse(s[Prefix.Length..], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out result);
        }

        result = -1;
        return false;
    }

    private sealed class LazStreamComparer : StringComparer
    {
        private static readonly Dictionary<string, int> Mapping = new(Ordinal)
        {
            { LasStreams.Header, 0 },
            { LasStreams.VariableLengthRecord, 10 },
            { LasStreams.PointData, 20 },
            { ChunkTablePosition, 30 },
            { Chunk, 100 },
            { ChunkTable, 10000 },
#if LAS1_3_OR_GREATER
            { LasStreams.ExtendedVariableLengthRecord, 10010 },
            { SpecialExtendedVariableLengthRecord, 10020 },
#endif
        };

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "This makes it harder to read")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement", Justification = "This makes it harder to read")]
        public override int Compare(string? x, string? y)
        {
            if (Ordinal.Compare(x, y) is 0)
            {
                // these are the same.
                return 0;
            }

            if (x is null)
            {
                return 1;
            }

            if (y is null)
            {
                return -1;
            }

            if (!TryGetValue(x, out var indexX))
            {
                return 1;
            }

            if (!TryGetValue(y, out var indexY))
            {
                return -1;
            }

            return indexX.CompareTo(indexY);

            static bool TryGetValue(string s, out int result)
            {
                if (Mapping.TryGetValue(s, out result))
                {
                    return true;
                }

                if (TryParseChunkNumber(s, out var chunkNumber))
                {
                    result = Mapping[Chunk] + chunkNumber;
                    return true;
                }

                result = -1;
                return false;
            }
        }

        public override bool Equals(string? x, string? y) => Ordinal.Equals(x, y);

        public override int GetHashCode(string obj) => Ordinal.GetHashCode(obj);
    }
}