// -----------------------------------------------------------------------
// <copyright file="LazEncodeBenchmarks.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Benchmarks for the LAZ compression (encode) hot path. Exercises the
/// arithmetic encoder, integer compressor, streaming median, and
/// <see cref="LazWriter"/> point-write loop.
/// </summary>
[MemoryDiagnoser]
public class LazEncodeBenchmarks
{
    private const int PointCount = 50_000;

    private GpsPointDataRecord[] points = [];

    [GlobalSetup]
    public void Setup()
    {
        // Generate a deterministic point cloud that resembles a real scan:
        // predictable X/Y drift, small Z variation, varying intensity and return.
        // Using a seeded PRNG keeps the workload identical across runs.
        var rng = new Random(42);
        var quantizer = new PointDataRecordQuantizer();
        var baseGpsTime = quantizer.GetGpsTime(new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        this.points = new GpsPointDataRecord[PointCount];

        var x = 500_000;
        var y = 4_000_000;
        var z = 1_000;
        var gpsTime = baseGpsTime;
        for (var i = 0; i < PointCount; i++)
        {
            x += rng.Next(-5, 6);
            y += rng.Next(-5, 6);
            z += rng.Next(-2, 3);
            gpsTime += 0.0001;
            this.points[i] = new()
            {
                X = x,
                Y = y,
                Z = z,
                Intensity = (ushort)rng.Next(0, 1024),
                ReturnNumber = (byte)((i % 5) + 1),
                NumberOfReturns = 5,
                ScanDirectionFlag = false,
                EdgeOfFlightLine = false,
                Classification = (Classification)2,
                ScanAngleRank = (sbyte)rng.Next(-30, 31),
                UserData = 0,
                PointSourceId = 1,
                GpsTime = gpsTime,
            };
        }
    }

    [Benchmark]
    [BenchmarkCategory("Encode", "Gps")]
    public long EncodeGpsPoints()
    {
        using var stream = new MemoryStream(capacity: PointCount * 32);
        var builder = HeaderBlockBuilder.FromPointType<GpsPointDataRecord>();
        builder.Version = new(1, 2); // format 1 is valid for LAS 1.2-1.4
        builder.NumberOfPointRecords = (uint)this.points.Length;
        builder.FileCreation = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#if LAS1_2_OR_GREATER
        builder.GlobalEncoding = GlobalEncoding.StandardGpsTime;
#endif
        builder.SetCompressed();

        using (var writer = new LazWriter(stream, leaveOpen: true))
        {
            writer.Write(builder.HeaderBlock);
            foreach (var point in this.points)
            {
                writer.Write(point);
            }
        }

        return stream.Length;
    }
}