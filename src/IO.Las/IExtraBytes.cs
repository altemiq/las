// -----------------------------------------------------------------------
// <copyright file="IExtraBytes.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The Extra Bytes VLR provides a mechanism whereby additional information can be added to the end of a standard Point Record.
/// </summary>
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ExtraBytes), nameof(ExtraBytes.Create))]
public interface IExtraBytes : IReadOnlyList<ExtraBytesItem>
{
    /// <summary>
    /// Gets the value for the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    ExtraBytesValue GetValue(int index, ReadOnlySpan<byte> source);

    /// <summary>
    /// Gets the values.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    IReadOnlyList<ExtraBytesValue> GetValues(ReadOnlySpan<byte> source);

    /// <summary>
    /// Gets the value for the specified index asynchronously.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    ValueTask<ExtraBytesValue> GetValueAsync(int index, ReadOnlyMemory<byte> source);

    /// <summary>
    /// Gets the values asynchronously.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    ValueTask<IReadOnlyList<ExtraBytesValue>> GetValuesAsync(ReadOnlyMemory<byte> source);
}