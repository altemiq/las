# IO.Las.Compression Benchmarks

[BenchmarkDotNet](https://benchmarkdotnet.org/) harness covering the LAZ
(de)compression hot paths.

## Run everything (local sources only)

```bash
dotnet run -c Release --project benchmarks/IO.Las.Compression.Benchmarks -- --filter '*'
```

This builds against the in-tree `src/IO.Las.Compression` project and produces
a single "local" job per benchmark.

## Compare local sources against a published NuGet baseline

Set the `LAS_BASELINE_VERSIONS` environment variable to one (or a
semicolon-separated list of) published `Altemiq.IO.Las.Compression` versions
on [GitHub Packages](https://github.com/altemiq/las/pkgs/nuget/Altemiq.IO.Las.Compression).
Each value produces an additional BenchmarkDotNet job that restores and builds
against that exact published package, so you see local and published numbers
in the same summary table.

```bash
# compare local against the latest published release
LAS_BASELINE_VERSIONS=1.5.0-beta.89 \
    dotnet run -c Release --project benchmarks/IO.Las.Compression.Benchmarks -- \
        --filter '*'

# compare against multiple baselines
LAS_BASELINE_VERSIONS="1.5.0-beta.89;1.5.0-beta.86;1.5.0-beta.75" \
    dotnet run -c Release --project benchmarks/IO.Las.Compression.Benchmarks -- \
        --filter '*'

# compare only a specific benchmark against baseline
LAS_BASELINE_VERSIONS=1.5.0-beta.89 \
    dotnet run -c Release --project benchmarks/IO.Las.Compression.Benchmarks -- \
        --filter '*DecodeColouredParsed*'
```

The resulting summary has one row per (benchmark × job) pair, so per-version
deltas are directly visible:

```
| Method                  | Job                 | Mean      | Allocated |
|------------------------ |-------------------- |----------:|----------:|
| DecodeColouredParsed    | local               |  32.36 ms |   2.82 MB |
| DecodeColouredParsed    | nuget-1.5.0-beta.89 |  36.35 ms |   2.26 MB |
| DecodeFusaParsed        | local               | 134.73 ms |  13.89 MB |
| DecodeFusaParsed        | nuget-1.5.0-beta.89 | 148.69 ms |  13.59 MB |
| DecodeFusaHeight7Parsed | local               | 175.35 ms |  17.31 MB |
| DecodeFusaHeight7Parsed | nuget-1.5.0-beta.89 | 186.02 ms |  17.01 MB |
| EncodeGpsPoints         | local               |  21.86 ms |   6.16 MB |
| EncodeGpsPoints         | nuget-1.5.0-beta.89 |  24.11 ms |   6.16 MB |
```

(Numbers above captured on Ubuntu 26.04 / .NET 10.0.7 / 12th Gen Intel
Core i7-1270P. Your machine will produce different absolute times.)

### Requirements

- **BenchmarkDotNet's default CsProj toolchain.** The NuGet swap relies on
  `Job.WithMsBuildArguments("/p:LasCompressionVersion=...")`, which
  BenchmarkDotNet's in-process toolchain cannot honor (it cannot re-restore
  a different package into an already-loaded assembly). The default toolchain
  recompiles each job in its own out-of-process csproj; this is slower than
  the in-process toolchain but is the documented way to compare package
  versions. See
  [BenchmarkDotNet sample: IntroNuGet](https://benchmarkdotnet.org/articles/samples/IntroNuGet.html)
  and [discussion #2990](https://github.com/dotnet/BenchmarkDotNet/discussions/2990).

### How the swap is wired

`benchmarks/IO.Las.Compression.Benchmarks/IO.Las.Compression.Benchmarks.csproj`
has two conditional `<ItemGroup>`s keyed on the `LasCompressionVersion`
property:

```xml
<!-- Local source (default): reference the in-tree Compression project. -->
<ItemGroup Condition=" '$(LasCompressionVersion)' == '' ">
  <ProjectReference Include="..\..\src\IO.Las.Compression\IO.Las.Compression.csproj" />
</ItemGroup>

<!-- Published baseline: reference the package at the requested version. -->
<ItemGroup Condition=" '$(LasCompressionVersion)' != '' ">
  <PackageReference Include="Altemiq.IO.Las.Compression"
                    VersionOverride="[$(LasCompressionVersion)]" />
</ItemGroup>
```

`Program.cs` turns a semicolon-separated `LAS_BASELINE_VERSIONS` env var into
one BDN `Job` per entry, each passing
`"/p:LasCompressionVersion=<version>"` via `WithMsBuildArguments`.

`VersionOverride` is used (rather than plain `Version`) because the parent
`benchmarks/Directory.Packages.props` enables central package management for
this repository; `VersionOverride` is the CPM-blessed way of pinning a
version on a per-project basis without having to declare a central
`<PackageVersion>` entry. An exact range (`[1.5.0-beta.89]` rather than
`1.5.0-beta.89`) is used so NuGet never silently resolves to an adjacent
cached version.

## Run a subset of benchmarks

```bash
# Only decode benchmarks
dotnet run -c Release --project benchmarks/IO.Las.Compression.Benchmarks -- \
    --filter '*LazDecodeBenchmarks*'

# Only fusa tests
dotnet run -c Release --project benchmarks/IO.Las.Compression.Benchmarks -- \
    --filter '*Fusa*'

# Only the encoder
dotnet run -c Release --project benchmarks/IO.Las.Compression.Benchmarks -- \
    --filter '*Encode*'
```

## What each benchmark does

| Benchmark | File | Point format | Path exercised |
|---|---|---|---|
| `DecodeFusaParsed` | `data/laz/fusa.laz` (~277k points) | `GpsPointDataRecord` (format 1) | `ReadPointDataRecord()` returning `LasPointSpan` |
| `DecodeColouredParsed` | `tests/.../coloured.laz` | `GpsColorPointDataRecord` (format 3) | parsed; hits `ColorReader2` |
| `DecodeFusaHeight7Parsed` | `tests/.../fusa_height_7.laz` | `ExtendedGpsColorPointDataRecord` (format 7) + extra bytes | parsed; hits the layered/extended v1.4 reader |
| `EncodeGpsPoints` | synthetic (50k points) | `GpsPointDataRecord` | `LazWriter.Write(GpsPointDataRecord)` × 50k |

The benchmark surface deliberately sticks to APIs that exist across all
published `Altemiq.IO.Las.Compression` versions on the feed (no span-
destination decode, no `LasPointSpan` deconstruction) so the same benchmark
code compiles against both the local tree and the historical baselines.
`MemoryDiagnoser` reports allocations per iteration, so allocation
regressions are visible directly in the output.

## Why the fixtures are copied

The `.laz` files live outside the benchmark project (in `data/laz/` and in
the test projects). The `.csproj` includes them as
`<None ... CopyToOutputDirectory="PreserveNewest" />`, so the benchmark just
does
`File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "data", "fusa.laz"))`
at `[GlobalSetup]` time. Each iteration wraps a fresh `MemoryStream` over the
same byte array, which isolates decompression throughput from disk I/O and
keeps timing deterministic.

## Listing available baseline versions

```bash
dotnet package search Altemiq.IO.Las.Compression \
    --source github \
    --prerelease
```

Or browse:
<https://github.com/altemiq/las/pkgs/nuget/Altemiq.IO.Las.Compression>.
