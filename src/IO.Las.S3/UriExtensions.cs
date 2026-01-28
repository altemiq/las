// -----------------------------------------------------------------------
// <copyright file="UriExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace System;
#pragma warning restore IDE0130, CheckNamespace

/// <summary>
/// <see cref="Uri"/> extensions.
/// </summary>
public static class UriExtensions
{
    extension(Uri)
    {
        /// <summary>
        /// Gets that the URI is a pointer to an S3 location. This field is read-only.
        /// </summary>
        public static string UriSchemeS3 => "s3";
    }
}