// -----------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

_ = builder
    .AddAzureStorage("azure")
    .RunAsEmulator()
    .AddBlobs("azure-blobs");

await builder
    .Build()
    .RunAsync()
    .ConfigureAwait(false);