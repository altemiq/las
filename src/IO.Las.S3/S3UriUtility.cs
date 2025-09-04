// -----------------------------------------------------------------------
// <copyright file="S3UriUtility.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.S3;

/// <summary>
/// The <see cref="Amazon.S3"/> <see cref="Uri"/> utility.
/// </summary>
public static
#if NET7_0_OR_GREATER
    partial
#endif
    class S3UriUtility
{
    private const string S3 = "s3";

    private const string RegionGroup = "region";
    private const string BucketNameGroup = "bucketname";
    private const string KeyGroup = "key";
    private const string UriSchemeS3 = S3;

    private const string RegionGroupRegexString = $"(?<{RegionGroup}>[-a-zA-Z0-9@:%._\\+~#=]+)";
    private const string BucketGroupRegexString = $"(?<{BucketNameGroup}>[-a-zA-Z0-9@:%._\\+~#=]+)";
    private const string KeyGroupRegexString = $"(?<{KeyGroup}>[-a-zA-Z0-9()!@:%_\\+.~#?&\\/=]*)";
    private const string AwsUrlSuffix = "\\.amazonaws\\.com\\/";

    private const string S3SchemeRegexString = $"{UriSchemeS3}:\\/\\/{BucketGroupRegexString}\\/{KeyGroupRegexString}";
    private const string VirtualHostedStyleRegexString = $"https?:\\/\\/{BucketGroupRegexString}\\.{S3}\\.{RegionGroupRegexString}{AwsUrlSuffix}\\b{KeyGroupRegexString}";
    private const string PathStyleNoRegionRegexString = $"https?:\\/\\/{S3}{AwsUrlSuffix}{BucketGroupRegexString}\\/{KeyGroupRegexString}";
    private const string PathStyleRegexString = $"https?:\\/\\/{S3}\\.{RegionGroupRegexString}{AwsUrlSuffix}{BucketGroupRegexString}\\/{KeyGroupRegexString}";
    private const string PathStyleDashRegexString = $"https?:\\/\\/{S3}-{RegionGroupRegexString}{AwsUrlSuffix}{BucketGroupRegexString}\\/{KeyGroupRegexString}";

#if !NET7_0_OR_GREATER
    private static readonly System.Text.RegularExpressions.Regex S3SchemeRegex = new(S3SchemeRegexString, System.Text.RegularExpressions.RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
    private static readonly System.Text.RegularExpressions.Regex VirtualHostedStyleRegex = new(VirtualHostedStyleRegexString, System.Text.RegularExpressions.RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
    private static readonly System.Text.RegularExpressions.Regex PathStyleNoRegionRegex = new(PathStyleNoRegionRegexString, System.Text.RegularExpressions.RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
    private static readonly System.Text.RegularExpressions.Regex PathStyleRegex = new(PathStyleRegexString, System.Text.RegularExpressions.RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
    private static readonly System.Text.RegularExpressions.Regex PathStyleDashRegex = new(PathStyleDashRegexString, System.Text.RegularExpressions.RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
#endif

    /// <summary>
    /// Gets a value indicating whether the <see cref="Uri"/> represents a <see cref="Amazon.S3"/> source.
    /// </summary>
    /// <param name="uri">The repository.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> represents a <see cref="Amazon.S3"/> source; otherwise <see langword="false"/>.</returns>
    public static bool IsS3(this Uri uri)
    {
        return IsS3Core(uri.OriginalString);

        static bool IsS3Core(string url)
        {
            return S3Scheme().IsMatch(url)
                || VirtualHostedStyle().IsMatch(url)
                || PathStyle().IsMatch(url)
                || PathStyleNoRegion().IsMatch(url)
                || PathStyleDash().IsMatch(url);
        }
    }

    /// <summary>
    /// Tries to transform the URL with the return indicating success.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="output">The output URL.</param>
    /// <returns><see langword="true"/> if <paramref name="url"/> was transformed; otherwise <see langword="false"/>.</returns>
    public static bool TryTransformUri(string? url, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? output)
    {
        if (url is not null
            && Uri.TryCreate(url, UriKind.Absolute, out var inputUri)
            && TryTransformUri(inputUri, out var tempUri))
        {
            output = tempUri.ToString();
            return true;
        }

        output = default;
        return false;
    }

    /// <summary>
    /// Tries to transform the URI with the return indicating success.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="output">The output URI.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> was transformed; otherwise <see langword="false"/>.</returns>
    public static bool TryTransformUri(Uri uri, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Uri? output)
    {
        if (HasS3Scheme(uri))
        {
            output = uri;
            return true;
        }

        return TryMakeUri(uri.OriginalString, out output);
    }

    /// <summary>
    /// Transforms the URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The transformed URL.</returns>
    public static string? TransformUri(string? url) => url switch
    {
        null => default,
        var u when Uri.TryCreate(u, UriKind.Absolute, out var uri) => TransformUri(uri).ToString(),
        _ => ThrowInvalidUri(url, nameof(url)),
    };

    /// <summary>
    /// Transforms the URI.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <returns>The transformed URI.</returns>
    public static Uri TransformUri(Uri uri) => uri switch
    {
        var u when HasS3Scheme(u) => u,
        var u when TryMakeUri(u.OriginalString, out var output) => output,
        _ => ThrowInvalidUri(uri, nameof(uri)),
    };

    /// <summary>
    /// Gets the bucket name and key from the URL.
    /// </summary>
    /// <param name="uri">The S3 URL.</param>
    /// <returns>The bucket name and key from <paramref name="uri"/>.</returns>
    internal static (string BucketName, string Key) GetBucketNameAndKey(Uri uri)
    {
        return TryGetBucketNameAndKey(System.Net.WebUtility.UrlDecode(uri.OriginalString), out var tuple)
            ? tuple
            : ThrowNotFoundException(uri);

        static bool TryGetBucketNameAndKey(string input, out (string BucketName, string Key) tuple)
        {
            return TryGetBucketNameAndKey(S3Scheme(), input, out tuple)
                || TryGetBucketNameAndKey(VirtualHostedStyle(), input, out tuple)
                || TryGetBucketNameAndKey(PathStyle(), input, out tuple)
                || TryGetBucketNameAndKey(PathStyleNoRegion(), input, out tuple)
                || TryGetBucketNameAndKey(PathStyleDash(), input, out tuple);

            static bool TryGetBucketNameAndKey(System.Text.RegularExpressions.Regex regex, string input, out (string BucketName, string Key) tuple)
            {
                if (regex.Match(input) is { Success: true } match)
                {
                    tuple = new(match.Groups[BucketNameGroup].Value, match.Groups[KeyGroup].Value);
                    return true;
                }

                tuple = default;
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.DoesNotReturn]
        static (string BucketName, string Key) ThrowNotFoundException(Uri uri)
        {
            throw new KeyNotFoundException(string.Format(Properties.Resources.Culture, Properties.Resources.FailedToGetBucketNameAndKey, uri));
        }
    }

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    private static T ThrowInvalidUri<T>(T url, string paramName) => throw new ArgumentException(string.Format(Properties.Resources.Culture, Properties.Resources.FailedToTransformUri, url), paramName);

    private static bool HasS3Scheme(Uri uri) => string.Equals(uri.Scheme, UriSchemeS3, StringComparison.OrdinalIgnoreCase);

    private static bool TryMakeUri(string input, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Uri? output)
        => TryMakeUri(VirtualHostedStyle(), input, out output)
        || TryMakeUri(PathStyle(), input, out output)
        || TryMakeUri(PathStyleNoRegion(), input, out output)
        || TryMakeUri(PathStyleDash(), input, out output);

    private static bool TryMakeUri(System.Text.RegularExpressions.Regex regex, string input, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Uri? uri)
    {
        if (regex.Match(input) is { Success: true } match)
        {
            var bucketName = match.Groups[BucketNameGroup];
            var key = match.Groups[KeyGroup];

            var builder = new UriBuilder
            {
                Scheme = UriSchemeS3,
                Host = bucketName.Value,
                Path = key.Value,
            };

            uri = builder.Uri;
            return true;
        }

        uri = default;
        return false;
    }

#if NET7_0_OR_GREATER
    [System.Text.RegularExpressions.GeneratedRegex(S3SchemeRegexString, System.Text.RegularExpressions.RegexOptions.ExplicitCapture, 1000)]
    private static partial System.Text.RegularExpressions.Regex S3Scheme();

    [System.Text.RegularExpressions.GeneratedRegex(VirtualHostedStyleRegexString, System.Text.RegularExpressions.RegexOptions.ExplicitCapture, 1000)]
    private static partial System.Text.RegularExpressions.Regex VirtualHostedStyle();

    [System.Text.RegularExpressions.GeneratedRegex(PathStyleRegexString, System.Text.RegularExpressions.RegexOptions.ExplicitCapture, 1000)]
    private static partial System.Text.RegularExpressions.Regex PathStyle();

    [System.Text.RegularExpressions.GeneratedRegex(PathStyleNoRegionRegexString, System.Text.RegularExpressions.RegexOptions.ExplicitCapture, 1000)]
    private static partial System.Text.RegularExpressions.Regex PathStyleNoRegion();

    [System.Text.RegularExpressions.GeneratedRegex(PathStyleDashRegexString, System.Text.RegularExpressions.RegexOptions.ExplicitCapture, 1000)]
    private static partial System.Text.RegularExpressions.Regex PathStyleDash();
#else
    private static System.Text.RegularExpressions.Regex S3Scheme() => S3SchemeRegex;

    private static System.Text.RegularExpressions.Regex VirtualHostedStyle() => VirtualHostedStyleRegex;

    private static System.Text.RegularExpressions.Regex PathStyle() => PathStyleRegex;

    private static System.Text.RegularExpressions.Regex PathStyleNoRegion() => PathStyleNoRegionRegex;

    private static System.Text.RegularExpressions.Regex PathStyleDash() => PathStyleDashRegex;
#endif
}