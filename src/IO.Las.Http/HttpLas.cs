// -----------------------------------------------------------------------
// <copyright file="HttpLas.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Http;

/// <summary>
/// Methods for <see cref="Http"/> <see cref="Stream"/> instances.
/// </summary>
public static class HttpLas
{
    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri uri) => Exists(uri, new HttpClient());

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri uri, IServiceProvider serviceProvider) => Exists(uri, CreateHttpClient(serviceProvider));

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri uri, HttpClient client) => ExistsAsync(uri, () => client).Result;

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri uri, Func<HttpClient> clientFactory) => Exists(uri, clientFactory());

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri uri) => ExistsAsync(uri, new HttpClient());

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri uri, IServiceProvider serviceProvider) => ExistsAsync(uri, CreateHttpClient(serviceProvider));

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri uri, HttpClient client) => ExistsAsync(client, uri);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri uri, Func<HttpClient> clientFactory) => ExistsAsync(uri, clientFactory());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead(Uri uri) => OpenRead(uri, new HttpClient());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead(Uri uri, IServiceProvider serviceProvider) => OpenRead(uri, CreateHttpClient(serviceProvider));

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead(Uri uri, HttpClient client) => new HttpChunkedStream(client, uri, GetContentLengthAsync(client, uri).Result ?? -1);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead(Uri uri, Func<HttpClient> clientFactory) => OpenRead(uri, clientFactory());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(Uri uri) => OpenReadAsync(uri, new HttpClient());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(Uri uri, IServiceProvider serviceProvider) => OpenReadAsync(uri, CreateHttpClient(serviceProvider));

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static async Task<Stream> OpenReadAsync(Uri uri, HttpClient client) => new HttpChunkedStream(client, uri, await GetContentLengthAsync(client, uri).ConfigureAwait(false) ?? -1);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(Uri uri, Func<HttpClient> clientFactory) => OpenReadAsync(uri, clientFactory());

    private static async Task<bool> ExistsAsync(HttpClient client, Uri uri)
    {
        var response = await client.SendAsync(new(HttpMethod.Head, uri), HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    private static async Task<long?> GetContentLengthAsync(HttpClient client, Uri uri)
    {
        var response = await client.SendAsync(new(HttpMethod.Head, uri), HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        return response.Content.Headers.ContentLength;
    }

    private static HttpClient CreateHttpClient(IServiceProvider serviceProvider) => serviceProvider.GetService(typeof(HttpClient)) is HttpClient httpClient
        ? httpClient
        : throw new InvalidOperationException(string.Format(Properties.Resources.Culture, Properties.Resources.FailedToCreate, nameof(httpClient)));
}