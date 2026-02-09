using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("azure")
    .RunAsEmulator();

_ = storage.AddBlobs("azure-blobs");

await builder
    .Build()
    .RunAsync()
    .ConfigureAwait(false);