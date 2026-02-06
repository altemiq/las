[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace Altemiq.IO.Las.S3;

using Aspire.Hosting.AWS;
using Microsoft.Extensions.Configuration;

public class GlobalHooks
{
    public static DistributedApplication App { get; private set; }

    public static ResourceNotificationService NotificationService => App.ResourceNotifications;

    [Before(TestSession)]
    public static async Task SetUp()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.IO_Las_S3_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        
        appHost.Services.AddDefaultAWSOptions(static services =>
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var config = configuration.GetAWSOptions();
            var model = services.GetRequiredService<DistributedApplicationModel>();

            if (model.Resources.OfType<IAWSProfileConfig>().FirstOrDefault() is { } profileConfig)
            {
                var profile = profileConfig.Profiles.Single();

                var accessKeyId = profile.AccessKeyId.GetValueAsync(CancellationToken.None).Result;
                var secretAccessKey = profile.SecretAccessKey.GetValueAsync(CancellationToken.None).Result;

                config.Credentials = new Amazon.Runtime.BasicAWSCredentials(
                    accessKeyId,
                    secretAccessKey);
            }

            if (model.Resources.OfType<LocalStackServerResource>().FirstOrDefault() is { } localStackServer)
            {
                var endpoint = localStackServer.GetEndpoint(Uri.UriSchemeHttp, KnownNetworkIdentifiers.LocalhostNetwork);
                config.DefaultClientConfig.ServiceURL = endpoint.Url;
                config.DefaultClientConfig.AuthenticationRegion = localStackServer.Region;
            }
            
            config.DefaultClientConfig.ServiceSpecificSettings.TryAdd(nameof(Amazon.S3.AmazonS3Config.UseAccelerateEndpoint), bool.FalseString);
            config.DefaultClientConfig.ServiceSpecificSettings.TryAdd(nameof(Amazon.S3.AmazonS3Config.ForcePathStyle), bool.TrueString);

            return config;
        });

        appHost.Services.AddAWSService<Amazon.S3.IAmazonS3>();

        App = await appHost.BuildAsync();
        await App.StartAsync();
    }

    [After(TestSession)]
    public static void CleanUp()
    {
        Console.WriteLine("...and after!");
    }
}
