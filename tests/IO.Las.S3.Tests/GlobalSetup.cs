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
    public static async Task SetUp(CancellationToken cancellationToken)
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.IO_Las_AWS_AppHost>(cancellationToken);
        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        builder.Services.AddDefaultAWSOptions(static services =>
        {
            var options = services.GetRequiredService<IConfiguration>().GetAWSOptions();
            var model = services.GetRequiredService<DistributedApplicationModel>();

            // Set credentials
            var profile = model.Resources.OfType<IAWSProfileConfig>().Single().Profiles.Single();
            options.Credentials = new Amazon.Runtime.BasicAWSCredentials(
                GetValue(profile.AccessKeyId.GetValueAsync(CancellationToken.None)),
                GetValue(profile.SecretAccessKey.GetValueAsync(CancellationToken.None)));

            // Set service URL and region
            var localStackServer = model.Resources.OfType<LocalStackServerResource>().Single();
            var endpoint = localStackServer.GetEndpoint(Uri.UriSchemeHttp, KnownNetworkIdentifiers.LocalhostNetwork);
            options.DefaultClientConfig.ServiceURL = endpoint.Url;
            options.DefaultClientConfig.AuthenticationRegion = localStackServer.Region;

            options.DefaultClientConfig.ServiceSpecificSettings.TryAdd(nameof(Amazon.S3.AmazonS3Config.UseAccelerateEndpoint), bool.FalseString);
            options.DefaultClientConfig.ServiceSpecificSettings.TryAdd(nameof(Amazon.S3.AmazonS3Config.ForcePathStyle), bool.TrueString);

            return options;

            static T GetValue<T>(ValueTask<T> task)
            {
                return task.IsCompleted
                    ? task.Result
                    : task.AsTask().GetAwaiter().GetResult();
            }
        });

        builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();

        App = await builder.BuildAsync(cancellationToken);
        await App.StartAsync(cancellationToken);
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