# Copilot Instructions: LAS/LAZ .NET Implementation

## Development Commands
- **Build**: `dotnet build`
- **Test All**: `dotnet test`
- **Test Single Project**: `dotnet test tests/Path.To.Project.Tests/Path.To.Project.Tests.csproj`
- **Format**: `./format.sh`
- **Clean**: `./clean.sh`
- **Coverage**: `./coverage.sh`

## Architecture Overview
This repository is a high-performance .NET implementation for reading, writing, and manipulating LAS/LAZ point cloud files, adhering to ASPRS and COPC specifications.

### Core Components
- `src/IO.Las`: Foundational library for LAS file I/O (binary parsing, header management, point data records).
- `src/IO.Las.Compression`: LAS compression (LAZ) and algorithms.
- `src/IO.Las.Indexing`: Spatial indexing (e.g., QuadTrees) for querying and sorting.
- `src/IO.Las.Geodesy`: Coordinate transformations and geodetic calculations.
- `src/IO.Las.DataFrame` & `src/IO.Las.Arrow`: Integration with Apache Arrow and data frames.
- `src/IO.Las.Cloud`, `src/IO.Las.S3`, `src/IO.Las.Azure`: Cloud-native extensions for object storage.
- `tools/Las`: CLI for LAS operations.
- `generators/IO.Las.CodeGeneration`: Custom source generators to reduce boilerplate.

## Key Conventions & Performance
- **Hardware Intrinsics**: Use `System.Runtime.Intrinsics` (SIMD) for vector operations (`Vector2D`, `Vector3D`) and point quantization.
- **Memory Management**: Prioritize `Span<T>`, `ReadOnlySpan<byte>`, and `Memory<T>` to minimize allocations and copying during binary parsing.
- **Point Abstractions**: Point data uses `IBasePointDataRecord` interfaces, but is often optimized via concrete types and raw reader implementations for performance.
- **Version Support**: Supports LAS versions 1.0 through 1.5 via conditional compilation and runtime checks.
