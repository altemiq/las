// -----------------------------------------------------------------------
// <copyright file="Index.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// Represent a type can be used to index a collection either from the start or the end.
/// </summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly struct Index : IEquatable<Index>
{
    private readonly long value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Index"/> struct with a specified index position and a value that indicates if the index is from the beginning or the end of a collection.
    /// </summary>
    /// <param name="value">The index value. It has to be zero or positive number.</param>
    /// <param name="fromEnd">Indicating if the index is from the start or from the end.</param>
    /// <remarks>If the <see cref="Index" /> constructed from the end, an index value of 1 points to the last element, and an index value of 0 points beyond the last element.</remarks>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Index(uint value, bool fromEnd = false)
    {
        var longValue = (long)value;
        this.value = fromEnd
            ? ~longValue
            : longValue;
    }

    private Index(long value) => this.value = value;

    /// <summary>Gets an <see cref="Index"/> pointing at first element.</summary>
    public static Index Start => new(0L);

    /// <summary>
    /// Gets an <see cref="Index"/> pointing at beyond last element.
    /// </summary>
    public static Index End => new(~0L);

    /// <summary>
    /// Gets the index value.
    /// </summary>
    public uint Value => this.value < 0 ? (uint)~this.value : (uint)this.value;

    /// <summary>
    /// Gets a value indicating whether the index is from the start or the end.
    /// </summary>
    public bool IsFromEnd => this.value < 0;

    /// <summary>
    /// Converts integer number to an <see cref="Index"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Index(uint value) => FromStart(value);

    /// <inheritdoc cref="Equals(Index)" />
    public static bool operator ==(Index left, Index right) => left.Equals(right);

    /// <inheritdoc cref="Equals(Index)" />
    public static bool operator !=(Index left, Index right) => !left.Equals(right);

    /// <summary>
    /// Create an <see cref="Index"/> from the start at a specified index position.
    /// </summary>
    /// <param name="value">The index value from the start.</param>
    /// <returns>The index value.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Index FromStart(uint value) => new((long)value);

    /// <summary>
    /// Create an <see cref="Index"/> from the end at a specified index position.
    /// </summary>
    /// <param name="value">The index value from the end.</param>
    /// <returns>The index value.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Index FromEnd(uint value) => new(~(long)value);

    /// <summary>
    /// Calculates the offset from the start of the collection using the specified collection length.
    /// </summary>
    /// <param name="length">The length of the collection that the Index will be used with. Must be a positive value.</param>
    /// <remarks>
    /// <para>For performance reasons, this method does not validate if <paramref name="length"/> or the returned value are negative. It also doesn't validate if the returned value is greater than <paramref name="length"/>.</para>
    /// <para>Collections are not expected to have a negative length/count. If this method's returned offset is negative and is then used to index a collection, the runtime will throw <see cref="ArgumentOutOfRangeException"/>, which will have the same effect as validation.</para>
    /// </remarks>
    /// <returns>The offset.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public uint GetOffset(uint length)
    {
        var offset = this.value;
        return this.IsFromEnd ? (uint)(offset + length + 1) : (uint)offset;
    }

    /// <inheritdoc/>
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is Index index && this.Equals(index);

    /// <inheritdoc/>
    public bool Equals(Index other) => this.value == other.value;

    /// <inheritdoc/>
    public override int GetHashCode() => this.value.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => this.ToString(default);

    /// <inheritdoc cref="System.IConvertible.ToString(System.IFormatProvider)" />
    public string ToString(IFormatProvider? provider)
    {
        return this.IsFromEnd ? ToStringFromEnd(this.Value, provider) : this.Value.ToString(provider);

#if NET6_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3236:Caller information arguments should not be provided explicitly", Justification = "Checked")]
#endif
        static string ToStringFromEnd(uint value, IFormatProvider? formatProvider)
        {
#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
            Span<char> span = stackalloc char[11]; // 1 for ^ and 10 for longest possible uint value
            var formatted = value.TryFormat(span[1..], out var charsWritten, provider: formatProvider);
            System.Diagnostics.Debug.Assert(formatted, $"Failed formatting in {nameof(Index)}.{nameof(ToStringFromEnd)}");
            span[0] = '^';
            return new(span[..(charsWritten + 1)]);
#else
            return '^' + value.ToString(formatProvider);
#endif
        }
    }
}