# AGENTS.md

## Repository Overview
.NET library for reading/writing ASPRS LAS LiDAR point cloud files, supporting LAS 1.1-1.5, LAZ 1.4 compression and COPC. Modular package structure from .NET Standard 2.0 up to .NET 10, low-allocation stream-centric I/O, AOT-compatible. The `las` CLI is published as a Native AOT executable and an OCI container. Test data lives in `data/` (git-lfs); specification PDFs live in `docs/` (LAS 1.0-1.5, LAZ 1.4 R0, COPC 1.0).

## Essential Commands
All commands run from the repo root. Shell scripts have Unix (`.sh`) and Windows (`.ps1`/`.cmd`) variants.

```bash
# .NET local tools (aspire CLI) - required before build/test
dotnet tool restore

# Restore, build the generator first, then the solution
dotnet restore
dotnet build generators/IO.Las.CodeGeneration --no-restore
dotnet build --no-restore

# Code formatting (whitespace, style, analyzers)
./format.sh

# Run tests with coverage and generate reports
./coverage.sh

# Clean build artifacts (bin, obj, TestResults directories)
./clean.sh

# Check semantic versioning (altemiq.semanticversioning global tool)
./check.version.sh
```

## Development Workflow
- Build and test against multiple LAS versions (1.1-1.5) using the `MaximumLasVersion` property (default 1.5; CI runs a 1.1-1.5 matrix; `Directory.Versions.props` derives the `SupportLas1x` and `LAS1_X(_OR_GREATER)` constants).
- Uses .NET 10 SDK (`global.json`) with `LangVersion preview`; libraries target `net10.0;net9.0;net7.0;net5.0;netstandard2.1;netstandard2.0`.
- Cross-platform publish targets: Windows/Linux x64+arm64, macOS arm64.
- Style is analyzer-enforced (`Altemiq.DotNet.CodingStandard`, `dotnet format`); follow `.editorconfig`.
- AOT-ready tools with Native AOT compilation support.

## Architecture
- Core: `IO.Las` - headers, VLRs, point data records, low-allocation streaming reader/writers.
- Compression: `IO.Las.Compression` (LAZ), `IO.Las.Compression.Arrow`, `IO.Las.Compression.DataFrame`.
- Cloud: `IO.Las.Cloud` (COPC), `IO.Las.S3`, `IO.Las.Azure`, `IO.Las.Http`.
- Data: `IO.Las.Arrow` (Apache Arrow), `IO.Las.DataFrame` (Polars dataframes).
- Geodesy: `IO.Las.Geodesy` (OGC WKT coordinate systems via PROJ).
- Indexing: `IO.Las.Indexing` (quad-trees); Tiling: `IO.Las.Tiling` (tiling VLRs).
- CLI: `tools/Las` (Native AOT + container publishing).
- Source generators: `generators/IO.Las.CodeGeneration` - build this before the rest of the solution.
- Test app hosts: `web/hosts/IO.Las.AWS.AppHost` (MiniStack/S3), `web/hosts/IO.Las.Azure.AppHost` (Azurite).
- Conditional compilation for LAS version support using MSBuild properties.

## Testing
- Uses TUnit test framework on Microsoft.Testing.Platform (runner in `global.json`)
- Run all tests: `dotnet test`
- Run specific test: `dotnet test --project tests/[Project].Tests`
- Cloud tests (`IO.Las.S3.Tests`, `IO.Las.Azure.Tests`) start emulators via `web/hosts/*AppHost` using `Aspire.Hosting.Testing` (S3 -> MiniStack, Azure -> Azurite); `dotnet tool restore` (aspire CLI) is required first.
- Tests run in CI against all LAS versions (1.1-1.5) with coverage.

## Conventions for Writing Code
- Low-allocation: prefer `Span<T>`, `Memory<T>`, `BinaryPrimitives`; SIMD (`Vector256`) is already used in hot paths (e.g. `BoundingBox`, vector math).
- Cross-TFM code: respect the `USE_SPAN_PARSING` / `USE_GENERIC_MATH` conditionals (see `src/Directory.Build.props`); add new APIs via `Meziantou.Polyfill` rather than hand-rolled polyfills.
- `IBasePointDataRecord<TSelf>` is only compiled on net7+; keep sealed concrete types and raw reader implementations as the fast paths.
- Release builds are strong-named (`Las.snk`); keep AOT compatibility (`IsAotCompatible`) - `tools/Las` is `PublishAot`.

## Versioning
- Version prefix = `MaximumLasVersion` + minor (e.g. `1.5.1`); CI appends the suffix: alpha (PR), beta (push to main) - the release workflow promotes to final.
- Breaking changes are checked with `check.version.sh` before releases.
