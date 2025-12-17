// -----------------------------------------------------------------------
// <copyright file="IndexExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_0_OR_GREATER
namespace Altemiq.IO.Las.Indexing;

#pragma warning disable SA1101

/// <summary>
/// The <see langword="Index"/> extensions.
/// </summary>
public static class IndexExtensions
{
    /// <summary>
    /// The dictionary extensions.
    /// </summary>
    extension<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        /// <summary>
        /// Attempts to add the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. It can be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the key/value pair was added to the dictionary successfully; otherwise, <see langword="false"/>.</returns>
        public bool TryAdd(TKey key, TValue value)
        {
            try
            {
                dictionary.Add(key, value);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
#endif