// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var app = WebApplication
    .CreateBuilder(args)
    .AddServiceDefaults()
    .Build();

if (FindDataDirectory(app.Environment.ContentRootPath) is { } rootDirectory)
{
    _ = app.UseFileServer(new FileServerOptions
    {
        EnableDefaultFiles = true,
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(rootDirectory),
        StaticFileOptions =
        {
            ServeUnknownFileTypes = true,
            DefaultContentType = System.Net.Mime.MediaTypeNames.Application.Octet,
        },
    });
}

await app.RunAsync().ConfigureAwait(false);

static string? FindDataDirectory(string path)
{
    return Path.Join(path, "data") switch
    {
        var desiredPath when Directory.Exists(desiredPath) => desiredPath,
        _ when Path.GetDirectoryName(path) is { } parentPath => FindDataDirectory(parentPath),
        _ => null,
    };
}