// -----------------------------------------------------------------------
// <copyright file="ColorReader2Tests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

using Readers.Compressed;
using Writers.Compressed;

/// <summary>
/// Round-trip tests for <see cref="ColorReader2"/> / <see cref="ColorWriter2"/>.
/// These guard the refactor of <see cref="ColorReader2.Read"/> which replaces
/// repeated <c>BinaryPrimitives.ReadUInt16LittleEndian</c> / <c>WriteUInt16LittleEndian</c>
/// round-trips on a 6-byte span with local <c>ushort</c> registers.
/// </summary>
public class ColorReader2Tests
{
    [Test]
    [Arguments((ushort)0x1234, (ushort)0x5678, (ushort)0x9ABC, (ushort)0x1234, (ushort)0x5678, (ushort)0x9ABC, "unchanged colour")]
    [Arguments((ushort)0x0000, (ushort)0x0000, (ushort)0x0000, (ushort)0x00FF, (ushort)0x0000, (ushort)0x0000, "red low byte diff only")]
    [Arguments((ushort)0x0000, (ushort)0x0000, (ushort)0x0000, (ushort)0xFF00, (ushort)0x0000, (ushort)0x0000, "red high byte diff only")]
    [Arguments((ushort)0x0000, (ushort)0x0000, (ushort)0x0000, (ushort)0x00FF, (ushort)0x00FF, (ushort)0x00FF, "all low bytes diff")]
    [Arguments((ushort)0x0000, (ushort)0x0000, (ushort)0x0000, (ushort)0xFF00, (ushort)0xFF00, (ushort)0xFF00, "all high bytes diff")]
    [Arguments((ushort)0x0000, (ushort)0x0000, (ushort)0x0000, (ushort)0xFFFF, (ushort)0xFFFF, (ushort)0xFFFF, "white from black")]
    [Arguments((ushort)0xFFFF, (ushort)0xFFFF, (ushort)0xFFFF, (ushort)0x0000, (ushort)0x0000, (ushort)0x0000, "black from white")]
    [Arguments((ushort)0x8080, (ushort)0x8080, (ushort)0x8080, (ushort)0x8182, (ushort)0x7F7E, (ushort)0x8181, "small deltas both bytes")]
    [Arguments((ushort)0x1234, (ushort)0x5678, (ushort)0x9ABC, (ushort)0x1235, (ushort)0x5678, (ushort)0x9ABC, "tiny red low delta only")]
    [Arguments((ushort)0x1234, (ushort)0x5678, (ushort)0x9ABC, (ushort)0x1234, (ushort)0x5679, (ushort)0x9ABC, "tiny green low delta only")]
    [Arguments((ushort)0x1234, (ushort)0x5678, (ushort)0x9ABC, (ushort)0x1234, (ushort)0x5678, (ushort)0x9ABD, "tiny blue low delta only")]
    public async Task ColorRoundTripSingleStep(ushort lastRed, ushort lastGreen, ushort lastBlue, ushort red, ushort green, ushort blue, string description)
    {
        _ = description;

        var seed = PackRgb(lastRed, lastGreen, lastBlue);
        var input = PackRgb(red, green, blue);
        var output = RoundTripColors(seed, [input]);

        await Assert.That(output[0])
            .IsEquivalentTo(input)
            .Because($"round-trip must preserve RGB for scenario \"{description}\"");
    }

