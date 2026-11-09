namespace Altemiq.IO.Las.Azure;

using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

public class AppFixture : TUnit.Aspire.AspireFixture<Projects.IO_Las_Azure_AppHost>
{
    private string connectionString;

    private readonly Lazy<global::Azure.Storage.Blobs.BlobContainerClient> client;

    public AppFixture()
    {
        client = new(
            () =>
            {
                var servicesClient = new global::Azure.Storage.Blobs.BlobServiceClient(connectionString);
                return servicesClient.GetBlobContainerClient("lidar");
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public global::Azure.Storage.Blobs.BlobContainerClient CreateContainerClient()
    {
        return client.Value;
    }

    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        _ = builder.Services.ConfigureHttpClientDefaults(static clientBuilder =>
        {
            _ = clientBuilder.AddStandardResilienceHandler();
        });
    }

    protected override async Task WaitForResourcesAsync(DistributedApplication app, CancellationToken cancellationToken)
    {
        await base.WaitForResourcesAsync(app, cancellationToken);

        var notifications = app.Services.GetRequiredService<Aspire.Hosting.ApplicationModel.ResourceNotificationService>();

        // wait for the seeder to be complete
        await notifications
            .WaitForResourceAsync(
             "seeder",
                Aspire.Hosting.ApplicationModel.KnownResourceStates.Finished,
                cancellationToken)
            .ConfigureAwait(false);

        // get the connection string
        connectionString = await app.GetConnectionStringAsync("blobs", cancellationToken);
    }
}