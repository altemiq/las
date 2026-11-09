// -----------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var azure = builder
    .AddAzureStorage("azure")
    .RunAsEmulator()
    .AddBlobs("blobs");

_ = builder
    .AddProject<Projects.IO_Las_Azure_Seeder>("seeder")
    .WithReference(azure)
    .WaitFor(azure);

await builder
    .Build()
    .RunAsync()
    .ConfigureAwait(false);