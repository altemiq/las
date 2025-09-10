// -----------------------------------------------------------------------
// <copyright file="LasStreams.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The LAS <see cref="Stream"/> names.
/// </summary>
internal static class LasStreams
{
    /// <summary>
    /// The header name.
    /// </summary>
    public const string Header = "header";

    /// <summary>
    /// The variable length record name.
    /// </summary>
    public const string VariableLengthRecord = "vlr";

#if LAS1_3_OR_GREATER
    /// <summary>
    /// The extended variable length record name.
    /// </summary>
    public const string ExtendedVariableLengthRecord = "evlr";
#endif

    /// <summary>
    /// The point data name.
    /// </summary>
    public const string PointData = "point";

    /// <summary>
    /// The stream comparer.
    /// </summary>
    public static readonly StringComparer Comparer = new LasStreamComparer();

    /// <summary>
    /// Switches the stream to the specified name if the stream is a <see cref="MultipleStream"/>.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="name">The name of the stream to switch to.</param>
    /// <returns><see langword="true"/> if the stream was successfully switched; otherwise <see langword="false"/>.</returns>
    public static bool SwitchStreamIfMultiple(this Stream stream, string name) => stream is MultipleStream multipleStream && multipleStream.SwitchTo(name);

    /// <summary>
    /// Gets a value indicating whether the specified <see cref="Stream"/> can switch streams.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns><see langword="true"/> if <paramref name="stream"/> can switch streams; otherwise <see langword="false"/>.</returns>
    public static bool CanSwitchStream(this Stream stream) => stream is MultipleStream;

    private sealed class LasStreamComparer : StringComparer
    {
        private static readonly Dictionary<string, int> Mapping = new(Ordinal)
        {
            { Header, 0 },
            { VariableLengthRecord, 10 },
            { PointData, 20 },
#if LAS1_3_OR_GREATER
            { ExtendedVariableLengthRecord, 30 },
#endif
        };

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "This makes it harder to read.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement", Justification = "This makes it harder to read.")]
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

            if (!Mapping.TryGetValue(x, out var indexX))
            {
                return 1;
            }

            if (Mapping.TryGetValue(y, out var indexY))
            {
                return indexX.CompareTo(indexY);
            }

            return -1;
        }

        public override bool Equals(string? x, string? y) => Ordinal.Equals(x, y);

        public override int GetHashCode(string obj) => Ordinal.GetHashCode(obj);
    }
}