// -----------------------------------------------------------------------
// <copyright file="CloudExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Cloud;

/// <summary>
/// The <see cref="Cloud"/> extensions.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "False positive")]
public static class CloudExtensions
{
    /// <summary>
    /// The <see cref="ILasReader"/> extensions
    /// </summary>
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