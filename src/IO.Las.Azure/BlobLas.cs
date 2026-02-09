// -----------------------------------------------------------------------
// <copyright file="BlobLas.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Azure;

using global::Azure.Storage.Blobs;
using global::Azure.Storage.Blobs.Specialized;

/// <summary>
/// Methods for <see cref="global::Azure.Storage.Blobs"/> <see cref="Stream"/> instances.
/// </summary>
public static class BlobLas
{
    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri uri) => Exists(new BlobBaseClient(uri));

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="lazyClient">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri uri, Lazy<BlobServiceClient> lazyClient) => Exists(uri, lazyClient.Value);

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> exists using the <see cref="BlobServiceClient"/>.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri uri, BlobServiceClient client)
    {
        var builder = new BlobUriBuilder(uri);
        return Exists(client.GetBlobContainerClient(builder.BlobContainerName).GetBlobBaseClient(builder.BlobName));
    }

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> container/blobName combination exists.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="lazyClient">The <see cref="BlobServiceClient"/>.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="blobName"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(string blobName, Lazy<BlobContainerClient> lazyClient) => Exists(blobName, lazyClient.Value);

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> container/blobName combination exists using the <see cref="BlobServiceClient"/>.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="blobName"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(string blobName, BlobContainerClient client) => Exists(client.GetBlobBaseClient(blobName));

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> container/blobName combination exists.
    /// </summary>
    /// <param name="container">The container name.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="lazyClient">The <see cref="BlobServiceClient"/>.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="container"/> and <paramref name="blobName"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(string container, string blobName, Lazy<BlobServiceClient> lazyClient) => Exists(container, blobName, lazyClient.Value);

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> container/blobName combination exists using the <see cref="BlobServiceClient"/>.
    /// </summary>
    /// <param name="container">The container name.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="container"/> and <paramref name="blobName"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(string container, string blobName, BlobServiceClient client) => Exists(client.GetBlobContainerClient(container).GetBlobBaseClient(blobName));

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri uri) => ExistsAsync(new BlobBaseClient(uri));

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="lazyClient">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri uri, Lazy<BlobServiceClient> lazyClient) => ExistsAsync(uri, lazyClient.Value);

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> exists using the <see cref="BlobServiceClient"/>.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri uri, BlobServiceClient client)
    {
        var builder = new BlobUriBuilder(uri);
        return ExistsAsync(builder.BlobContainerName, builder.BlobName, client);
    }

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> container/blobName combination exists.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="lazyClient">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="blobName"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(string blobName, Lazy<BlobContainerClient> lazyClient) => ExistsAsync(blobName, lazyClient.Value);

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> container/blobName combination exists using the <see cref="BlobServiceClient"/>.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="blobName"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(string blobName, BlobContainerClient client) => ExistsAsync(client.GetBlobBaseClient(blobName));

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> container/blobName combination exists.
    /// </summary>
    /// <param name="container">The container name.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="lazyClient">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="container"/> and <paramref name="blobName"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(string container, string blobName, Lazy<BlobServiceClient> lazyClient) => ExistsAsync(container, blobName, lazyClient.Value);

    /// <summary>
    /// Determines whether the specified <see cref="global::Azure.Storage.Blobs"/> container/blobName combination exists using the <see cref="BlobServiceClient"/>.
    /// </summary>
    /// <param name="container">The container name.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="container"/> and <paramref name="blobName"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(string container, string blobName, BlobServiceClient client) => ExistsAsync(client.GetBlobContainerClient(container).GetBlobBaseClient(blobName));

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static Stream OpenRead(Uri uri)
    {
        var (client, length) = GetClientAndLength(new(uri));
        return new BlobChunkedStream(client, length);
    }

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="lazyClient">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static Stream OpenRead(Uri uri, Lazy<BlobServiceClient> lazyClient) => OpenRead(uri, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> for reading using the <see cref="BlobServiceClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static Stream OpenRead(Uri uri, BlobServiceClient client)
    {
        var builder = new BlobUriBuilder(uri);
        var (blobBaseClient, length) = GetClientAndLength(client, builder.BlobContainerName, builder.BlobName);
        return new BlobChunkedStream(blobBaseClient, length);
    }

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> container/blobName combination for reading.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="lazyClient">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static Stream OpenRead(string blobName, Lazy<BlobContainerClient> lazyClient) => OpenRead(blobName, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> container/blobName combination for reading using the <see cref="BlobContainerClient"/>.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static Stream OpenRead(string blobName, BlobContainerClient client)
    {
        var (blobBaseClient, length) = GetClientAndLength(client, blobName);
        return new BlobChunkedStream(blobBaseClient, length);
    }

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> container/blobName combination for reading.
    /// </summary>
    /// <param name="container">The container name.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="lazyClient">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static Stream OpenRead(string container, string blobName, Lazy<BlobServiceClient> lazyClient) => OpenRead(container, blobName, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> container/blobName combination for reading using the <see cref="BlobContainerClient"/>.
    /// </summary>
    /// <param name="container">The container name.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static Stream OpenRead(string container, string blobName, BlobServiceClient client)
    {
        var (blobBaseClient, length) = GetClientAndLength(client, container, blobName);
        return new BlobChunkedStream(blobBaseClient, length);
    }

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static async Task<Stream> OpenReadAsync(Uri uri)
    {
        var (client, length) = await GetClientAndLengthAsync(new(uri), CancellationToken.None).ConfigureAwait(false);
        return new BlobChunkedStream(client, length);
    }

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="lazyClient">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(Uri uri, Lazy<BlobServiceClient> lazyClient) => OpenReadAsync(uri, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> for reading using the <see cref="BlobServiceClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="global::Azure.Storage.Blobs"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static async Task<Stream> OpenReadAsync(Uri uri, BlobServiceClient client)
    {
        var builder = new BlobUriBuilder(uri);
        var (blobBaseClient, length) = await GetClientAndLengthAsync(client, builder.BlobContainerName, builder.BlobName, CancellationToken.None).ConfigureAwait(false);
        return new BlobChunkedStream(blobBaseClient, length);
    }

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> container/blobName combination for reading.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="lazyClient">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(string blobName, Lazy<BlobContainerClient> lazyClient) => OpenReadAsync(blobName, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> container/blobName combination for reading using the <see cref="BlobContainerClient"/>.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static async Task<Stream> OpenReadAsync(string blobName, BlobContainerClient client)
    {
        var (blobBaseClient, length) = await GetClientAndLengthAsync(client, blobName, CancellationToken.None).ConfigureAwait(false);
        return new BlobChunkedStream(blobBaseClient, length);
    }

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> container/blobName combination for reading.
    /// </summary>
    /// <param name="container">The container name.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="lazyClient">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(string container, string blobName, Lazy<BlobServiceClient> lazyClient) => OpenReadAsync(container, blobName, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="global::Azure.Storage.Blobs"/> container/blobName combination for reading using the <see cref="BlobContainerClient"/>.
    /// </summary>
    /// <param name="container">The container name.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="client">The <see cref="global::Azure.Storage.Blobs"/> client.</param>
    /// <returns>The <see cref="global::Azure.Storage.Blobs"/> stream.</returns>
    public static async Task<Stream> OpenReadAsync(string container, string blobName, BlobServiceClient client)
    {
        var (blobBaseClient, length) = await GetClientAndLengthAsync(client, container, blobName, CancellationToken.None).ConfigureAwait(false);
        return new BlobChunkedStream(blobBaseClient, length);
    }

    private static bool Exists<T>(T client)
        where T : BlobBaseClient => client.Exists();

    private static async Task<bool> ExistsAsync<T>(T client)
        where T : BlobBaseClient
    {
        var response = await client.ExistsAsync().ConfigureAwait(false);
        return response.HasValue && response.Value;
    }

    private static (BlobBaseClient Client, long Length) GetClientAndLength(BlobServiceClient client, string container, string blobName) =>
        GetClientAndLength(client.GetBlobContainerClient(container), blobName);

    private static (BlobBaseClient Client, long Length) GetClientAndLength(BlobContainerClient client, string blobName) =>
        GetClientAndLength(client.GetBlobBaseClient(blobName));

    private static (BlobBaseClient Client, long Length) GetClientAndLength(BlobBaseClient client) =>
        client.GetProperties() is { HasValue: true, Value: var properties }
            ? (client, properties.ContentLength)
            : throw new System.Diagnostics.UnreachableException();

    private static Task<(BlobBaseClient Client, long Length)> GetClientAndLengthAsync(BlobServiceClient client, string container, string blobName, CancellationToken cancellationToken) =>
        GetClientAndLengthAsync(client.GetBlobContainerClient(container), blobName, cancellationToken);

    private static Task<(BlobBaseClient Client, long Length)> GetClientAndLengthAsync(BlobContainerClient client, string blobName, CancellationToken cancellationToken) =>
        GetClientAndLengthAsync(client.GetBlobBaseClient(blobName), cancellationToken);

    private static async Task<(BlobBaseClient Client, long Length)> GetClientAndLengthAsync(BlobBaseClient client, CancellationToken cancellationToken) =>
        await client.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false) is { HasValue: true, Value: var properties }
            ? (client, properties.ContentLength)
            : throw new System.Diagnostics.UnreachableException();
}