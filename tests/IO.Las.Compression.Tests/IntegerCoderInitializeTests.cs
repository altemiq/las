// -----------------------------------------------------------------------
// <copyright file="IntegerCoderInitializeTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

using System.Reflection;
using Readers.Compressed;

/// <summary>
/// Tests asserting the Initialize paths in <see cref="IntegerCompressor"/>,
/// <see cref="IntegerDecompressor"/>, and the layered <see cref="LayeredValue"/> byte readers.
///
/// These tests are specifically written to guard the LINQ-to-for-loop refactor
/// in <c>IntegerCompressor.Initialize</c>, <c>IntegerDecompressor.Initialize</c>,
/// <c>ByteReader3.Initialize</c>, and <c>ByteReader4.Initialize</c>.
/// </summary>
public class IntegerCoderInitializeTests
{
    private static readonly FieldInfo CorrectorModelsDec = typeof(IntegerDecompressor)
        .GetField("correctorModels", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo BitsModelsDec = typeof(IntegerDecompressor)
        .GetField("bitsModels", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo CorrectorBitModelDec = typeof(IntegerDecompressor)
        .GetField("correctorBitModel", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo CorrectorModelsEnc = typeof(IntegerCompressor)
        .GetField("correctorModels", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo BitsModelsEnc = typeof(IntegerCompressor)
        .GetField("bitsModels", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo CorrectorBitModelEnc = typeof(IntegerCompressor)
        .GetField("correctorBitModel", BindingFlags.Instance | BindingFlags.NonPublic);

    [Test]
    public async Task IntegerDecompressorInitializeSkipsNullSlotZeroAndInitsAllOthers()
    {
        var decoder = new ArithmeticDecoder();

        // bits=16, contexts=4 -> correctorModels.Length = 17, [0] stays null.
        var decompressor = new IntegerDecompressor(decoder, bits: 16, contexts: 4);

        decompressor.Initialize();

        await Assert.That(CorrectorModelsDec.GetValue(decompressor) as ArithmeticSymbolModel[])
            .IsNotNull().And
            .Count().IsEqualTo(17).And
            .Satisfies(cm => cm[0], m => m.IsNull()).And
            .Satisfies(cm => cm.Skip(1), m=> m.All((ArithmeticSymbolModel sm) => sm?.Initialized is true, "Is initialized"));

        // All bitsModels and the corrector bit model must be initialized.
        await Assert.That(BitsModelsDec.GetValue(decompressor) as ArithmeticSymbolModel[])
            .IsNotNull().And
            .Count().IsEqualTo(4).And.All(m => m.Initialized);

        await Assert.That(CorrectorBitModelDec.GetValue(decompressor)).IsNotNull();
    }

    [Test]
    public async Task IntegerCompressorInitializeSkipsNullSlotZeroAndInitsAllOthers()
    {
        var encoder = new ArithmeticEncoder();

        var compressor = new IntegerCompressor(encoder, bits: 16, contexts: 4);

        compressor.Initialize();

        await Assert.That(CorrectorModelsEnc.GetValue(compressor) as ArithmeticSymbolModel[])
            .IsNotNull().And
            .Count().IsEqualTo(17).And
            .Satisfies(cm => cm[0], m => m.IsNull()).And
            .Satisfies(cm => cm.Skip(1), m=> m.All((ArithmeticSymbolModel sm) => sm?.Initialized is true, "Is initialized"));

        await Assert.That(BitsModelsEnc.GetValue(compressor) as ArithmeticSymbolModel[])
            .IsNotNull().And
            .Count().IsEqualTo(4).And.All(m => m.Initialized);

        await Assert.That(CorrectorBitModelEnc.GetValue(compressor)).IsNotNull();
    }

    [Test]
    [Arguments(16U, 1U)]
    [Arguments(16U, 4U)]
    [Arguments(32U, 1U)]
    public async Task IntegerCompressorDecompressorRoundTrip(uint bits, uint contexts)
    {
        // This round-trips integer values through the coder.
        // Encode requires Initialize (which sets up correctorModels[1..]).
        // If the refactor misses any corrector model, EncodeSymbol will throw
        // IndexOutOfRangeException / CompressionNotInitializedException.
        //
        // We pick prediction/real pairs with a corrector spanning K = 0..14,
        // which exercises correctorModels[1] through correctorModels[14],
        // a superset of the models touched by any single large-K path.
        // Reals are chosen such that they stay within the signed interval
        // [correctorMin..correctorMax] after adding pred, so Decompress
        // returns the same value without wraparound.
        var pairs = new (int Pred, int Real)[]
        {
            (0, 0),            // K = 0, corrector bit 0
            (0, 1),            // K = 0, corrector bit 1
            (100, 101),        // K = 1
            (100, 97),         // K = 2
            (200, 208),        // K = 3
            (500, 515),        // K = 4
            (100, 131),        // K = 5
            (100, 37),         // K = 6
            (1000, 1127),      // K = 7  (boundary of bitsHigh=8)
            (100, 355),        // K = 8
            (1000, 1511),      // K = 9  (exercises GetLargeK path)
            (2000, 977),       // K = 10
            (2048, 4095),      // K = 11
            (5000, 905),       // K = 12
            (8192, 16383),     // K = 13
            (16383, 32766),    // K = 14
        };

        byte[] encoded;
        using (var output = new MemoryStream())
        {
            var encoder = new ArithmeticEncoder();
            _ = encoder.Initialize(output);

            var compressor = new IntegerCompressor(encoder, bits: bits, contexts: contexts);
            compressor.Initialize();

            for (var i = 0; i < pairs.Length; i++)
            {
                compressor.Compress(pairs[i].Pred, pairs[i].Real, context: (uint)(i % contexts));
            }

            encoder.Done();
            encoded = output.ToArray();
        }

        await Assert.That(encoded).Count().IsGreaterThan(0);

        using var input = new MemoryStream(encoded);
        var decoder = new ArithmeticDecoder();
        _ = decoder.Initialize(input);

        var decompressor = new IntegerDecompressor(decoder, bits: bits, contexts: contexts);
        decompressor.Initialize();

        for (var i = 0; i < pairs.Length; i++)
        {
            var decoded = decompressor.Decompress(pairs[i].Pred, context: (uint)(i % contexts));
            await Assert.That(decoded).IsEqualTo(pairs[i].Real);
        }

        decoder.Done();
    }

    [Test]
    public async Task LayeredValueInitializeIfRequestedCumulativeIndexing()
    {
        // This mirrors what ByteReader3/4.Initialize do with Aggregate:
        // walk the LayeredValue[] in order, passing the cumulative byte offset as index.
        // The new for-loop implementation must produce the same cumulative index behavior.
        var requested0 = new LayeredValue(requested: true) { ByteCount = 7 };
        var requested1 = new LayeredValue(requested: false) { ByteCount = 5 }; // skipped in stream but still advances stream
        var requested2 = new LayeredValue(requested: true) { ByteCount = 11 };
        var requested3 = new LayeredValue(requested: true) { ByteCount = 0 };  // 0 byte count: no action

        var layered = new[] { requested0, requested1, requested2, requested3 };

        // Total buffered bytes actually consumed by the *requested* layers.
        var expectedReadCount = requested0.GetByteCountIfRequested()
            + requested1.GetByteCountIfRequested()
            + requested2.GetByteCountIfRequested()
            + requested3.GetByteCountIfRequested();
        await Assert.That(expectedReadCount).IsEqualTo(18U);

        // LayeredValue.InitializeIfRequested also advances past non-requested, non-zero layers
        // via stream.Position += ByteCount, so the underlying stream needs 7 + 5 + 11 = 23 bytes.
        var streamBytes = new byte[32];
        for (var i = 0; i < streamBytes.Length; i++)
        {
            streamBytes[i] = (byte)(i + 1);
        }

        using var stream = new MemoryStream(streamBytes);
        var buffer = new byte[expectedReadCount];

        // Cumulative index is what the old Aggregate was computing.
        uint cumulativeIndex = 0;
        for (var i = 0; i < layered.Length; i++)
        {
            cumulativeIndex += layered[i].InitializeIfRequested(stream, buffer, (int)cumulativeIndex);
        }

        // Cumulative index must equal total requested bytes read.
        await Assert.That(cumulativeIndex).IsEqualTo(18U);

        // Buffer contents should be stream bytes 1..7 (requested0), then stream bytes 13..23 (requested2)
        // because layer 1 (5 bytes) is skipped via stream.Position +=.
        for (var i = 0; i < 7; i++)
        {
            await Assert.That(buffer[i]).IsEqualTo((byte)(i + 1));
        }

        for (var i = 0; i < 11; i++)
        {
            await Assert.That(buffer[7 + i]).IsEqualTo((byte)(i + 13));
        }

        // Changed flags reflect whether the layer was requested AND had a nonzero byte count.
        await Assert.That(requested0.Changed).IsTrue();
        await Assert.That(requested1.Changed).IsFalse();
        await Assert.That(requested2.Changed).IsTrue();
        await Assert.That(requested3.Changed).IsFalse();
    }
}
