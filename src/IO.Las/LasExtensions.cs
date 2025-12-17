// -----------------------------------------------------------------------
// <copyright file="LasExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_1_OR_GREATER
namespace Altemiq.IO.Las;

#pragma warning disable MA0040, S2325, SA1101

/// <summary>
/// The <see langword="Las"/> extensions.
/// </summary>
internal static class LasExtensions
{
    extension(Guid guid)
    {
        /// <summary>
        /// Tries to write the current GUID instance into a span of bytes.
        /// </summary>
        /// <param name="destination">When this method returns, the GUID as a span of bytes.</param>
        /// <returns><see langword="true"/> if the GUID is successfully written to the specified span; <see langword="false"/> otherwise.</returns>
        public bool TryWriteBytes(Span<byte> destination)
        {
            var byteArray = guid.ToByteArray();
            if (byteArray.Length > destination.Length)
            {
                return false;
            }

            byteArray.CopyTo(destination);
            return true;
        }
    }

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