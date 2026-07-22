// -----------------------------------------------------------------------
// <copyright file="CloudExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Cloud;

#pragma warning disable RCS1263, SA1101

/// <summary>
/// The <see cref="Cloud"/> extensions.
/// </summary>
public static class CloudExtensions
{
    /// <summary>
    /// The <see cref="ILasReader"/> extensions
    /// </summary>
    /// <param name="reader">The reader.</param>
    extension(ILasReader reader)
    {
        /// <summary>
        /// Copies the contents the current reader to the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void CopyTo(ILasWriter writer) =>
            reader.CopyTo(writer, static vlr => !vlr.IsForCompression() && !vlr.IsForCloudOptimization(), static evlr => !evlr.IsForCompression() && !evlr.IsForCloudOptimization());

        /// <summary>
        /// Copies the contents the current reader to the specified writer asynchronously.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cancellationToken">The token for cancelling the task.</param>
        /// <returns>The asynchronous task for copying the contents to <paramref name="writer"/>.</returns>
        public Task CopyToAsync(ILasWriter writer, CancellationToken cancellationToken = default) =>
            reader.CopyToAsync(writer, static vlr => !vlr.IsForCompression() && !vlr.IsForCloudOptimization(), static evlr => !evlr.IsForCompression() && !evlr.IsForCloudOptimization(), cancellationToken);
    }
}