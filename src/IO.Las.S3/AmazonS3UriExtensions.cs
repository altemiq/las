// -----------------------------------------------------------------------
// <copyright file="AmazonS3UriExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.S3;

#pragma warning disable RCS1263, SA1101

/// <summary>
/// <see cref="Amazon.S3.Util.AmazonS3Uri"/> extensions.
/// </summary>
public static class AmazonS3UriExtensions
{
    /// <content>
    /// <see cref="Amazon.S3.Util.AmazonS3Uri"/> extensions.
    /// </content>
    /// <param name="uri">The uri.</param>
    extension(Amazon.S3.Util.AmazonS3Uri uri)
    {
        /// <summary>
        /// Gets this <see cref="Amazon.S3.Util.AmazonS3Uri"/> as an S3 style uri.
        /// </summary>
        /// <returns>The S3 style uri.</returns>
        public Uri ToS3Style()
        {
            var builder = new UriBuilder(Uri.UriSchemeS3, uri.Bucket) { Path = uri.Key };
            return builder.Uri;
        }

        /// <summary>
        /// Gets this <see cref="Amazon.S3.Util.AmazonS3Uri"/> as a path style uri.
        /// </summary>
        /// <returns>The path style uri.</returns>
        public Uri ToPathStyle()
        {
            var authority = uri.Region is { } region
                ? $"{Uri.UriSchemeS3}.{region.SystemName}.{Host}"
                : $"{Uri.UriSchemeS3}.{Host}";
            var builder = new UriBuilder(Uri.UriSchemeHttps, authority) { Path = $"{uri.Bucket}/{uri.Key}" };
            return builder.Uri;
        }

        /// <summary>
        /// Gets this <see cref="Amazon.S3.Util.AmazonS3Uri"/> as a virtual hosted style uri.
        /// </summary>
        /// <returns>The virtual hosted style uri.</returns>
        public Uri ToVirtualHostStyle()
        {
            var authority = uri.Region is { } region
                ? $"{uri.Bucket}.{Uri.UriSchemeS3}.{region.SystemName}.{Host}"
                : $"{uri.Bucket}.{Uri.UriSchemeS3}.{Host}";
            var builder = new UriBuilder(Uri.UriSchemeHttps, authority) { Path = uri.Key };
            return builder.Uri;
        }
    }

    private const string Host = "amazonaws.com";
}