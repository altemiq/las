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

_ = builder
    .AddMiniStack("ministack", regionEndPoint: Amazon.RegionEndpoint.APSoutheast2, services: MiniStackServices.SimpleStorageService)
    .WithReference(profiles)
    .EnsureBucket("lidar");

await builder.Build().RunAsync().ConfigureAwait(false);