namespace Altemiq.IO.Las.S3.Data;

using TUnit.Core.Interfaces;

public class S3ClientDataClass : IAsyncInitializer, IAsyncDisposable
{
    public Amazon.S3.IAmazonS3 S3Client { get; private set; }
    
    public async Task InitializeAsync()
    {
        const string BucketName = "lidar";
        
        var services = GlobalHooks.App?.Services ?? throw new NullReferenceException();
        this.S3Client = services.GetRequiredAwsService<Amazon.S3.IAmazonS3>();
        if (GlobalHooks.NotificationService != null)
        {
            await GlobalHooks.NotificationService.WaitForResourceAsync("localstack", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
        }
        
        // wait until the bucket is available
        while (!await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(this.S3Client, BucketName))
        {
            await Task.Delay(500);
        }
        
        // ensure we load all the embedded data into the bucket
        var @namespace = typeof(S3ClientDataClass).Namespace;
        foreach (var manifestResourceName in typeof(S3ClientDataClass).Assembly.GetManifestResourceNames())
        {
            // strip off the namespace
            var path = manifestResourceName;
            if (@namespace is not null && manifestResourceName.StartsWith(@namespace))
            {
                path = manifestResourceName[(@namespace.Length + 1)..];
            }
            
            // convert the path into a key
            var keyBuilder = new System.Text.StringBuilder();
            var index = path.LastIndexOf('.');
            for (int i = 0; i < index; i++)
            {
                keyBuilder.Append(path[i] is '.' ? '/' : path[i]);
            }

            keyBuilder.Append(path[index..]);
            var key = keyBuilder.ToString();
            
            // push this to S3
            if (!await FileExistsAsync(this.S3Client, BucketName, key, CancellationToken.None).ConfigureAwait(false))
            {
                await this.S3Client.PutObjectAsync(new()
                {
                    BucketName = BucketName,
                    Key = key,
                    InputStream = typeof(S3ClientDataClass).Assembly.GetManifestResourceStream(manifestResourceName),
                });
            }
        }
        
        static async Task<bool> FileExistsAsync(Amazon.S3.IAmazonS3 client, string bucketName, string key, CancellationToken cancellationToken)
        {
            try
            {
                return await client.GetObjectMetadataAsync(bucketName, key, cancellationToken).ConfigureAwait(false) is { LastModified: not null };
            }
            catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // File does not exist in S3
                return false;
            }
        }
}

    public ValueTask DisposeAsync()
    {
        this.S3Client?.Dispose();
        return default;
    }
}