// -----------------------------------------------------------------------
// <copyright file="PathExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The <see cref="Path"/> extension methods.
/// </summary>
internal static class PathExtensions
{
    extension(Path)
    {
        /// <summary>
        /// Determines whether the specified uri exists.
        /// </summary>
        /// <param name="uri">The uri to check.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri) => Exists(uri, static () => new());

        /// <summary>
        /// Determines whether the specified uri exists.
        /// </summary>
        /// <param name="uri">The uri to check.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri, IServiceProvider? serviceProvider) => Exists(uri, CreateHttpClient(serviceProvider));

        /// <summary>
        /// Determines whether the specified uri exists.
        /// </summary>
        /// <param name="uri">The uri to check.</param>
        /// <param name="httpClientFactory">The <see cref="System.Net.Http"/> client factory.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri, Func<HttpClient> httpClientFactory) => uri switch
        {
            { Scheme: "file" } => Path.Exists(uri.LocalPath),
            _ when S3.S3UriUtility.IsS3(uri) => S3.S3Las.Exists(uri),
            { Scheme: "http" or "https" } => Http.HttpLas.Exists(uri, httpClientFactory),
            _ => false,
        };

        /// <inheritdoc cref="Path.GetExtension(string)"/>
        public static string GetExtension(Uri uri)
        {
            var builder = new UriBuilder(uri);
            return Path.GetExtension(builder.Path);
        }

        /// <inheritdoc cref="Path.ChangeExtension"/>
        public static Uri ChangeExtension(Uri uri, string extension)
        {
            var builder = new UriBuilder(uri);
            builder.Path = Path.ChangeExtension(builder.Path, extension);
            return builder.Uri;
        }

#pragma warning disable S1144
        private static Func<HttpClient> CreateHttpClient(IServiceProvider? serviceProvider)
        {
            if (serviceProvider is not null)
            {
                return serviceProvider.GetRequiredService<HttpClient>;
            }

            return CreateHttpClient;
        }

        private static HttpClient CreateHttpClient() => new();
#pragma warning restore S1144
    }
}