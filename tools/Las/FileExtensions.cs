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
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri, IServiceProvider? serviceProvider = default) => File.Exists(uri, GetStreamProviders(serviceProvider));

        /// <summary>
        /// Determines whether the specified uri exists.
        /// </summary>
        /// <param name="uri">The uri to check.</param>
        /// <param name="providers">The stream providers.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri, IEnumerable<IStreamProvider> providers) => providers.Any(provider => provider.IsValid(uri) && provider.Exists(uri));

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenRead(Uri uri, IServiceProvider? serviceProvider = default) => File.OpenRead(uri, GetStreamProviders(serviceProvider));

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="providers">The stream providers.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenRead(Uri uri, IEnumerable<IStreamProvider> providers)
            => providers
                .Where(provider => provider.CanRead && provider.IsValid(uri))
                .Select(provider => provider.OpenRead(uri))
                .FirstOrDefault() ?? throw new NotSupportedException();

        /// <summary>
        /// Opens an existing URI for writing.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream OpenWrite(Uri uri, IServiceProvider? serviceProvider = default) => File.OpenWrite(uri, GetStreamProviders(serviceProvider));

        /// <summary>
        /// Opens an existing URI for writing.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="providers">The stream providers.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "This will be used in the future.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This will be used in the future.")]
        public static Stream OpenWrite(Uri uri, IEnumerable<IStreamProvider> providers)
            => providers
                .Where(provider => provider.CanWrite && provider.IsValid(uri))
                .Select(provider => provider.OpenWrite(uri))
                .FirstOrDefault() ?? throw new NotSupportedException();

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a uri is created if one does not exist, and determines whether the contents of existing uris are retained or overwritten.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream Open(Uri uri, FileMode mode, IServiceProvider? serviceProvider = default) => File.Open(uri, mode, GetStreamProviders(serviceProvider));

        /// <summary>
        /// Opens an existing URI for reading.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a uri is created if one does not exist, and determines whether the contents of existing uris are retained or overwritten.</param>
        /// <param name="providers">The stream providers.</param>
        /// <returns>The stream from URI.</returns>
        /// <exception cref="InvalidOperationException">The uri scheme is not supported.</exception>
        public static Stream Open(Uri uri, FileMode mode, IEnumerable<IStreamProvider> providers)
        {
            foreach (var provider in providers)
            {
                if (provider.IsValid(uri))
                {
                    continue;
                }

                if (mode is FileMode.Open
                    && provider.CanRead
                    && provider.OpenRead(uri) is { } readStream)
                {
                    return readStream;
                }

                if (provider.CanWrite
                    && provider.OpenRead(uri) is { } writeStream)
                {
                    return writeStream;
                }
            }

            throw new NotSupportedException();
        }
    }

    private static IEnumerable<IStreamProvider> GetStreamProviders(IServiceProvider? serviceProvider) =>
        serviceProvider is not null
            ? serviceProvider.GetServices<IStreamProvider>()
            : [new Providers.FileStreamProvider()];
}