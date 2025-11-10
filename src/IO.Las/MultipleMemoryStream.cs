// -----------------------------------------------------------------------
// <copyright file="MultipleMemoryStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// A <see cref="MultipleStream"/> backed by <see cref="MemoryStream"/> instances.
/// </summary>
public class MultipleMemoryStream : MultipleStream
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleMemoryStream"/> class.
    /// </summary>
    public MultipleMemoryStream()
        : base(default(StringComparer))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleMemoryStream"/> class.
    /// </summary>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys, or <see langword="null"/> to use <see cref="StringComparer.Ordinal"/>.</param>
    public MultipleMemoryStream(IEqualityComparer<string>? comparer)
        : base(new Dictionary<string, Stream>(comparer ?? StringComparer.Ordinal))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleMemoryStream"/> class.
    /// </summary>
    /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> to use to back this instance.</param>
    public MultipleMemoryStream(IDictionary<string, Stream>? dictionary)
        : base(dictionary ?? new Dictionary<string, Stream>(StringComparer.Ordinal))
    {
    }

    /// <inheritdoc/>
    protected override Stream CreateStream(string name) => new MemoryStream();
}