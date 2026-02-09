// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable RCS1263, SA1101

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The <see cref="IServiceCollection"/> extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <content>
    /// The <see cref="IServiceCollection"/> extensions.
    /// </content>
    /// <param name="serviceCollection">The service collection.</param>
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Adds the file provider.
        /// </summary>
        /// <returns>The service collection to chain.</returns>
        public IServiceCollection AddFile() =>
            serviceCollection
                .AddSingleton<IStreamProvider, Providers.FileStreamProvider>();

        /// <summary>
        /// Adds the AWS provider.
        /// </summary>
        /// <returns>The service collection to chain.</returns>
        public IServiceCollection AddAws() =>
            serviceCollection
                .AddAWSService<Amazon.S3.IAmazonS3>()
                .AddSingleton<IStreamProvider, Providers.S3StreamProvider>();

        /// <summary>
        /// Adds the Azure provider.
        /// </summary>
        /// <returns>The service collection to chain.</returns>
        public IServiceCollection AddAzure() =>
            serviceCollection
                .AddSingleton<IStreamProvider, Providers.BlobStreamProvider>();

        /// <summary>
        /// Adds the HTTP provider.
        /// </summary>
        /// <returns>The service collection to chain.</returns>
        public IServiceCollection AddHttp() =>
            serviceCollection
                .AddHttpClient()
                .AddSingleton<IStreamProvider, Providers.HttpStreamProvider>();
    }
}