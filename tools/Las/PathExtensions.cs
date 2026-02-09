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
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri, IServiceProvider? serviceProvider = default) => Path.Exists(uri, GetStreamProviders(serviceProvider));

        /// <summary>
        /// Determines whether the specified uri exists.
        /// </summary>
        /// <param name="uri">The uri to check.</param>
        /// <param name="providers">The stream providers.</param>
        /// <returns><see langword="true"/> if <paramref name="uri"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool Exists(Uri uri, IEnumerable<IStreamProvider> providers) => providers.Any(provider => provider.IsValid(uri) && provider.Exists(uri));

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

    private static IEnumerable<IStreamProvider> GetStreamProviders(IServiceProvider? serviceProvider) =>
        serviceProvider is not null
            ? serviceProvider.GetServices<IStreamProvider>()
            : [new Providers.FileStreamProvider()];
}