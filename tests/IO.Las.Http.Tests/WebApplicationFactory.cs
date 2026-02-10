namespace Altemiq.IO.Las.Http;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class WebApplicationFactory : TUnit.Core.Interfaces.IAsyncInitializer, IAsyncDisposable
{
    private static readonly int Port = GetPort();

    private static readonly string Url = $"{Uri.UriSchemeHttp}://localhost:{Port}/";
    
    private static readonly ILoggerFactory LoggerFactory = new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory();

    private readonly HttpClient client = new() { BaseAddress = new(Url) };

    private readonly Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServer server = new(GetKestrelServerOptions(), GetSocketTransportFactory(LoggerFactory), LoggerFactory);

    private readonly CancellationTokenSource cts = new();

    private bool disposed;

    public HttpClient CreateClient() => this.client;

    public Task InitializeAsync()
    {
        return this.server.StartAsync(new SimpleHttpApplication(), this.cts.Token);
    }

    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        await this.server.StopAsync(CancellationToken.None).ConfigureAwait(false);
        
#if NET8_0_OR_GREATER
        await this.cts.CancelAsync();
#else
        this.cts.Cancel();
#endif

        GC.SuppressFinalize(this);
    }

    private static int GetPort()
    {
        var random = new Random((int)DateTime.UtcNow.Ticks);
        return random.Next(25000, 30000);
    }

    private static Microsoft.Extensions.Options.OptionsWrapper<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions> GetKestrelServerOptions()
    {
        var kestrelServerOptions = new Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions
        {
            Limits =
            {
                MaxRequestBodySize = int.MaxValue,
                MaxConcurrentConnections = 20,
            },
        };

        kestrelServerOptions.ListenAnyIP(Port);

        return new(kestrelServerOptions);
    }

    private static Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketTransportFactory GetSocketTransportFactory(ILoggerFactory loggerFactory)
    {
        var transportOptions = new Microsoft.Extensions.Options.OptionsWrapper<Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketTransportOptions>(new());
        var applicationLifetime = new Microsoft.AspNetCore.Hosting.Internal.ApplicationLifetime(loggerFactory.CreateLogger<Microsoft.AspNetCore.Hosting.Internal.ApplicationLifetime>());
        
        return new(transportOptions, applicationLifetime, loggerFactory);
    }

    private class SimpleHttpApplication : Microsoft.AspNetCore.Hosting.Server.IHttpApplication<HttpContext>
    {
        public HttpContext CreateContext(Microsoft.AspNetCore.Http.Features.IFeatureCollection contextFeatures)
        {
            return new DefaultHttpContext(contextFeatures);
        }

        public async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.Headers.Append("Accept-Ranges", "bytes");

            if (context.Request.Path is { HasValue: false })
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                var urlPath = context.Request.Path.Value!.TrimStart('/');

                var manifestPath = $"{typeof(WebApplicationFactory).Namespace}.Data.{urlPath.Replace('/', '.')}";
                if (typeof(WebApplicationFactory).Assembly.GetManifestResourceStream(manifestPath) is { } manifestResourceStream)
                {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
                    await
#endif
                    using (manifestResourceStream)
                    {

                        if (context.Request.Method == HttpMethod.Head.Method)
                        {
                            // just return the length
                            context.Response.ContentLength = manifestResourceStream.Length;
                            context.Response.ContentType = GetMimeType(urlPath);
                            return;
                        }

                        var byteRange = manifestResourceStream.Length;
                        if (context.Request.Headers.TryGetValue("Range", out var rangeValues)
                            && rangeValues is [{} rangeValue, ..])
                        {
                            var range = rangeValue.Replace("bytes=", "").Split('-');
                            var startByte = int.Parse(range[0]);
                            if (string.IsNullOrEmpty(range[1].Trim()) || !long.TryParse(range[1], out var endByte))
                            {
                                endByte = manifestResourceStream.Length - 1;
                            }

                            byteRange = endByte - startByte + 1;
                            manifestResourceStream.Seek(startByte, SeekOrigin.Begin);
                            context.Response.Headers.Append("Content-Range", $"bytes {startByte}-{byteRange - 1}/{byteRange}");
                        }

                        byte[] buffer = new byte[byteRange];
                        var read = await manifestResourceStream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                        context.Response.ContentType = GetMimeType(urlPath);
                        context.Response.ContentLength = read;
                        await context.Response.Body.WriteAsync(buffer.AsMemory(0, read));
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }

            static string GetMimeType(string filePath)
            {
                return Path.GetExtension(filePath).ToLower() switch
                {
                    ".html" => "text/html",
                    ".htm" => "text/html",
                    ".css" => "text/css",
                    ".js" => "application/javascript",
                    ".png" => "image/png",
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".svg" => "image/svg+xml",
                    ".las" or ".laz" => "application/vnd.las",
                    _ => "application/octet-stream",
                };
            }
        }

        public void DisposeContext(HttpContext context, Exception exception)
        {
        }
    }
}