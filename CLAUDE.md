# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

- Build: `dotnet build`
- Test all: `dotnet test`
- Test single project: `dotnet test tests/Path.To.Project.Tests/Path.To.Project.Tests.csproj`
- Format: `./format.sh`
- Clean: `./clean.sh`
- Coverage: `./coverage.sh`

## Architecture Overview

The repository is a high-performance .NET implementation for reading, writing, and manipulating LAS/LAZ point cloud files, adhering to ASPRS and COPC specifications.

### Core Components

- `src/IO.Las`: The foundational library for LAS file I/O, including binary parsing, header management, and point data record definitions.
- `src/IO.Las.Compression`: Implements LAS compression (LAZ) and related compression algorithms.
- `src/IO.Las.Indexing`: Provides spatial indexing (e.g., QuadTrees) for efficient point cloud querying and sorting.
- `src/IO.Las.Geodesy`: Handles coordinate transformations and geodetic calculations.
- `src/IO.Las.DataFrame` & `src/IO.Las.Arrow`: Integration with Apache Arrow and data frame structures for analytical workloads.
- `src/IO.Las.Cloud`, `src/IO.Las.S3`, `src/IO.Las.Azure`: Cloud-native extensions for accessing LAS/COPC data stored in object storage.
- `tools/Las`: A command-line interface (CLI) for performing LAS operations.
- `generators/IO.Las.CodeGeneration`: Custom source generators used to reduce boilerplate in the core library.

### Performance & Implementation Details

- **Hardware Intrinsics**: The codebase makes extensive use of `System.Runtime.Intrinsics` (SIMD) for vector operations (`Vector2D`, `Vector3D`) and point quantization to maximize throughput.
- **Memory Management**: Uses `Span<T>`, `ReadOnlySpan<byte>`, and `Memory<T>` extensively to minimize allocations and avoid unnecessary copying during binary parsing.
- **Abstraction**: Point data is handled via interfaces (`IBasePointDataRecord`) but often optimized through concrete types and raw reader implementations for performance.
- **Version Support**: Supports multiple LAS versions (1.0 through 1.5) via conditional compilation and runtime checks.
