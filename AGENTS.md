# AGENTS.md

High-value context for agents working in this repo. See also `CLAUDE.md` and `.github/copilot-instructions.md` (mostly overlapping summaries).

## Toolchain

- Requires .NET SDK 10.0.100 (rollForward: latestMinor) per `global.json`. CI also installs 8.0.x and 9.0.x because `src/IO.Las` multi-targets `net10.0;net9.0;net7.0;net5.0;netstandard2.1;netstandard2.0` and tests target `net10.0;net9.0;net8.0;net472`.
- Test runner is `Microsoft.Testing.Platform` (set in `global.json`), not VSTest. Test framework is **TUnit** (added globally via `tests/Directory.Build.targets`). `dotnet test` invokes `testhost` directly; do not pass VSTest-style args.
- Solution file is `Las.slnx` (XML solution format), not `.sln`.
- NuGet sources: nuget.org + `https://nuget.pkg.github.com/altemiq/index.json` (required for `Altemiq.IO.Las*` packages). `dotnet restore` will fail without GitHub auth if those packages aren't cached.

## Build order (matters)

The core library consumes an internal source generator. CI and local builds do:

```
dotnet build generators/IO.Las.CodeGeneration --no-restore
dotnet build --no-restore
```

Building the whole solution cold without pre-building the generator can leave analyzers/source-generators in an inconsistent state. When in doubt, build `generators/IO.Las.CodeGeneration` first.

The generator also downloads `platforms.csv` and `models.csv` from `github.com/ASPRSorg/LAS/wiki/attachments/` into the intermediate output on first build of `src/IO.Las` (see `_DownloadStandardSystemIdentifiers` target). Offline builds need these files cached in `obj/`.

## LAS version conditional compilation

This is the single biggest gotcha. `Directory.Versions.props` defines `MaximumLasVersion` (default `1.5`, range 1.1-1.5) which sets `SupportLas12`..`SupportLas15` and `LAS1_x` / `LAS1_x_OR_GREATER` `DefineConstants`.

- `src/IO.Las/IO.Las.csproj` and `tests/IO.Las.Tests/IO.Las.Tests.csproj` `<Compile Remove="…" />` entire files based on these flags (e.g. `*Color*.cs` removed when `SupportLas12 != true`, `*Waveform*.cs` when `<1.3`, `*Extended*PointDataRecord*.cs` and `*ExtraBytes*.cs` when `<1.4`). When adding new files for a specific LAS version, use the matching naming convention or add a new `<Compile Remove>` guard.
- CI runs the full test matrix against LAS versions 1.1, 1.2, 1.3, 1.4, 1.5 via `env.MaximumLasVersion`. To reproduce a CI failure locally: `MaximumLasVersion=1.2 dotnet test ...`.
- Resources under `src/IO.Las/Properties/v1.x/` are similarly stripped. Do not reference v1.x resources unconditionally.

## Common commands

- Build: `dotnet build` (after building the generator, see above)
- Test all: `dotnet test`
- Test single project: `dotnet test --project tests/IO.Las.Tests/IO.Las.Tests.csproj`
- Test one TFM: `dotnet test --project tests/IO.Las.Tests/IO.Las.Tests.csproj --framework net10.0`
- Format (runs whitespace + style + analyzers): `./format.sh` — excludes diagnostic `S1133`; writes reports to `whitespace.report.json`, `style.report.json`, `analyzers.report.json` at repo root.
- Clean: `./clean.sh` — removes all `bin`/`obj`/`TestResults`, root `nupkg`/`versions`/`.vs`, and `*.binlog`.
- Coverage: `./coverage.sh` — installs `dotnet-reportgenerator-globaltool`, runs `dotnet test --coverage --coverage-output-format cobertura`, outputs HTML to `coverage/report`.
- Breaking-change check: `./check.version.sh` — installs `altemiq.semanticversioning`, runs `dotnet semver diff`.

## Test data

- `.las`, `.laz`, `.lax`, `.db`, `.zip`, `.mdb` are Git LFS tracked. Clone with `git lfs pull` or `actions/checkout@v6` with `lfs: true` (as CI does). Missing LFS = missing point cloud fixtures = test failures that look like parse errors.
- `tests/IO.Las.Tests/IO.Las.Tests.csproj` embeds `../../data/las/fusa.las` plus any `*.las`/`*.bin` next to the test code. Place new fixtures under `data/` or the test project, not arbitrary paths.
- `tools/abenberg_data_2008.sh` downloads and prepares a large public dataset (requires LAStools binaries and several system libs via apt). It is not part of any automated test; only run when explicitly working with that dataset.

