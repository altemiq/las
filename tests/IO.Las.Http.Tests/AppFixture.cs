namespace Altemiq.IO.Las.Http;

using Microsoft.Extensions.DependencyInjection;

public class AppFixture : TUnit.Aspire.AspireFixture<Projects.IO_Las_Http_AppHost>
{
    protected override void ConfigureBuilder(Aspire.Hosting.Testing.IDistributedApplicationTestingBuilder builder)
    {
        _ = builder.Services.ConfigureHttpClientDefaults(static clientBuilder =>
        {
            _ = clientBuilder.AddStandardResilienceHandler();
        });
    }

    public HttpClient CreateWebHttpClient()
    {
        return CreateHttpClient("web-server");
    }
}