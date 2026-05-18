// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

// Benchmarks comparing the current source tree against a published
// Altemiq.IO.Las.Compression NuGet package from GitHub Packages.
//
// Usage:
//   # default: just run local (equivalent to older in-process setup)
//   dotnet run -c Release --project benchmarks/IO.Las.Compression.Benchmarks -- --filter '*'
//
//   # compare local against a specific published baseline
//   dotnet run -c Release --project benchmarks/IO.Las.Compression.Benchmarks -- \
//       --filter '*' --envVars LAS_BASELINE_VERSIONS=1.5.0-beta.89
//
//   # compare against multiple baselines
//   dotnet run -c Release --project benchmarks/IO.Las.Compression.Benchmarks -- \
//       --filter '*' --envVars LAS_BASELINE_VERSIONS=1.5.0-beta.89;1.5.0-beta.86
//
// Pattern inspired by https://benchmarkdotnet.org/articles/samples/IntroNuGet.html
// and https://github.com/dotnet/BenchmarkDotNet/discussions/2990
var config = BuildConfig();

_ = BenchmarkSwitcher
    .FromAssembly(typeof(Altemiq.IO.Las.Compression.LazDecodeBenchmarks).Assembly)
    .Run(args, config);

static IConfig BuildConfig()
{
    var config = ManualConfig.CreateMinimumViable()
        .AddDiagnoser(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);

    // Always include a "local" job that builds against the in-tree ProjectReference.
    config.AddJob(Job.Default
        .WithMsBuildArguments("/p:LasCompressionVersion=")
        .WithId("local"));

    // Optionally add one job per baseline version specified via the
    // LAS_BASELINE_VERSIONS env var (semicolon-separated).
    var baselines = Environment.GetEnvironmentVariable("LAS_BASELINE_VERSIONS");
    if (string.IsNullOrWhiteSpace(baselines))
    {
        return config;
    }

    foreach (var version in baselines.Split([';', ',', ' '], StringSplitOptions.RemoveEmptyEntries))
    {
        config.AddJob(Job.Default
            .WithMsBuildArguments($"/p:LasCompressionVersion={version}")
            .WithId($"nuget-{version}"));
    }

    return config;
}