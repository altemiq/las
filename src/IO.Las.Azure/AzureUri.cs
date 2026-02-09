// -----------------------------------------------------------------------
// <copyright file="AzureUri.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Azure;

/// <summary>
/// The <see cref="global::Azure"/> <see cref="Uri"/>.
/// </summary>
public static class AzureUri
{
    /// <summary>
    /// Checks whether the given URI is an <see cref="global::Azure"/> <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">The S3 URI to be checked.</param>
    /// <returns><see langword="true"/> if the URI is an <see cref="global::Azure"/> <see cref="Uri"/>, <see langword="false"/>; otherwise.</returns>
    public static bool IsAzureBlobUri(Uri uri)
    {
        var builder = new global::Azure.Storage.Blobs.BlobUriBuilder(uri);
        return builder.AccountName is not null
               && builder.BlobContainerName is not null
               && builder.BlobName is not null
               && builder.Host.EndsWith("blob.core.windows.net", StringComparison.OrdinalIgnoreCase);
    }
}