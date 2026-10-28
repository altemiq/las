# Copilot Instructions: LAS/LAZ .NET Implementation

## Development Commands
- **Tools**: `dotnet tool restore` (installs the `aspire` CLI; see `dotnet-tools.json`)
- **Restore**: `dotnet restore`
- **Build**: `dotnet build generators/IO.Las.CodeGeneration --no-restore` then `dotnet build --no-restore`
- **Test All**: `dotnet test` (TUnit on Microsoft.Testing.Platform)
- **Test Single Project**: `dotnet test --project tests/Path.To.Project.Tests/Path.To.Project.Tests.csproj`
- **Format**: `./format.sh` (Unix) / `format.ps1` (Windows)
- **Clean**: `./clean.sh`
- **Coverage**: `./coverage.sh`
- **Version check**: `./check.version.sh` (altemiq.semanticversioning tool)

## Architecture Overview
This repository is a high-performance .NET implementation for reading, writing, and manipulating LAS/LAZ point cloud files, adhering to ASPRS (LAS 1.1-1.5), LAZ 1.4 and COPC 1.0 specifications (specification PDFs in `docs/`, test data in `data/` via git-lfs).

### Core Components
- `src/IO.Las`: Foundational library for LAS file I/O (binary parsing, header management, point data records, VLRs).
- `src/IO.Las.Compression`: LAS compression (LAZ) and algorithms; `Compression.Arrow` / `Compression.DataFrame` add the compressed variants.
- `src/IO.Las.Indexing`: Spatial indexing (quad-trees) for querying and sorting.
- `src/IO.Las.Geodesy`: Coordinate transformations and geodetic calculations (OGC WKT/PROJ).
- `src/IO.Las.Arrow` & `src/IO.Las.DataFrame`: Apache Arrow and Polars data-frame integration.
- `src/IO.Las.Cloud` (COPC), `src/IO.Las.S3`, `src/IO.Las.Azure`, `src/IO.Las.Http`: object-storage and HTTP(S) support.
- `src/IO.Las.Tiling`: Reads tiling VLRs.
- `tools/Las`: CLI for LAS operations (Native AOT, containerized).
- `generators/IO.Las.CodeGeneration`: Custom source generators to reduce boilerplate (build before the solution).
- `web/hosts/*AppHost`: Aspire local app hosts that start cloud emulators (MiniStack/S3, Azurite) used by the cloud tests.

## Key Conventions & Performance
- **Hardware Intrinsics**: Use `System.Runtime.Intrinsics` (SIMD, e.g. `Vector256`) for vector operations (`Vector2D`, `Vector3D`) and point quantization.
- **Memory Management**: Prioritize `Span<T>`, `ReadOnlySpan<byte>`, and `Memory<T>` to minimize allocations and copying during binary parsing.
- **Point Abstractions**: Point data records implement the generic `IBasePointDataRecord<TSelf>` interface (compiled on net7+ only), with sealed concrete types and raw reader implementations as the fast paths for performance.
- **Version Support**: Supports LAS versions 1.1 through 1.5 via the `MaximumLasVersion` MSBuild property and `LAS1_X(_OR_GREATER)` conditional compilation constants (see `Directory.Versions.props`).
- **Cross-TFM**: Respect the `USE_SPAN_PARSING` / `USE_GENERIC_MATH` conditionals in `src/Directory.Build.props`; add new APIs through `Meziantou.Polyfill`, never hand-written polyfills.
- **Analytics**: Coding standards are enforced by `Altemiq.DotNet.CodingStandard` analyzers and `.editorconfig`; run `./format.sh` before committing.
- **Tests**: TUnit; cloud tests require the Aspire emulators from `web/hosts/` - run `dotnet tool restore` before `dotnet test`.
