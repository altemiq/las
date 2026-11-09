namespace Altemiq.IO.Las.S3;

using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class AppFixture : TUnit.Aspire.AspireFixture<Projects.IO_Las_AWS_AppHost>
{
    private readonly Lazy<Amazon.S3.IAmazonS3> client;

    public AppFixture()
    {
        client = new(() => App.Services.GetRequiredService<Amazon.S3.IAmazonS3>(), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Amazon.S3.IAmazonS3 CreateAmazonS3Client()
    {
        return client.Value;
    }

    protected override void ConfigureBuilder(Aspire.Hosting.Testing.IDistributedApplicationTestingBuilder builder)
    {
        _ = builder.Services.ConfigureHttpClientDefaults(static clientBuilder =>
        {
            _ = clientBuilder.AddStandardResilienceHandler();
        });

        _ = builder.Services.AddDefaultAWSOptions(static services =>
        {
            var options = services.GetRequiredService<IConfiguration>().GetAWSOptions();
            var model = services.GetRequiredService<DistributedApplicationModel>();

            // Set credentials
            var profile = model.Resources.OfType<Aspire.Hosting.AWS.IAWSProfileConfig>().Single().Profiles.Single();
            options.Credentials = new Amazon.Runtime.BasicAWSCredentials(
                GetValue(profile.AccessKeyId.GetValueAsync(CancellationToken.None)),
                GetValue(profile.SecretAccessKey.GetValueAsync(CancellationToken.None)));

            // Set service URL and region
            var ministackServer = model.Resources.OfType<MiniStackServerResource>().Single();

            var endpoint = ministackServer.GetEndpoint(Uri.UriSchemeHttp, KnownNetworkIdentifiers.LocalhostNetwork);
            options.DefaultClientConfig.ServiceURL = endpoint.Url;
            options.DefaultClientConfig.AuthenticationRegion = ministackServer.Region;

            _ = options.DefaultClientConfig.ServiceSpecificSettings.TryAdd(nameof(Amazon.S3.AmazonS3Config.UseAccelerateEndpoint), bool.FalseString);
            _ = options.DefaultClientConfig.ServiceSpecificSettings.TryAdd(nameof(Amazon.S3.AmazonS3Config.ForcePathStyle), bool.TrueString);

            return options;

            static T GetValue<T>(ValueTask<T> task)
            {
                return task.IsCompleted
                    ? task.Result
                    : task.AsTask().GetAwaiter().GetResult();
            }
        });

        _ = builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();
    }
}