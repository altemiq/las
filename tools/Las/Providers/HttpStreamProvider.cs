// -----------------------------------------------------------------------
// <copyright file="HttpStreamProvider.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Providers;

/// <summary>
/// The <see cref="Http"/> <see cref="Stream"/> provider.
/// </summary>
public class HttpStreamProvider(HttpClient client) : IStreamProvider
{
    /// <inheritdoc />
    public bool CanRead => true;

    /// <inheritdoc />
    public bool CanWrite => false;

    /// <inheritdoc />
    public bool IsValid(Uri uri) => uri is { Scheme: "http" or "https" };

    /// <inheritdoc />
    public bool Exists(Uri uri) => Http.HttpLas.Exists(uri, client);

    /// <inheritdoc />
    public ValueTask<bool> ExistsAsync(Uri uri, CancellationToken cancellationToken = default) => new(Http.HttpLas.ExistsAsync(uri, client, cancellationToken));

    /// <inheritdoc />
    public Stream OpenRead(Uri uri) => Http.HttpLas.OpenRead(uri);

    /// <inheritdoc />
    public ValueTask<Stream> OpenReadAsync(Uri uri, CancellationToken cancellationToken = default) => new(Http.HttpLas.OpenReadAsync(uri, cancellationToken));

    /// <inheritdoc />
    public Stream OpenWrite(Uri uri) => throw new NotSupportedException();

    /// <inheritdoc />
    public ValueTask<Stream> OpenWriteAsync(Uri uri, CancellationToken cancellationToken = default) => throw new NotSupportedException();
}