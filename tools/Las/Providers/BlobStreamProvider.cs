// -----------------------------------------------------------------------
// <copyright file="BlobStreamProvider.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Providers;

/// <summary>
/// The <see cref="Azure"/> <see cref="Stream"/> provider.
/// </summary>
public class BlobStreamProvider : IStreamProvider
{
    /// <inheritdoc />
    public bool CanRead => true;

    /// <inheritdoc />
    public bool CanWrite => false;

    /// <inheritdoc />
    public bool IsValid(Uri uri) => Azure.AzureUri.IsAzureBlobUri(uri);

    /// <inheritdoc />
    public bool Exists(Uri uri) => Azure.BlobLas.Exists(uri);

    /// <inheritdoc />
    public ValueTask<bool> ExistsAsync(Uri uri, CancellationToken cancellationToken = default) => new(Azure.BlobLas.ExistsAsync(uri, cancellationToken));

    /// <inheritdoc />
    public Stream OpenRead(Uri uri) => Azure.BlobLas.OpenRead(uri);

    /// <inheritdoc />
    public ValueTask<Stream> OpenReadAsync(Uri uri, CancellationToken cancellationToken = default) => new(Azure.BlobLas.OpenReadAsync(uri, cancellationToken));

    /// <inheritdoc />
    public Stream OpenWrite(Uri uri) => throw new NotSupportedException();

    /// <inheritdoc />
    public ValueTask<Stream> OpenWriteAsync(Uri uri, CancellationToken cancellationToken = default) => throw new NotSupportedException();
}