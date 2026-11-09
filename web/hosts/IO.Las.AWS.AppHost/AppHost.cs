// -----------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var profiles = builder
    .AddAWSProfileConfig("aws")
    .WithProfile(
        "LAS",
        builder.AddParameter("access-key-id", new GenerateParameterDefault { MinLength = 12, Lower = false, Upper = false, Special = false }));

var ministack = builder
    .AddMiniStack("ministack", regionEndPoint: Amazon.RegionEndpoint.APSoutheast2, services: MiniStackServices.SimpleStorageService)
    .WithReference(profiles)
    .EnsureBucket("lidar");

if (FindDataDirectory(builder.AppHostDirectory) is { } rootDirectory)
{
    _ = ministack.WithMirror(rootDirectory, "lidar", isReadOnly: true);
}

await builder.Build().RunAsync().ConfigureAwait(false);

static string? FindDataDirectory(string path)
{
    return Path.Join(path, "data") switch
    {
        var desiredPath when Directory.Exists(desiredPath) => desiredPath,
        _ when Path.GetDirectoryName(path) is { } parentPath => FindDataDirectory(parentPath),
        _ => null,
    };
}