// -----------------------------------------------------------------------
// <copyright file="S3ChunkedStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.S3;

/// <summary>
/// An optimized <see cref="Amazon.S3"/> <see cref="Stream"/> for <see cref="Las"/> files.
/// </summary>
public sealed class S3ChunkedStream : ChunkedStream
{
    private readonly Amazon.S3.IAmazonS3 client;

    private readonly string bucket;

    private readonly string key;

    /// <summary>
    /// Initializes a new instance of the <see cref="S3ChunkedStream"/> class.
    /// </summary>
    /// <param name="uri">The S3 URL.</param>
    /// <param name="length">The length of the resource.</param>
    internal S3ChunkedStream(Uri uri, long length)
        : this(new Amazon.S3.AmazonS3Client(), uri, length)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="S3ChunkedStream"/> class.
    /// </summary>
    /// <param name="client">The S3 client.</param>
    /// <param name="uri">The S3 URL.</param>
    /// <param name="length">The length of the resource.</param>
    internal S3ChunkedStream(Amazon.S3.IAmazonS3 client, Uri uri, long length)
        : base(length)
    {
        this.client = client;
        (this.bucket, this.key) = S3UriUtility.GetBucketNameAndKey(uri);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="S3ChunkedStream"/> class.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <param name="length">The length of the resource.</param>
    internal S3ChunkedStream(string bucket, string key, long length)
        : this(new Amazon.S3.AmazonS3Client(), bucket, key, length)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="S3ChunkedStream"/> class.
    /// </summary>
    /// <param name="client">The S3 client.</param>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <param name="length">The length of the resource.</param>
    internal S3ChunkedStream(Amazon.S3.IAmazonS3 client, string bucket, string key, long length)
        : base(length) => (this.client, this.bucket, this.key) = (client, bucket, key);

    /// <inheritdoc/>
    protected override Stream? GetStream(long start, int length)
    {
        var end = Math.Min(start + length - 1, this.Length - 1);
        if (start >= end)
        {
            return default;
        }

        var request = new Amazon.S3.Model.GetObjectRequest
        {
            BucketName = this.bucket,
            Key = this.key,
            ByteRange = new(start, end),
        };

        if (GetResponse(this.client, request) is not { ResponseStream: { } responseStream })
        {
            return default;
        }

        using (responseStream)
        {
            var stream = new MemoryStream(length);
            responseStream.CopyTo(stream);
            stream.Position = 0;
            return stream;
        }

        static Amazon.S3.Model.GetObjectResponse GetResponse(Amazon.S3.IAmazonS3 client, Amazon.S3.Model.GetObjectRequest request)
        {
            return
#if NETFRAMEWORK
                client.GetObject(request);
#else
                client.GetObjectAsync(request).GetAwaiter().GetResult();
#endif
        }
    }

    /// <inheritdoc/>
    protected override async ValueTask<Stream?> GetStreamAsync(long start, int length, CancellationToken cancellationToken = default)
    {
        var end = Math.Min(start + length - 1, this.Length - 1);
        if (start >= end)
        {
            return default;
        }

        var request = new Amazon.S3.Model.GetObjectRequest
        {
            BucketName = this.bucket,
            Key = this.key,
            ByteRange = new(start, end),
        };

        if (await this.client.GetObjectAsync(request, cancellationToken).ConfigureAwait(false) is not { ResponseStream: { } responseStream })
        {
            return default;
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
        await using (responseStream.ConfigureAwait(false))
#else
        using (responseStream)
#endif
        {
            var stream = new MemoryStream(length);
            await responseStream.CopyToAsync(stream, 81920, cancellationToken).ConfigureAwait(false);
            stream.Position = 0;
            return stream;
        }
    }
}