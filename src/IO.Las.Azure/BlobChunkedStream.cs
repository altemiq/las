// -----------------------------------------------------------------------
// <copyright file="BlobChunkedStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Azure;

/// <summary>
/// An optimized <see cref="global::Azure.Storage.Blobs"/> <see cref="Stream"/> for <see cref="Las"/> files.
/// </summary>
public class BlobChunkedStream(global::Azure.Storage.Blobs.Specialized.BlobBaseClient client, long length, int minimumCacheSize = ushort.MaxValue) : ChunkedStream(length)
{
    /// <inheritdoc/>
    protected override Stream? GetStream(long start, int length)
    {
        var requestLength = Math.Min(Math.Max(length, minimumCacheSize), this.Length - start);
        if (requestLength < 0L)
        {
            return default;
        }

        var range = new global::Azure.HttpRange(start, requestLength);
        if (client.Download(range) is not { Value: { } response })
        {
            return default;
        }

        var stream = new MemoryStream((int)response.ContentLength);
        response.Content.CopyTo(stream);
        stream.Position = 0;
        return stream;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Stream?> GetStreamAsync(long start, int length, CancellationToken cancellationToken = default)
    {
        var requestLength = Math.Min(Math.Max(length, minimumCacheSize), this.Length - start);
        if (requestLength < 0L)
        {
            return default;
        }

        var range = new global::Azure.HttpRange(start, requestLength);
        if (await client.DownloadAsync(range, cancellationToken: cancellationToken).ConfigureAwait(false) is not { Value: { } response })
        {
            return default;
        }

        var stream = new MemoryStream((int)response.ContentLength);
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        await response.Content.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
#else
        await response.Content.CopyToAsync(stream).ConfigureAwait(false);
#endif
        stream.Position = 0;
        return stream;
    }
}