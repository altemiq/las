// -----------------------------------------------------------------------
// <copyright file="FileStreamProvider.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Providers;

/// <summary>
/// The file provider.
/// </summary>
public class FileStreamProvider : IStreamProvider
{
    /// <inheritdoc />
    public bool CanRead => true;

    /// <inheritdoc />
    public bool CanWrite => true;

    /// <inheritdoc />
    public bool IsValid(Uri uri) => uri is { Scheme: "file" };

    /// <inheritdoc />
    public bool Exists(Uri uri) => ExistsCore(uri);

    /// <inheritdoc />
    public ValueTask<bool> ExistsAsync(Uri uri, CancellationToken cancellationToken = default) => new(ExistsCore(uri));

    /// <inheritdoc />
    public Stream OpenRead(Uri uri) => OpenReadCore(uri);

    /// <inheritdoc />
    public ValueTask<Stream> OpenReadAsync(Uri uri, CancellationToken cancellationToken = default) => new(OpenReadCore(uri));

    /// <inheritdoc />
    public Stream OpenWrite(Uri uri) => OpenWriteCore(uri);

    /// <inheritdoc />
    public ValueTask<Stream> OpenWriteAsync(Uri uri, CancellationToken cancellationToken = default) => new(OpenWriteCore(uri));

    private static bool ExistsCore(Uri uri) => uri is { Scheme: "file", LocalPath: { } path } && Path.Exists(path);

    private static FileStream OpenReadCore(Uri uri) => new(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, ushort.MaxValue);

    private static FileStream OpenWriteCore(Uri uri)
    {
        var path = uri.LocalPath;
        CreateDirectoryIfPossible(path);
        return File.Open(path, FileMode.Create);

        static void CreateDirectoryIfPossible(string path)
        {
            if (File.Exists(path))
            {
                return;
            }

            if (Path.GetDirectoryName(path) is { } directoryName)
            {
                Directory.CreateDirectory(directoryName);
            }
        }
    }
}