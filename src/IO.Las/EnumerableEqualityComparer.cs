// -----------------------------------------------------------------------
// <copyright file="EnumerableEqualityComparer.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="IEnumerable{T}"/> comparer class.
/// </summary>
internal static class EnumerableEqualityComparer
{
    /// <summary>
    /// Gets the instance for the type of item.
    /// </summary>
    /// <typeparam name="T">The type of item in the enumerable.</typeparam>
    /// <returns>The equality comparer.</returns>
    public static IEqualityComparer<IEnumerable<T>> Instance<T>() => Instances<T>.Value;

    private static class Instances<T>
    {
        internal static readonly IEqualityComparer<IEnumerable<T>> Value = new Comparer<T>();
    }

    private sealed class Comparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        /// <inheritdoc/>
        bool IEqualityComparer<IEnumerable<T>>.Equals(IEnumerable<T>? x, IEnumerable<T>? y) =>
            x is null
                ? y is null
                : y is not null && x.SequenceEqual(y);

        /// <inheritdoc/>
        int IEqualityComparer<IEnumerable<T>>.GetHashCode(IEnumerable<T> obj)
        {
            System.HashCode hashCode = default;
            foreach (var data in obj)
            {
                hashCode.Add(data);
            }

            return hashCode.ToHashCode();
        }
    }
}