namespace Altemiq.IO.Las.Azure.Data;

using TUnit.Core.Interfaces;

public class BlobContainerClientDataClass : IAsyncInitializer, IAsyncDisposable
{
    public global::Azure.Storage.Blobs.BlobContainerClient BlobContainerClient { get; private set; }

    public async Task InitializeAsync()
    {
        const string ContainerName = "lidar";

        var services = GlobalHooks.App?.Services ?? throw new NullReferenceException();
        if (GlobalHooks.NotificationService != null)
        {
            await GlobalHooks.NotificationService.WaitForResourceAsync("azure-blobs", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
        }
        
        // get the client
        var servicesClient = services.GetRequiredService<global::Azure.Storage.Blobs.BlobServiceClient>();
            
        this.BlobContainerClient = servicesClient.GetBlobContainerClient(ContainerName);
        await this.BlobContainerClient.CreateIfNotExistsAsync();

        // ensure we load all the embedded data into the bucket
        var @namespace = typeof(BlobContainerClientDataClass).Namespace;
        foreach (var manifestResourceName in typeof(BlobContainerClientDataClass).Assembly.GetManifestResourceNames())
        {
            // strip off the namespace
            var path = manifestResourceName;
            if (@namespace is not null && manifestResourceName.StartsWith(@namespace))
            {
                path = manifestResourceName[(@namespace.Length + 1)..];
            }

            // convert the path into a name
            var blobNameBuilder = new System.Text.StringBuilder();
            var index = path.LastIndexOf('.');
            for (int i = 0; i < index; i++)
            {
                blobNameBuilder.Append(path[i] is '.' ? '/' : path[i]);
            }

            blobNameBuilder.Append(path[index..]);
            var blobName = blobNameBuilder.ToString();

            // push this to blob storage
            var blobClient = this.BlobContainerClient.GetBlobClient(blobName);
            if (await blobClient.ExistsAsync())
            {
                continue;
            }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            await
#endif
                using var stream = typeof(BlobContainerClientDataClass).Assembly.GetManifestResourceStream(manifestResourceName);
            await blobClient.UploadAsync(stream);
        }
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return default;
    }
}