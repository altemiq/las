// -----------------------------------------------------------------------
// <copyright file="HttpLas.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Http;

using System.Diagnostics.CodeAnalysis;

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
    public static bool Exists([StringSyntax(StringSyntaxAttribute.Uri)] string? uri) => Exists(CreateUri(uri));

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri? uri) => Exists(uri, new HttpClient());

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, IServiceProvider serviceProvider) => Exists(CreateUri(uri), serviceProvider);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri? uri, IServiceProvider serviceProvider) => Exists(uri, CreateHttpClient(serviceProvider));

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, HttpClient client) => Exists(CreateUri(uri), client);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri? uri, HttpClient client) => ExistsAsync(uri, () => client).Result;

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="lazyClient">The <see cref="Http"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, Lazy<HttpClient> lazyClient) => Exists(CreateUri(uri), lazyClient);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="lazyClient">The <see cref="Http"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri? uri, Lazy<HttpClient> lazyClient) => ExistsAsync(uri, lazyClient.Value).Result;

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, Func<HttpClient> clientFactory) => Exists(CreateUri(uri), clientFactory);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri? uri, Func<HttpClient> clientFactory) => ExistsAsync(uri, clientFactory()).Result;

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, CancellationToken cancellationToken = default) => ExistsAsync(CreateUri(uri), cancellationToken);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri? uri, CancellationToken cancellationToken = default) => ExistsAsync(uri, new HttpClient(), cancellationToken);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => ExistsAsync(CreateUri(uri), serviceProvider, cancellationToken);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri? uri, IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => ExistsAsync(uri, CreateHttpClient(serviceProvider), cancellationToken);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, HttpClient client, CancellationToken cancellationToken = default) => ExistsAsync(CreateUri(uri), client, cancellationToken);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri? uri, HttpClient client, CancellationToken cancellationToken = default) => ExistsAsync(client, uri, cancellationToken);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="lazyClient">The <see cref="Http"/> client.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, Lazy<HttpClient> lazyClient, CancellationToken cancellationToken = default) => ExistsAsync(CreateUri(uri), lazyClient, cancellationToken);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="lazyClient">The <see cref="Http"/> client.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri? uri, Lazy<HttpClient> lazyClient, CancellationToken cancellationToken = default) => ExistsAsync(uri, lazyClient.Value, cancellationToken);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, Func<HttpClient> clientFactory, CancellationToken cancellationToken = default) => ExistsAsync(CreateUri(uri), clientFactory, cancellationToken);

    /// <summary>
    /// Determines whether the specified HTTP uri exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a HEAD request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri? uri, Func<HttpClient> clientFactory, CancellationToken cancellationToken = default) => ExistsAsync(uri, clientFactory(), cancellationToken);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="string"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="string"/> to the <see cref="Las"/> file.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead([StringSyntax(StringSyntaxAttribute.Uri)] string? uri) => OpenRead(CreateUri(uri));

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead(Uri? uri) => OpenRead(uri, new HttpClient());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="string"/> for reading using the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="string"/> to the <see cref="Las"/> file.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, IServiceProvider serviceProvider) => OpenRead(CreateUri(uri), serviceProvider);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead(Uri? uri, IServiceProvider serviceProvider) => OpenRead(uri, CreateHttpClient(serviceProvider));

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="string"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="string"/> to the <see cref="Las"/> file.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, HttpClient client) => OpenRead(CreateUri(uri), client);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead(Uri? uri, HttpClient client) => new HttpChunkedStream(client, uri, GetContentLengthAsync(client, uri, CancellationToken.None).Result ?? -1);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="string"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="string"/> to the <see cref="Las"/> file.</param>
    /// <param name="lazyClient">The <see cref="Http"/> client factory.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, Lazy<HttpClient> lazyClient) => OpenRead(uri, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="lazyClient">The <see cref="Http"/> client factory.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead(Uri? uri, Lazy<HttpClient> lazyClient) => OpenRead(uri, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="string"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="string"/> to the <see cref="Las"/> file.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, Func<HttpClient> clientFactory) => OpenRead(uri, clientFactory());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Stream OpenRead(Uri? uri, Func<HttpClient> clientFactory) => OpenRead(uri, clientFactory());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="string"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="string"/> to the <see cref="Las"/> file.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, CancellationToken cancellationToken = default) => OpenReadAsync(CreateUri(uri), cancellationToken);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(Uri? uri, CancellationToken cancellationToken = default) => OpenReadAsync(uri, new HttpClient(), cancellationToken);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="string"/> for reading using the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="string"/> to the <see cref="Las"/> file.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => OpenReadAsync(CreateUri(uri), serviceProvider, cancellationToken);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(Uri? uri, IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => OpenReadAsync(uri, CreateHttpClient(serviceProvider), cancellationToken);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="string"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="string"/> to the <see cref="Las"/> file.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, HttpClient client, CancellationToken cancellationToken = default) => OpenReadAsync(CreateUri(uri), client, cancellationToken);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="client">The <see cref="Http"/> client.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static async Task<Stream> OpenReadAsync(Uri? uri, HttpClient client, CancellationToken cancellationToken = default) => new HttpChunkedStream(client, uri, await GetContentLengthAsync(client, uri, cancellationToken).ConfigureAwait(false) ?? -1);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="string"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="string"/> to the <see cref="Las"/> file.</param>
    /// <param name="lazyClient">The <see cref="Http"/> clients.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, Lazy<HttpClient> lazyClient, CancellationToken cancellationToken = default) => OpenReadAsync(uri, lazyClient.Value, cancellationToken);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="lazyClient">The <see cref="Http"/> clients.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(Uri? uri, Lazy<HttpClient> lazyClient, CancellationToken cancellationToken = default) => OpenReadAsync(uri, lazyClient.Value, cancellationToken);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="string"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="string"/> to the <see cref="Las"/> file.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? uri, Func<HttpClient> clientFactory, CancellationToken cancellationToken = default) => OpenReadAsync(CreateUri(uri), clientFactory, cancellationToken);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="Http"/> <see cref="Uri"/> for reading using the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Http"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="clientFactory">The <see cref="Http"/> client factory.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The <see cref="Http"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(Uri? uri, Func<HttpClient> clientFactory, CancellationToken cancellationToken = default) => OpenReadAsync(uri, clientFactory(), cancellationToken);

    private static async Task<bool> ExistsAsync(HttpClient client, Uri? uri, CancellationToken cancellationToken)
    {
        var response = await client.SendAsync(new(HttpMethod.Head, uri), HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    private static async Task<long?> GetContentLengthAsync(HttpClient client, Uri? uri, CancellationToken cancellationToken)
    {
        var response = await client.SendAsync(new(HttpMethod.Head, uri), HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        return response.Content.Headers.ContentLength;
    }

    private static HttpClient CreateHttpClient(IServiceProvider serviceProvider) => serviceProvider.GetService(typeof(HttpClient)) is HttpClient httpClient
        ? httpClient
        : throw new InvalidOperationException(string.Format(Properties.Resources.Culture, Properties.Resources.FailedToCreate, nameof(httpClient)));

    private static Uri? CreateUri(string? uri) =>
        string.IsNullOrEmpty(uri) ? null : new Uri(uri, UriKind.RelativeOrAbsolute);
}