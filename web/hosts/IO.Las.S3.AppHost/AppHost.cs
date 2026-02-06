// -----------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var region = Amazon.RegionEndpoint.APSoutheast2;

var profiles = builder
    .AddAWSProfileConfig("aws")
    .WithProfile("LAS");

_ = builder
    .AddLocalStack("localstack", regionEndPoint: region)
    .WithReference(profiles)
    .EnsureBucket("lidar");

await builder.Build().RunAsync().ConfigureAwait(false);