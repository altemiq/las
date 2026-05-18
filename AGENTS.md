# AGENTS.md

## Repository Overview
.NET library for reading/writing ASPRS LAS LiDAR point cloud files with modular package structure supporting LAS versions 1.1-1.5.

## Essential Commands
```bash
# Code formatting (whitespace, style, analyzers)
./format.sh

# Run tests with coverage and generate reports
./coverage.sh

# Clean build artifacts (bin, obj, TestResults directories)
./clean.sh

# Check semantic versioning
./check.version.sh
```

## Development Workflow
- Build and test against multiple LAS versions (1.1-1.5) using `MaximumLasVersion` property
- Uses .NET 10 SDK with latest language features
- Cross-platform support (Windows/Linux/macOS x64/arm64)
- AOT-ready tools with Native AOT compilation support

## Architecture
- Modular design with separate packages:
  - Core: IO.Las
  - Extensions: Arrow, Azure, Cloud, Compression, DataFrame, Geodesy, Http, Indexing, S3, Tiling
- Conditional compilation for LAS version support using MSBuild properties
- Code generation via IO.Las.CodeGeneration project
- Command-line tool (las) with container support

## Testing
- Uses Microsoft.Testing.Platform with TUnit test framework
- Run specific test: `dotnet test --project tests/[Project].Tests`
- Run all tests: `dotnet test`
- Tests run in CI against all LAS versions (1.1-1.5)

## Build Process
- Restore: `dotnet restore`
- Build: `dotnet build`
- Pack (NuGet): `dotnet pack`
- Publish AOT tool: `dotnet publish tools/Las --configuration Release`

## Versioning
- Uses semantic versioning with version determined by MaximumLasVersion property
- Version suffixes: alpha (PR), beta (push to main), final (release)