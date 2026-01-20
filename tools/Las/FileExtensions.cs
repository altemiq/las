// -----------------------------------------------------------------------
// <copyright file="FileExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="File"/> extension methods.
/// </summary>
internal static class FileExtensions
{
    extension(File)
    {
        /// <summary>
        /// Determines whether the specified uri exists.
        /// </summary>
        /// <param name="uri">The uri to check.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri) => File.Exists(uri, Amazon.S3.AmazonS3Client.Create(), HttpClient.Create());

        /// <summary>
        /// Determines whether the specified uri exists.
        /// </summary>
        /// <param name="uri">The uri to check.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri, IServiceProvider? serviceProvider) => File.Exists(uri, Amazon.S3.AmazonS3Client.Create(serviceProvider), HttpClient.Create(serviceProvider));

        /// <summary>
        /// Determines whether the specified uri exists.
        /// </summary>
        /// <param name="uri">The uri to check.</param>
        /// <param name="lazyS3Client">The <see cref="Amazon.S3"/> client.</param>
        /// <param name="lazyHttpClient">The <see cref="System.Net.Http"/> client.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri, Lazy<Amazon.S3.IAmazonS3> lazyS3Client, Lazy<HttpClient> lazyHttpClient) => uri switch
        {
            { Scheme: "file" } => File.Exists(uri.LocalPath),
            _ when S3.S3UriUtility.IsS3(uri) => S3.S3Las.Exists(uri, lazyS3Client),
            { Scheme: "http" or "https" } => Http.HttpLas.Exists(uri, lazyHttpClient),
            _ => false,
        };

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenRead(Uri uri) => File.OpenRead(uri, Amazon.S3.AmazonS3Client.Create(), HttpClient.Create());

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenRead(Uri uri, IServiceProvider? serviceProvider) => File.OpenRead(uri, Amazon.S3.AmazonS3Client.Create(serviceProvider), HttpClient.Create(serviceProvider));

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="lazyS3Client">The <see cref="Amazon.S3"/> client.</param>
        /// <param name="lazyHttpClient">The <see cref="System.Net.Http"/> client.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenRead(Uri uri, Lazy<Amazon.S3.IAmazonS3> lazyS3Client, Lazy<HttpClient> lazyHttpClient)
            => uri switch
            {
                { Scheme: "file" } => new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, ushort.MaxValue),
                _ when S3.S3UriUtility.IsS3(uri) => S3.S3Las.OpenRead(uri, lazyS3Client),
                { Scheme: "http" or "https" } => Http.HttpLas.OpenRead(uri, lazyHttpClient),
                _ => throw new NotSupportedException(),
            };

        /// <summary>
        /// Opens an existing URI for writing.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenWrite(Uri uri) => File.OpenWrite(uri, Amazon.S3.AmazonS3Client.Create(), HttpClient.Create());

        /// <summary>
        /// Opens an existing URI for writing.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenWrite(Uri uri, IServiceProvider? serviceProvider) => File.OpenWrite(uri, Amazon.S3.AmazonS3Client.Create(serviceProvider), HttpClient.Create(serviceProvider));

        /// <summary>
        /// Opens an existing URI for writing.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="lazyS3Client">The <see cref="Amazon.S3"/> client.</param>
        /// <param name="lazyHttpClient">The <see cref="System.Net.Http"/> client.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "This will be used in the future.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This will be used in the future.")]
        public static Stream OpenWrite(Uri uri, Lazy<Amazon.S3.IAmazonS3> lazyS3Client, Lazy<HttpClient> lazyHttpClient)
        {
            return uri switch
            {
                { Scheme: "file" } => FileOpenWrite(uri.LocalPath),
                _ => throw new NotSupportedException(),
            };

            static Stream FileOpenWrite(string path)
            {
                File.CreateDirectoryIfPossible(path);
                return File.Open(path, FileMode.Create);
            }
        }

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a uri is created if one does not exist, and determines whether the contents of existing uris are retained or overwritten.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream Open(Uri uri, FileMode mode) => File.Open(uri, mode, Amazon.S3.AmazonS3Client.Create(), HttpClient.Create());

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a uri is created if one does not exist, and determines whether the contents of existing uris are retained or overwritten.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream Open(Uri uri, FileMode mode, IServiceProvider? serviceProvider) => File.Open(uri, mode, Amazon.S3.AmazonS3Client.Create(serviceProvider), HttpClient.Create(serviceProvider));

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a uri is created if one does not exist, and determines whether the contents of existing uris are retained or overwritten.</param>
        /// <param name="lazyAmazonS3">The <see cref="Amazon.S3"/> client.</param>
        /// <param name="lazyHttpClient">The <see cref="System.Net.Http"/> client.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream Open(Uri uri, FileMode mode, Lazy<Amazon.S3.IAmazonS3> lazyAmazonS3, Lazy<HttpClient> lazyHttpClient) => (uri, mode) switch
        {
            ({ Scheme: "file" }, _) => File.Open(uri.LocalPath, mode),
            (_, FileMode.Open) when S3.S3UriUtility.IsS3(uri) => S3.S3Las.OpenRead(uri, lazyAmazonS3),
            ({ Scheme: "http" or "https" }, FileMode.Open) => Http.HttpLas.OpenRead(uri, lazyHttpClient),
            _ => throw new NotSupportedException(),
        };

        private static void CreateDirectoryIfPossible(string path)
        {
            if (File.Exists(path))
            {
                return;
            }

            if (Path.GetDirectoryName(path) is { } directoryName)
            {
                Directory.CreateDirectory(directoryName);
            }
        }
    }
}