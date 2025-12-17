// -----------------------------------------------------------------------
// <copyright file="CompressionExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Compression extension methods.
/// </summary>
public static class CompressionExtensions
{
    /// <summary>
    /// Gets a value indicating whether the specified variable length record is for compression.
    /// </summary>
    /// <param name="record">The variable length record.</param>
    /// <returns><see langword="true"/> is <paramref name="record"/> is for compression; otherwise <see langword="false"/>.</returns>
    public static bool IsForCompression(this VariableLengthRecord record) => record is CompressedTag;

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets a value indicating whether the specified extended variable length record is for compression.
    /// </summary>
    /// <param name="record">The extended variable length record.</param>
    /// <returns><see langword="true"/> is <paramref name="record"/> is for compression; otherwise <see langword="false"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1175:Unused 'this' parameter.", Justification = "This is required for extension methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This is required for extension methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1175:Unused \'this\' parameter", Justification = "This is required for extension methods")]
    public static bool IsForCompression(this ExtendedVariableLengthRecord record) => false;
#endif

    /// <summary>
    /// Registers compression VLRs.
    /// </summary>
    /// <param name="processor">The VLR processor.</param>
    public static void RegisterCompression(this VariableLengthRecordProcessor processor) => processor.Register(CompressedTag.TagRecordId, ProcessCompressedTag);

    /// <summary>
    /// Registers compression VLRs.
    /// </summary>
    /// <param name="processor">The VLR processor.</param>
    /// <returns><see langword="true" /> when the compression processors are successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processors, in which case nothing gets added.</returns>
    public static bool TryRegisterCompression(this VariableLengthRecordProcessor processor) => processor.TryRegister(CompressedTag.TagRecordId, ProcessCompressedTag);

    /// <summary>
    /// Gets the extra bytes count.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>The extra bytes count.</returns>
    internal static ushort GetExtraBytesCount(this IEnumerable<LasItem> items) => (ushort)items
#if LAS1_4_OR_GREATER
        .Where(static item => item.IsType(LasItemType.Byte) || item.IsType(LasItemType.Byte14))
#else
        .Where(static item => item.IsType(LasItemType.Byte))
#endif
        .Sum(static item => item.Size);

    /// <summary>
    /// Gets the point version.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <param name="defaultVersion">The default version.</param>
    /// <returns>The point version.</returns>
    internal static ushort GetPointVersion(this IEnumerable<LasItem> items, ushort defaultVersion = 2) => items
#if LAS1_4_OR_GREATER
        .Where(item => item.IsType(LasItemType.Point10) || item.IsType(LasItemType.Point14))
#else
        .Where(item => item.IsType(LasItemType.Point10))
#endif
        .Select(item => item.Version)
        .FirstOrDefault(defaultVersion);

#if !NET6_0_OR_GREATER
    /// <summary>
    /// Returns the first element of a sequence, or a specified default value if the sequence contains no elements.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to return the first element of.</param>
    /// <param name="defaultValue">The default value to return if the sequence is empty.</param>
    /// <returns><paramref name="defaultValue"/> if <paramref name="source"/> is empty; otherwise, the first element in <paramref name="source"/>.</returns>
    internal static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var enumerator = source.GetEnumerator();
        return enumerator.MoveNext() ? enumerator.Current : defaultValue;
    }
#endif

    private static CompressedTag ProcessCompressedTag(VariableLengthRecordHeader header, ReadOnlySpan<byte> data) => new(header, data);
}