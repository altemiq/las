// -----------------------------------------------------------------------
// <copyright file="S3Las.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.S3;

/// <summary>
/// Methods for <see cref="S3"/> <see cref="Stream"/> instances.
/// </summary>
public static class S3Las
{
    /// <summary>
    /// Determines whether the specified <see cref="S3"/> <see cref="Uri"/> exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri uri) => Exists(uri, new Amazon.S3.AmazonS3Client());

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> <see cref="Uri"/> exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="lazyClient">The <see cref="S3"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri uri, Lazy<Amazon.S3.IAmazonS3> lazyClient) => Exists(uri, lazyClient.Value);

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> <see cref="Uri"/> exists using the <see cref="Amazon.S3.IAmazonS3"/>.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="client">The <see cref="S3"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(Uri uri, Amazon.S3.IAmazonS3 client)
    {
        var (bucketName, key) = S3UriUtility.GetBucketNameAndKey(uri);
        return Exists(bucketName, key, client);
    }

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> bucket/key combination exists.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="bucket"/> and <paramref name="key"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(string bucket, string key) => Exists(bucket, key, new Amazon.S3.AmazonS3Client());

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> bucket/key combination exists.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <param name="lazyClient">The <see cref="S3"/> client.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="bucket"/> and <paramref name="key"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(string bucket, string key, Lazy<Amazon.S3.IAmazonS3> lazyClient) => Exists(bucket, key, lazyClient.Value);

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> bucket/key combination exists using the <see cref="Amazon.S3.IAmazonS3"/>.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <param name="client">The <see cref="S3"/> client.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="bucket"/> and <paramref name="key"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static bool Exists(string bucket, string key, Amazon.S3.IAmazonS3 client) => ExistsAsync(client, bucket, key).Result;

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> <see cref="Uri"/> exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri uri) => ExistsAsync(uri, new Amazon.S3.AmazonS3Client());

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> <see cref="Uri"/> exists.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="lazyClient">The <see cref="S3"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri uri, Lazy<Amazon.S3.IAmazonS3> lazyClient) => ExistsAsync(uri, lazyClient.Value);

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> <see cref="Uri"/> exists using the <see cref="Amazon.S3.IAmazonS3"/>.
    /// </summary>
    /// <param name="uri">The uri to check.</param>
    /// <param name="client">The <see cref="S3"/> client.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(Uri uri, Amazon.S3.IAmazonS3 client)
    {
        var (bucketName, key) = S3UriUtility.GetBucketNameAndKey(uri);
        return ExistsAsync(bucketName, key, client);
    }

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> bucket/key combination exists.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="bucket"/> and <paramref name="key"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(string bucket, string key) => ExistsAsync(bucket, key, new Amazon.S3.AmazonS3Client());

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> bucket/key combination exists.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <param name="lazyClient">The <see cref="S3"/> client.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="bucket"/> and <paramref name="key"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(string bucket, string key, Lazy<Amazon.S3.IAmazonS3> lazyClient) => ExistsAsync(bucket, key, lazyClient.Value);

    /// <summary>
    /// Determines whether the specified <see cref="S3"/> bucket/key combination exists using the <see cref="Amazon.S3.IAmazonS3"/>.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <param name="client">The <see cref="S3"/> client.</param>
    /// <returns><see langword="true"/> if the resource at <paramref name="bucket"/> and <paramref name="key"/> returns from a request successfully; otherwise <see langword="false"/>.</returns>
    public static Task<bool> ExistsAsync(string bucket, string key, Amazon.S3.IAmazonS3 client) => ExistsAsync(client, bucket, key);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="S3"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static Stream OpenRead(Uri uri) => OpenRead(uri, new Amazon.S3.AmazonS3Client());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="S3"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="lazyClient">The <see cref="S3"/> client.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static Stream OpenRead(Uri uri, Lazy<Amazon.S3.IAmazonS3> lazyClient) => OpenRead(uri, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> <see cref="Uri"/> for reading using the <see cref="Amazon.S3.IAmazonS3"/>.
    /// </summary>
    /// <param name="uri">The <see cref="S3"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="client">The <see cref="S3"/> client.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static Stream OpenRead(Uri uri, Amazon.S3.IAmazonS3 client)
    {
        var (bucketName, key) = S3UriUtility.GetBucketNameAndKey(uri);
        return new S3ChunkedStream(client, bucketName, key, GetContentLengthAsync(client, uri).GetAwaiter().GetResult());
    }

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> bucket/key combination for reading.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static Stream OpenRead(string bucket, string key) => OpenRead(bucket, key, new Amazon.S3.AmazonS3Client());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> bucket/key combination for reading.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <param name="lazyClient">The <see cref="S3"/> client.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static Stream OpenRead(string bucket, string key, Lazy<Amazon.S3.IAmazonS3> lazyClient) => OpenRead(bucket, key, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> bucket/key combination for reading using the <see cref="Amazon.S3.IAmazonS3"/>.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <param name="client">The <see cref="S3"/> client.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static Stream OpenRead(string bucket, string key, Amazon.S3.IAmazonS3 client) => new S3ChunkedStream(client, bucket, key, GetContentLengthAsync(client, bucket, key).GetAwaiter().GetResult());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="S3"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(Uri uri) => OpenReadAsync(uri, new Amazon.S3.AmazonS3Client());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> <see cref="Uri"/> for reading.
    /// </summary>
    /// <param name="uri">The <see cref="S3"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="lazyClient">The <see cref="S3"/> client.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(Uri uri, Lazy<Amazon.S3.IAmazonS3> lazyClient) => OpenReadAsync(uri, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> <see cref="Uri"/> for reading using the <see cref="Amazon.S3.IAmazonS3"/> client.
    /// </summary>
    /// <param name="uri">The <see cref="S3"/> <see cref="Uri"/> to the <see cref="Las"/> file.</param>
    /// <param name="client">The <see cref="S3"/> client.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static async Task<Stream> OpenReadAsync(Uri uri, Amazon.S3.IAmazonS3 client)
    {
        var (bucketName, key) = S3UriUtility.GetBucketNameAndKey(uri);
        return new S3ChunkedStream(client, bucketName, key, await GetContentLengthAsync(client, uri).ConfigureAwait(false));
    }

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> bucket/key combination for reading.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(string bucket, string key) => OpenReadAsync(bucket, key, new Amazon.S3.AmazonS3Client());

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> bucket/key combination for reading.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <param name="lazyClient">The <see cref="S3"/> client.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static Task<Stream> OpenReadAsync(string bucket, string key, Lazy<Amazon.S3.IAmazonS3> lazyClient) => OpenReadAsync(bucket, key, lazyClient.Value);

    /// <summary>
    /// Opens the <see cref="Las"/> <see cref="S3"/> bucket/key combination for reading using the <see cref="Amazon.S3.IAmazonS3"/> client.
    /// </summary>
    /// <param name="bucket">The bucket name.</param>
    /// <param name="key">The key to the resource.</param>
    /// <param name="client">The <see cref="S3"/> client.</param>
    /// <returns>The <see cref="S3"/> stream.</returns>
    public static async Task<Stream> OpenReadAsync(string bucket, string key, Amazon.S3.IAmazonS3 client) => new S3ChunkedStream(client, bucket, key, await GetContentLengthAsync(client, bucket, key).ConfigureAwait(false));

    private static async Task<bool> ExistsAsync(Amazon.S3.IAmazonS3 client, string bucket, string key)
    {
        try
        {
            var request = new Amazon.S3.Model.GetObjectMetadataRequest
            {
                BucketName = bucket,
                Key = key,
            };

            var response = await client.GetObjectMetadataAsync(request).ConfigureAwait(false);
            return IsSuccessStatusCode(response.HttpStatusCode);

            static bool IsSuccessStatusCode(System.Net.HttpStatusCode code)
            {
                return (int)code is >= 200 and < 300;
            }
        }
        catch (Amazon.S3.AmazonS3Exception ex) when (ex is { StatusCode: System.Net.HttpStatusCode.NotFound })
        {
            return false;
        }
    }

    private static Task<long> GetContentLengthAsync(Amazon.S3.IAmazonS3 client, Uri uri)
    {
        var (bucket, key) = S3UriUtility.GetBucketNameAndKey(uri);
        return GetContentLengthAsync(client, bucket, key);
    }

    private static async Task<long> GetContentLengthAsync(Amazon.S3.IAmazonS3 client, string bucket, string key)
    {
        var request = new Amazon.S3.Model.GetObjectMetadataRequest
        {
            BucketName = bucket,
            Key = key,
        };

        var response = await client.GetObjectMetadataAsync(request).ConfigureAwait(false);

        return response.ContentLength;
    }
}