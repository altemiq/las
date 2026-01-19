// -----------------------------------------------------------------------
// <copyright file="AmazonS3ClientExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The <see cref="HttpClient"/> extensions.
/// </summary>
internal static class AmazonS3ClientExtensions
{
    extension(Amazon.S3.AmazonS3Client)
    {
        /// <summary>
        /// Creates a <see cref="Amazon.S3.IAmazonS3"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The client.</returns>
        public static Lazy<Amazon.S3.IAmazonS3> Create(IServiceProvider? serviceProvider = default) => serviceProvider is not null
            ? new(serviceProvider.GetRequiredService<Amazon.S3.IAmazonS3>)
            : new(static () => new Amazon.S3.AmazonS3Client());
    }
}