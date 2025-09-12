// -----------------------------------------------------------------------
// <copyright file="FileExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using Microsoft.Extensions.DependencyInjection;

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
            { Scheme: "file" } => File.Exists(uri.LocalPath),
            _ when S3.S3UriUtility.IsS3(uri) => S3.S3Las.Exists(uri),
            { Scheme: "http" or "https" } => Http.HttpLas.Exists(uri, httpClientFactory),
            _ => false,
        };

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenRead(Uri uri) => OpenRead(uri, static () => new());

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenRead(Uri uri, IServiceProvider? serviceProvider) => OpenRead(uri, CreateHttpClient(serviceProvider));

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="httpClientFactory">The <see cref="System.Net.Http"/> client factory.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenRead(Uri uri, Func<HttpClient> httpClientFactory)
            => uri switch
            {
                { Scheme: "file" } => new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, ushort.MaxValue),
                _ when S3.S3UriUtility.IsS3(uri) => S3.S3Las.OpenRead(uri),
                { Scheme: "http" or "https" } => Http.HttpLas.OpenRead(uri, httpClientFactory),
                _ => throw new NotSupportedException(),
            };

        /// <summary>
        /// Opens an existing URI for writing.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenWrite(Uri uri) => OpenWrite(uri, CreateHttpClient);

        /// <summary>
        /// Opens an existing URI for writing.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenWrite(Uri uri, IServiceProvider? serviceProvider) => OpenWrite(uri, CreateHttpClient(serviceProvider));

        /// <summary>
        /// Opens an existing URI for writing.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="httpClientFactory">The <see cref="System.Net.Http"/> client factory.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "This will be used in the future.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This will be used in the future.")]
        public static Stream OpenWrite(Uri uri, Func<HttpClient> httpClientFactory)
        {
            return uri switch
            {
                { Scheme: "file" } => FileOpenWrite(uri.LocalPath),
                _ => throw new NotSupportedException(),
            };

            static Stream FileOpenWrite(string path)
            {
                CreateDirectoryIfPossible(path);
                return File.Open(path, FileMode.Create);
            }
        }

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether an uri is created if one does not exist, and determines whether the contents of existing uris are retained or overwritten.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream Open(Uri uri, FileMode mode) => Open(uri, mode, static () => new());

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether an uri is created if one does not exist, and determines whether the contents of existing uris are retained or overwritten.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream Open(Uri uri, FileMode mode, IServiceProvider? serviceProvider) => Open(uri, mode, CreateHttpClient(serviceProvider));

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether an uri is created if one does not exist, and determines whether the contents of existing uris are retained or overwritten.</param>
        /// <param name="httpClientFactory">The <see cref="System.Net.Http"/> client factory.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream Open(Uri uri, FileMode mode, Func<HttpClient> httpClientFactory) => (uri, mode) switch
        {
            ({ Scheme: "file" }, _) => File.Open(uri.LocalPath, mode),
            (_, FileMode.Open) when S3.S3UriUtility.IsS3(uri) => S3.S3Las.OpenRead(uri),
            ({ Scheme: "http" or "https" }, FileMode.Open) => Http.HttpLas.OpenRead(uri, httpClientFactory),
            _ => throw new NotSupportedException(),
        };

#pragma warning disable IDE0051, S1144
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

        private static Func<HttpClient> CreateHttpClient(IServiceProvider? serviceProvider)
        {
            if (serviceProvider is not null)
            {
                return serviceProvider.GetRequiredService<HttpClient>;
            }

            return CreateHttpClient;
        }

        private static HttpClient CreateHttpClient() => new();
#pragma warning restore IDE0051, S1144
    }
}