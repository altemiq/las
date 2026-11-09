// -----------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

_ = builder
    .AddProject<Projects.IO_Las_Http_Web>("web-server");

await builder
    .Build()
    .RunAsync()
    .ConfigureAwait(false);