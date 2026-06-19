// -----------------------------------------------------------------------
// <copyright file="PolyfillExtensions.Guid.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable SA1101

/// <content>
/// Extension methods for <see cref="Guid"/>.
/// </content>
internal static partial class PolyfillExtensions
{
    /// <summary>
    /// The <see cref="Guid"/> extensions.
    /// </summary>
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
}