// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable CA1848, CA1873

using System.Diagnostics;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host
    .CreateApplicationBuilder(args)
    .ConfigureOpenTelemetry();

builder.Services
    .AddSingleton(new ActivitySource(builder.Environment.ApplicationName))
    .AddHostedService<Seeder>();

using var host = builder.Build();

await host.RunAsync().ConfigureAwait(false);

#pragma warning disable IDE0040, MA0047, SA1400, RCS1110

/// <summary>
/// The data seeder.
/// </summary>
/// <param name="activitySource">The activity source.</param>
/// <param name="configuration">The configuration.</param>
/// <param name="appLifetime">The application lifetime.</param>
/// <param name="logger">The logger.</param>
sealed class Seeder(
    ActivitySource activitySource,
    IConfiguration configuration,
    IHostApplicationLifetime appLifetime,
    ILogger<Seeder> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = activitySource.StartActivity("seeding", ActivityKind.Internal);

        if (FindDataDirectory(AppDomain.CurrentDomain.BaseDirectory) is { } rootDirectory)
        {
            var connectionString =
                configuration.GetConnectionString("blobs")
                ?? throw new InvalidOperationException("Missing connection string 'blobs'");

            await SeedAsync(
                new BlobServiceClient(connectionString),
                rootDirectory,
                logger,
                stoppingToken).ConfigureAwait(false);
        }

        appLifetime.StopApplication();

        static string? FindDataDirectory(string path)
        {
            return Path.Join(path, "data") switch
            {
                var desiredPath when Directory.Exists(desiredPath) => desiredPath,
                _ when Path.GetDirectoryName(path) is { } parentPath => FindDataDirectory(parentPath),
                _ => null,
            };
        }

        static async Task SeedAsync(
            BlobServiceClient blobServiceClient,
            string seedFolder,
            ILogger? logger,
            CancellationToken cancellationToken = default)
        {
            const string containerName = "lidar";

            var container =
                blobServiceClient.GetBlobContainerClient(containerName);

            _ = await container
                .CreateIfNotExistsAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (!Directory.Exists(seedFolder))
            {
                logger?.LogWarning("Seed folder not found: {Folder}", seedFolder);
                return;
            }

            foreach (var file in Directory.EnumerateFiles(seedFolder, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path
                    .GetRelativePath(seedFolder, file)
                    .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                var blob = container.GetBlobClient(relativePath);

                var exists = await blob
                    .ExistsAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (exists)
                {
                    logger?.LogDebug("Skipping existing blob: {Blob}", relativePath);
                    continue;
                }

                logger?.LogInformation("Uploading: {Blob}", relativePath);

                var stream = File.OpenRead(file);
                await using (stream.ConfigureAwait(false))
                {
                    _ = await blob.UploadAsync(stream, cancellationToken).ConfigureAwait(false);
                }
            }

            logger?.LogInformation("Storage seed completed");
        }
    }
}