// -----------------------------------------------------------------------
// <copyright file="HttpChunkedStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Http;

/// <summary>
/// The <see cref="System.Net.Http"/> <see cref="Stream"/> for LAS.
/// </summary>
public sealed class HttpChunkedStream : ChunkedStream
{
    private readonly HttpClient httpClient;

    private readonly Uri uri;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpChunkedStream"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="length">The content length.</param>
    internal HttpChunkedStream(HttpClient httpClient, Uri uri, long length)
        : base(length) => (this.httpClient, this.uri) = (httpClient, uri);

    /// <inheritdoc/>
    protected override Stream? GetStream(long start, int length)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = this.uri,
            Headers =
            {
                Range = new(start, start + length - 1),
            },
        };

        return this.httpClient.SendAsync(request).Result is { IsSuccessStatusCode: true } response
            ? response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
            : default;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Stream?> GetStreamAsync(long start, int length, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = this.uri,
            Headers =
            {
                Range = new(start, start + length - 1),
            },
        };

        return await this.httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false) is { IsSuccessStatusCode: true } response
            ? await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false)
            : default;
    }
}