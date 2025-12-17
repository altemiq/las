// -----------------------------------------------------------------------
// <copyright file="HttpExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if !NET5_0_OR_GREATER
namespace Altemiq.IO.Las.Http;

#pragma warning disable MA0040, S2325, SA1101

/// <summary>
/// The <see cref="Http"/> extensions.
/// </summary>
internal static class HttpExtensions
{
    extension(HttpContent content)
    {
        /// <summary>
        /// Serialize the HTTP content and return a stream that represents the content as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return content.ReadAsStreamAsync();
        }
    }
}
#endif