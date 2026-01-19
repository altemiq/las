// -----------------------------------------------------------------------
// <copyright file="HttpClientExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The <see cref="HttpClient"/> extensions.
/// </summary>
internal static class HttpClientExtensions
{
    extension(HttpClient)
    {
        /// <summary>
        /// Creates a <see cref="HttpClient"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The client.</returns>
        public static Lazy<HttpClient> Create(IServiceProvider? serviceProvider = default) => serviceProvider is not null
            ? new(serviceProvider.GetRequiredService<HttpClient>)
            : new(static () => new());
    }
}