## Project layout highlights

- `src/IO.Las` — core. Heavily partial-class based; MSBuild sets `DependentUpon` for `PointConverter.*.cs`, `HeaderBlockBuilder.*.cs`, `ExtensionMethods.*.cs`, `Vector*D.*.cs` so nested filenames group in IDEs.
- `src/IO.Las.Compression` depends on internals of `IO.Las` via `InternalsVisibleTo` (see `src/IO.Las/IO.Las.csproj`). Same for `Arrow`, `Cloud`, `Indexing`, and the `las` CLI. Debug builds also expose internals to `IO.Las.Tests` and `IO.Las.Compression.Tests`.
- `generators/IO.Las.CodeGeneration` — Roslyn source generator (`IsRoslynComponent=true`, netstandard2.0) that generates code from `Constants.cs`, `Color`, `Platform` (uses downloaded `platforms.csv`/`models.csv`), and `Model` info. Generator is wired via `<ProjectReference … OutputItemType="Analyzer" ReferenceOutputAssembly="false" />`.
- `tools/Las` produces the `las` CLI; packaged as both a multi-arch binary and an OCI container via `dotnet publish /t:PublishContainer`.
- `web/hosts/` contains .NET Aspire app hosts (`IO.Las.AWS.AppHost`, `IO.Las.Azure.AppHost`) — not part of default solution build for tests.
- `container/` has Linux/Windows container files and PROJ / VC_redist assets used by the published CLI image.

## Code conventions

- `Company=Altemiq`; assemblies are named `Altemiq.IO.Las*`, root namespace matches. Test projects strip the `.Tests` suffix for `RootNamespace`.
- `Nullable=enable`, `ImplicitUsings=enable`, `LangVersion=latest` in `src/` and `generators/`. Tests have `Nullable=disable`.
- Assemblies are strong-name signed from non-Debug configurations using `Las.snk`; the `PublicKey` is embedded in `src/Directory.Build.props`. Don't remove `InternalsVisibleTo` without also removing the public-key entry, and don't sign Debug builds.
- `AnalysisLevel=latest`. Pre-suppressed globally: `SA1642`, `SA1643` (ctor/finalizer doc), and via `.editorconfig`: `RCS1163` (unused param) off, `S1133` demoted to suggestion. The `format.sh` script also excludes `S1133`. When adding a deprecated API, `[Obsolete]` is fine; don't re-enable `S1133`.
- `EmbeddedResource` generation uses `GenerateStronglyTypedResources=true` with namespace `Altemiq.<ProjectName>`.
- SIMD / hardware intrinsics are real here: `Vector2D`/`Vector3D` have `.Intrinsics.cs` and `.Fallback.cs` partials. Match this pattern when adding new vectorised types.
- `Meziantou.Polyfill` is used heavily with explicit `MeziantouPolyfill_IncludedPolyfills` lists per project. When targeting older TFMs, add to the include list rather than using ad-hoc shims.
- Spelling dictionary lives in `Las.sln.DotSettings` — `asprs`, `copc`, `lasf`, `evlr`, `vlr`, `riegl`, `altemiq`, `meziantou`, `epsg`, `northing`, `transcoded`, etc. are intentional.
- Uses roslyn analyzers from `Meziantou.Analyzer`, `Roslynator.Analyzers`, `SonarAnalyzer.CSharp`, and `StyleCop.Analyzers.Unstable` through `Altemiq.DotNet.CodingStandard`

## CI / release

- `.github/workflows/build.yml` — runs tests against LAS matrix, builds NuGets, publishes CLI for win/linux/osx × x64/arm64, builds OCI container, and on `main` deploys to GitHub Packages + `ghcr.io/altemiq/io/las` and attaches tools artifacts to the release.
- `.github/workflows/release.yml`, `container.yml` — release and container publish flows. `container.yml` is excluded from build triggers.
- Versioning: `VersionPrefix` defaults to `$(MaximumLasVersion).$(MinorVersion)` (e.g. `1.5.0`). Suffix is `alpha` for PRs and `beta` for pushes to main; final versions come from `dotnet semver`.

## When modifying the repo

- If you add files to `src/IO.Las`, confirm they don't collide with the LAS-version `<Compile Remove>` globs unless that is intentional.
- If you add a test fixture binary, ensure it's tracked by Git LFS (see `.gitattributes`).
- If you change public API, run `./check.version.sh` (or be prepared for the release pipeline to catch it).
- Prefer `./format.sh` over editor-level formatting; it runs whitespace, style, and analyzer passes matching CI expectations.
