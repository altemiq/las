using Aspire.Hosting.Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace Altemiq.IO.Las.Azure;

using Microsoft.Extensions.Azure;


public class GlobalHooks
{
    public static DistributedApplication App { get; private set; }

    public static ResourceNotificationService NotificationService => App.ResourceNotifications;

    [Before(TestSession)]
    public static async Task SetUp(CancellationToken cancellationToken)
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.IO_Las_Azure_AppHost>(cancellationToken);
        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        string connectionString = default;
        builder.Services.AddAzureClients(azureBuilder =>
        {
            azureBuilder.AddClient<BlobServiceClient, BlobClientOptions>(options =>
            {
                while (connectionString is null)
                {
                    Thread.Sleep(100);
                }
                
                return new(connectionString, options);
            });
        });
        
        // get the blob storage
        if (builder.Resources.OfType<AzureBlobStorageResource>().SingleOrDefault() is { } blobStorage)
        {
            builder.Eventing.Subscribe<ResourceReadyEvent>(blobStorage, AddConnectionString);
        }
        else if (builder.Resources.OfType<AzureBlobStorageContainerResource>().SingleOrDefault() is { } blobStorageContainer)
        {
            builder.Eventing.Subscribe<ResourceReadyEvent>(blobStorageContainer, AddConnectionString);
        } 

        App = await builder.BuildAsync(cancellationToken);
        
        await App.StartAsync(cancellationToken);

        async Task AddConnectionString(ResourceReadyEvent @event, CancellationToken token)
        {
            if (@event.Resource is IResourceWithConnectionString connectionStringResource)
            {
                connectionString = await connectionStringResource.ConnectionStringExpression.GetValueAsync(token);
            }
        }
    }

    [After(TestSession)]
    public static async Task CleanUp(CancellationToken cancellationToken)
    {
        if (App is { } app)
        {
            await app.StopAsync(cancellationToken);
            await app.DisposeAsync();
        }
    }
}