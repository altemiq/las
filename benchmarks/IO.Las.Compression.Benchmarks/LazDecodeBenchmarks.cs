// -----------------------------------------------------------------------
// <copyright file="LazDecodeBenchmarks.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Benchmarks for the LAZ decompression hot paths. These exercise
/// the arithmetic coder, integer (de)compressor, streaming median,
/// color/extra-byte readers, and the parsed <see cref="LazReader"/>
/// code path.
/// </summary>
/// <remarks>
/// This benchmark file is deliberately written to be source-compatible
/// with every published version of <c>Altemiq.IO.Las.Compression</c> on
/// the GitHub Packages feed, so the same binary can be compiled against
/// either the local source tree or a published baseline package for
/// side-by-side throughput comparison. See <see cref="Program"/> for the
/// per-job <c>LasCompressionVersion</c> swap config.
/// </remarks>
[MemoryDiagnoser]
public class LazDecodeBenchmarks
{
    private byte[] fusa = [];
    private byte[] coloured = [];
    private byte[] fusaHeight7 = [];

    [GlobalSetup]
    public void Setup()
    {
        // Load the LAZ fixtures into memory once. Each iteration reads from a
        // fresh MemoryStream wrapping this array, isolating decoder throughput
        // from disk I/O and giving deterministic timing.
        this.fusa = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "data", "fusa.laz"));
        this.coloured = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "data", "coloured.laz"));
        this.fusaHeight7 = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "data", "fusa_height_7.laz"));
    }

    [Benchmark]
    [BenchmarkCategory("Fusa", "Parsed")]
    public long DecodeFusaParsed() => DecodeAll(this.fusa);

    [Benchmark]
    [BenchmarkCategory("Coloured", "Parsed")]
    public long DecodeColouredParsed() => DecodeAll(this.coloured);

    [Benchmark]
    [BenchmarkCategory("FusaHeight7", "Parsed")]
    public long DecodeFusaHeight7Parsed() => DecodeAll(this.fusaHeight7);

    private static long DecodeAll(byte[] laz)
    {
        using var stream = new MemoryStream(laz, writable: false);
        using var reader = new LazReader(stream);

        var count = 0L;
        while (true)
        {
            var point = reader.ReadPointDataRecord();
            if (point.PointDataRecord is null)
            {
                break;
            }

            count++;
        }

        return count;
    }
}