    [Test]
    public async Task ColorRoundTripSequenceExercisesAllBranches()
    {
        // A crafted sequence that walks through every combination of low/high/green/blue
        // branches in ColorReader2.Read, including large jumps that exercise the
        // .Clamp() overflow paths in both writer and reader.
        var seed = PackRgb(0x4030, 0x6050, 0x8070);

        var sequence = new[]
        {
            PackRgb(0x4030, 0x6050, 0x8070), // identical: sym bit 6 unset
            PackRgb(0x4031, 0x6050, 0x8070), // red low-byte changed only
            PackRgb(0x4131, 0x6050, 0x8070), // red high and low changed
            PackRgb(0x4131, 0x6151, 0x8070), // + green changed
            PackRgb(0x4131, 0x6151, 0x8171), // + blue changed
            PackRgb(0x0000, 0xFFFF, 0x8080), // large jumps: triggers .Clamp() overflow on green
            PackRgb(0xFFFF, 0x0000, 0x0000), // invert: large negative diffs
            PackRgb(0xFFFF, 0x0000, 0x0000), // repeat (tests unchanged branches)
            PackRgb(0x1234, 0x5678, 0x9ABC),
            PackRgb(0x1234, 0x5678, 0x9ABC), // idem
        };

        var output = RoundTripColors(seed, sequence);

        for (var i = 0; i < sequence.Length; i++)
        {
            await Assert.That(output[i])
                .IsEquivalentTo(sequence[i])
                .Because($"round-trip must preserve item {i} (input RGB bytes: [{string.Join(", ", sequence[i])}])");
        }
    }

    [Test]
    [Arguments((ushort)0x00FF, (ushort)0x0000, (ushort)0x0000, (ushort)0x0000, (ushort)0x0001, (ushort)0x0000, "negative diff_l overflow on green low byte")]
    [Arguments((ushort)0x0000, (ushort)0x0000, (ushort)0x0000, (ushort)0x00FF, (ushort)0x0001, (ushort)0x0000, "positive diff_l overflow on green low byte")]
    [Arguments((ushort)0xFF00, (ushort)0x0000, (ushort)0x0000, (ushort)0x0000, (ushort)0x0100, (ushort)0x0000, "negative diff_h overflow on green high byte")]
    [Arguments((ushort)0x0000, (ushort)0x0000, (ushort)0x0000, (ushort)0xFF00, (ushort)0x0100, (ushort)0x0000, "positive diff_h overflow on green high byte")]
    public async Task ColorRoundTripClampOverflowBranches(ushort lastRed, ushort lastGreen, ushort lastBlue, ushort red, ushort green, ushort blue, string description)
    {
        // These scenarios exercise the .Clamp() paths in the writer's corrector calculation
        // where (diff + lastItem[x]) falls outside [0..255]. The C# writer had a parenthesize
        // bug that applied .Clamp() only to `last` (which is already in range, so a no-op).
        var seed = PackRgb(lastRed, lastGreen, lastBlue);
        var input = PackRgb(red, green, blue);
        var output = RoundTripColors(seed, [input]);

        await Assert.That(output[0])
            .IsEquivalentTo(input)
            .Because($"round-trip must preserve RGB for scenario \"{description}\"");
    }

    /// <summary>
    /// Encodes the full <paramref name="sequence"/> via <see cref="ColorWriter2"/> seeded with
    /// <paramref name="seed"/>, then decodes via <see cref="ColorReader2"/> seeded with the
    /// same value, and returns the decoded colors.
    /// </summary>
    private static byte[][] RoundTripColors(byte[] seed, byte[][] sequence)
    {
        byte[] encoded;
        using (var output = new MemoryStream())
        {
            var encoder = new ArithmeticEncoder();
            _ = encoder.Initialize(output);

            var writer = new ColorWriter2(encoder);
            _ = writer.Initialize(seed);

            foreach (var color in sequence)
            {
                var scratch = (byte[])color.Clone();
                writer.Write(scratch);
            }

            encoder.Done();
            encoded = output.ToArray();
        }

        using var input = new MemoryStream(encoded);
        var decoder = new ArithmeticDecoder();
        _ = decoder.Initialize(input);

        var reader = new ColorReader2(decoder);
        _ = reader.Initialize(seed);

        var results = new byte[sequence.Length][];
        for (var i = 0; i < sequence.Length; i++)
        {
            var buffer = new byte[6];
            reader.Read(buffer);
            results[i] = buffer;
        }

        decoder.Done();
        return results;
    }

    private static byte[] PackRgb(ushort red, ushort green, ushort blue)
    {
        var buffer = new byte[6];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(buffer, red);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(2), green);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(4), blue);
        return buffer;
    }
}
