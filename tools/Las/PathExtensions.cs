// -----------------------------------------------------------------------
// <copyright file="PathExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

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
        public static bool Exists(Uri uri) => Path.Exists(uri, Amazon.S3.AmazonS3Client.Create(), HttpClient.Create());

        /// <summary>
        /// Determines whether the specified uri exists.
        /// </summary>
        /// <param name="uri">The uri to check.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri, IServiceProvider? serviceProvider) => Path.Exists(uri, Amazon.S3.AmazonS3Client.Create(serviceProvider), HttpClient.Create(serviceProvider));

        /// <summary>
        /// Determines whether the specified uri exists.
        /// </summary>
        /// <param name="uri">The uri to check.</param>
        /// <param name="lazyAmazonS3">The <see cref="Amazon.S3"/> client.</param>
        /// <param name="lazyHttpClient">The <see cref="System.Net.Http"/> client.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri, Lazy<Amazon.S3.IAmazonS3> lazyAmazonS3, Lazy<HttpClient> lazyHttpClient) => uri switch
        {
            { Scheme: "file" } => Path.Exists(uri.LocalPath),
            _ when Amazon.S3.Util.AmazonS3Uri.IsAmazonS3Endpoint(uri) => S3.S3Las.Exists(uri, lazyAmazonS3),
            { Scheme: "http" or "https" } => Http.HttpLas.Exists(uri, lazyHttpClient),
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
    }
}