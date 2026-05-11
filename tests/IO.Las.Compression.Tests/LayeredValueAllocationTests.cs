// -----------------------------------------------------------------------
// <copyright file="LayeredValueAllocationTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if NETCOREAPP2_0_OR_GREATER

namespace Altemiq.IO.Las.Compression;

using Readers.Compressed;

/// <summary>
/// Asserts that <see cref="LayeredValue.Initialize(Stream, byte[], int)"/> and
/// <see cref="LayeredValue.InitializeIfRequested(Stream, byte[], int)"/> do
/// not allocate a new backing <see cref="Stream"/> on every call.
/// </summary>
/// <remarks>
/// The original implementation constructed a fresh <see cref="MemoryStream"/>
/// on each call, so for a v1.4 layered chunk with N layers × M chunks it
/// allocated N × M MemoryStreams per file. The optimisation re-uses a single
/// re-targetable stream per <see cref="LayeredValue"/>. This test locks in
/// the invariant by measuring allocations on the warm path.
/// </remarks>
public class LayeredValueAllocationTests
{
    [Test]
    public async Task InitializeDoesNotAllocateOnWarmCalls()
    {
        var value = new LayeredValue(requested: true) { ByteCount = 8 };
        var buffer = new byte[8];
        var streamBytes = new byte[64];
        for (var i = 0; i < streamBytes.Length; i++)
        {
            streamBytes[i] = (byte)i;
        }

        using var stream = new MemoryStream(streamBytes);

        // Warm-up: first call is allowed to allocate (the cached stream
        // instance gets created lazily inside LayeredValue).
        _ = value.Initialize(stream, buffer);
        stream.Position = 0;

        // Now measure. Subsequent calls must not allocate a new Stream.
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 128; i++)
        {
            stream.Position = 0;
            _ = value.Initialize(stream, buffer);
        }

        var delta = GC.GetAllocatedBytesForCurrentThread() - before;

        // The read + decoder.Initialize path is entirely synchronous and uses
        // stack-allocated spans. Any per-iteration managed allocation (e.g.
        // a fresh MemoryStream) would be hundreds of bytes × 128 iterations,
        // so a small constant budget is safe.
        await Assert.That(delta)
            .IsLessThan(2_048)
            .Because($"warm Initialize() should not allocate a new backing stream per call; observed {delta} bytes across 128 iterations");
    }

    [Test]
    public async Task InitializeIfRequestedDoesNotAllocateOnWarmCalls()
    {
        var value = new LayeredValue(requested: true) { ByteCount = 8 };
        var buffer = new byte[8];
        var streamBytes = new byte[64];
        for (var i = 0; i < streamBytes.Length; i++)
        {
            streamBytes[i] = (byte)i;
        }

        using var stream = new MemoryStream(streamBytes);

        // Warm-up
        stream.Position = 0;
        _ = value.InitializeIfRequested(stream, buffer);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 128; i++)
        {
            stream.Position = 0;
            value.ByteCount = 8;
            _ = value.InitializeIfRequested(stream, buffer);
        }

        var delta = GC.GetAllocatedBytesForCurrentThread() - before;

        await Assert.That(delta)
            .IsLessThan(2_048)
            .Because($"warm InitializeIfRequested() should not allocate a new backing stream per call; observed {delta} bytes across 128 iterations");
    }
}

#endif
