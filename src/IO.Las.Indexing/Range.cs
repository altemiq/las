// -----------------------------------------------------------------------
// <copyright file="Range.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// Represent a range has start and end indexes.
/// </summary>
/// <param name="start">The inclusive start index of the range.</param>
/// <param name="end">The exclusive end index of the range.</param>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly struct Range(Index start, Index end) : IEquatable<Range>
{
    /// <summary>
    /// Gets a <see cref="Range"/> object starting from first element to the end.
    /// </summary>
    public static Range All => new(Index.Start, Index.End);

    /// <summary>
    /// Gets the inclusive start index of the <see cref="Range"/>.
    /// </summary>
    public Index Start { get; } = start;

    /// <summary>
    /// Gets the exclusive end index of the <see cref="Range"/>.
    /// </summary>
    public Index End { get; } = end;

    /// <inheritdoc cref="Equals(Range)" />
    public static bool operator ==(Range left, Range right) => left.Equals(right);

    /// <inheritdoc cref="Equals(Range)" />
    public static bool operator !=(Range left, Range right) => !left.Equals(right);

    /// <summary>
    /// Returns a new <see cref="Range"/> instance starting from a specified start index to the end.
    /// </summary>
    /// <param name="start">The position of the first element from which the <see cref="Range"/> will be created.</param>
    /// <returns>A range from <paramref name="start"/> to the end.</returns>
    public static Range StartAt(Index start) => new(start, Index.End);

    /// <summary>
    /// Returns a new <see cref="Range"/> instance starting from the start to a specified end index.
    /// </summary>
    /// <param name="end">The position of the last element up to which the <see cref="Range"/> will be created.</param>
    /// <returns>A range that starts from the start to <paramref name="end"/>.</returns>
    public static Range EndAt(Index end) => new(Index.Start, end);

    /// <inheritdoc/>
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) =>
        obj is Range range &&
        range.Start.Equals(this.Start) &&
        range.End.Equals(this.End);

    /// <inheritdoc/>
    public bool Equals(Range other) => other.Start.Equals(this.Start) && other.End.Equals(this.End);

    /// <inheritdoc/>
    public override int GetHashCode()
#if NETSTANDARD2_0_OR_GREATER || NET46_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        => HashCode.Combine(this.Start.GetHashCode(), this.End.GetHashCode());
#else
    {
        var h1 = this.Start.GetHashCode();
        var rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
        return ((int)rol5 + h1) ^ this.End.GetHashCode();
    }
#endif

    /// <inheritdoc/>
    public override string ToString() => this.ToString(default);

#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
    /// <inheritdoc cref="IConvertible.ToString(IFormatProvider)" />
#if NET6_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3236:Caller information arguments should not be provided explicitly", Justification = "Checked")]
#endif
    public string ToString(IFormatProvider? provider)
    {
        // 2 for the dots, then for each index 1 for '^' and 10 for longest possible uint
        Span<char> span = stackalloc char[2 + (2 * 11)];
        var pos = 0;

        if (this.Start.IsFromEnd)
        {
            span[0] = '^';
            pos = 1;
        }

        var formatted = this.Start.Value.TryFormat(span[pos..], out int charsWritten, provider: provider);
        System.Diagnostics.Debug.Assert(formatted, $"Failed to format {nameof(this.Start)} in {nameof(Range)}.{nameof(this.ToString)}");
        pos += charsWritten;

        span[pos++] = '.';
        span[pos++] = '.';

        if (this.End.IsFromEnd)
        {
            span[pos++] = '^';
        }

        formatted = this.End.Value.TryFormat(span[pos..], out charsWritten, provider: provider);
        System.Diagnostics.Debug.Assert(formatted, $"Failed to format {nameof(this.End)} in {nameof(Range)}.{nameof(this.ToString)}");
        pos += charsWritten;

        return new(span[..pos]);
    }
#else
    /// <inheritdoc cref="IConvertible.ToString(IFormatProvider)" />
    public string ToString(IFormatProvider? provider) => this.Start.ToString(provider) + ".." + this.End.ToString(provider);
#endif

    /// <summary>
    /// Calculates the start offset and length of the range object using a length.
    /// </summary>
    /// <param name="length">A positive integer that represents the length of that the range will be used with.</param>
    /// <returns>The start offset and length of the range.</returns>
    /// <remarks>
    /// For performance reasons, this method doesn't validate <paramref name="length"/> to ensure that it is not negative. It does ensure that <paramref name="length"/> is within the current <see cref="Range"/> instance.
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public (uint Offset, uint Length) GetOffsetAndLength(uint length)
    {
        var startOffset = this.Start.GetOffset(length);
        var endOffset = this.End.GetOffset(length);

        return endOffset <= length && startOffset <= endOffset
            ? (startOffset, endOffset - startOffset)
            : throw new ArgumentOutOfRangeException(nameof(length));
    }
}