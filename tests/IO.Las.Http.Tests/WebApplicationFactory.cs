namespace Altemiq.IO.Las.Http;

public class WebApplicationFactory : TUnit.Core.Interfaces.IAsyncInitializer, IAsyncDisposable
{
    private const int Port = 25812;

    private static readonly string Url = $"{Uri.UriSchemeHttp}://localhost:{Port}/";

    private readonly HttpClient client = new() { BaseAddress = new(Url) };

    private readonly HttpListener listener = new();

    private readonly CancellationTokenSource cts = new();

    private Task listenerTask;

    public HttpClient CreateClient() => this.client;

    public Task InitializeAsync()
    {
        this.listener.Prefixes.Add($"{Uri.UriSchemeHttp}://*:{Port}/");
        this.listener.Start();

        this.listenerTask = Task.Run(() =>
        {
            var cancellationToken = this.cts.Token;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = this.listener.GetContext();

                    // say that we accept ranges
                    context.Response.Headers.Add("Accept-Ranges", "bytes");

                    if (context.Request.Url is not { } url)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                    else
                    {
                        var urlPath = System.Web.HttpUtility.UrlDecode(url.AbsolutePath.TrimStart('/'));

                        var manifestPath = $"{typeof(WebApplicationFactory).Namespace}.Data.{urlPath.Replace('/', '.')}";
                        if (typeof(WebApplicationFactory).Assembly.GetManifestResourceStream(manifestPath) is { } manifestResourceStream)
                        {
                            using (manifestResourceStream)
                            {
                                if (context.Request.HttpMethod == HttpMethod.Head.Method)
                                {
                                    // just return the length
                                    context.Response.ContentLength64 = manifestResourceStream.Length;
                                    context.Response.ContentType = GetMimeType(urlPath);
                                    context.Response.OutputStream.Close();
                                    continue;
                                }

                                var byteRange = manifestResourceStream.Length;
                                if (context.Request.Headers.GetValues("Range") is { } rangeValues)
                                {
                                    var range = rangeValues[0].Replace("bytes=", "").Split('-');
                                    var startByte = int.Parse(range[0]);
                                    if (string.IsNullOrEmpty(range[1].Trim()) || !long.TryParse(range[1], out var endByte))
                                    {
                                        endByte = manifestResourceStream.Length - 1;
                                    }

                                    byteRange = endByte - startByte + 1;
                                    manifestResourceStream.Seek(startByte, SeekOrigin.Begin);
                                    context.Response.Headers.Add("Content-Range", $"bytes {startByte}-{byteRange - 1}/{byteRange}");
                                }

                                byte[] buffer = new byte[byteRange];
                                var read = manifestResourceStream.Read(buffer, 0, buffer.Length);
                                context.Response.ContentType = GetMimeType(urlPath);
                                context.Response.ContentLength64 = read;
                                context.Response.OutputStream.Write(buffer, 0, read);
                            }
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                    }

                    context.Response.OutputStream.Close();
                }
                catch (Exception)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
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
        });

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await this.cts.CancelAsync();
        this.listener?.Stop();
        await this.listenerTask.WaitAsync(CancellationToken.None);
        this.listenerTask?.Dispose();
        GC.SuppressFinalize(this);
    }
}