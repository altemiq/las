// -----------------------------------------------------------------------
// <copyright file="UriExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable CA1050, MA0047, RCS1110

/// <summary>
/// <see cref="Uri"/> extensions.
/// </summary>
public static class UriExtensions
{
    extension(Uri)
    {
        /// <inheritdoc cref="Altemiq.IO.Las.Azure.AzureUri.IsAzureBlobUri" />
        public static bool IsAzureBlobUri(Uri uri) => Altemiq.IO.Las.Azure.AzureUri.IsAzureBlobUri(uri);
    }
}