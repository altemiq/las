// -----------------------------------------------------------------------
// <copyright file="S3StreamProvider.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Providers;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The <see cref="S3"/> <see cref="Stream"/> provider.
/// </summary>
public class S3StreamProvider(IServiceProvider services) : IStreamProvider
{
    private readonly Lazy<Amazon.S3.IAmazonS3> client = new(services.GetRequiredService<Amazon.S3.IAmazonS3>, LazyThreadSafetyMode.PublicationOnly);

    /// <inheritdoc />
    public bool CanRead => true;

    /// <inheritdoc />
    public bool CanWrite => false;

    /// <inheritdoc />
    public bool IsValid(Uri uri) => Amazon.S3.Util.AmazonS3Uri.IsAmazonS3Endpoint(uri);

    /// <inheritdoc />
    public bool Exists(Uri uri) => S3.S3Las.Exists(uri, this.GetClient());

    /// <inheritdoc />
    public ValueTask<bool> ExistsAsync(Uri uri, CancellationToken cancellationToken = default) => new(S3.S3Las.ExistsAsync(uri, this.GetClient(), cancellationToken));

    /// <inheritdoc />
    public Stream OpenRead(Uri uri) => S3.S3Las.OpenRead(uri, this.GetClient());

    /// <inheritdoc />
    public ValueTask<Stream> OpenReadAsync(Uri uri, CancellationToken cancellationToken = default) => new(S3.S3Las.OpenReadAsync(uri, this.GetClient(), cancellationToken));

    /// <inheritdoc />
    public Stream OpenWrite(Uri uri) => throw new NotSupportedException();

    /// <inheritdoc />
    public ValueTask<Stream> OpenWriteAsync(Uri uri, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    private Amazon.S3.IAmazonS3 GetClient() => this.client.Value;
}