// -----------------------------------------------------------------------
// <copyright file="VlrCopyToTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

public sealed class TextAreaDescriptionTests
{
    [Test]
    [Arguments("UNIT")]
    [Arguments("HELLO WORLD")]
    [Arguments("A")]
    public async Task NullTerminatorImmediatelyAfterValue(string value)
    {
        var record = new TextAreaDescription(value);
        var buffer = new byte[record.Size];

        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(record.Size);

        var expected = System.Text.Encoding.ASCII.GetBytes(value);
        _ = await Assert.That(buffer.AsSpan(VariableLengthRecordHeader.Size, expected.Length).ToArray()).IsEquivalentTo(expected);

        // NUL must be at the position immediately after the text — not at a further offset derived
        // from the accumulated byte count (which is what the old bug produced).
        _ = await Assert.That(buffer[VariableLengthRecordHeader.Size + expected.Length]).IsEqualTo((byte)0);
    }

    [Test]
    public async Task SingleCharValue()
    {
        var record = new TextAreaDescription("Z");
        var buffer = new byte[record.Size];

        _ = record.CopyTo(buffer);

        _ = await Assert.That(record.Size).IsEqualTo(VariableLengthRecordHeader.Size + 2);
        _ = await Assert.That(buffer[VariableLengthRecordHeader.Size]).IsEqualTo((byte)'Z');
        _ = await Assert.That(buffer[VariableLengthRecordHeader.Size + 1]).IsEqualTo(0);
    }

    [Test]
    public async Task CopyToBufferOfExactSize()
    {
        var value = "TEST";
        var record = new TextAreaDescription(value);
        var bufferSize = VariableLengthRecordHeader.Size + value.Length + 1;

        // A buffer of exactly the expected size leaves no room for an out-of-range NUL write,
        // so the old (buggy) code would have thrown IndexOutOfRangeException at this call.
        var buffer = new byte[bufferSize];
        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(bufferSize);
        _ = await Assert.That(buffer[^1]).IsEqualTo(0);
    }
}

public sealed class GeoAsciiParamsTagTests
{
    [Test]
    public async Task TwoStrings()
    {
        var record = new GeoAsciiParamsTag("UNIT", "METER");
        var buffer = new byte[record.Size];

        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(record.Size);

        int offset = VariableLengthRecordHeader.Size;

        var unitBytes = System.Text.Encoding.ASCII.GetBytes("UNIT");
        _ = await Assert.That(buffer.AsSpan(offset, unitBytes.Length).ToArray()).IsEquivalentTo(unitBytes);
        _ = await Assert.That(buffer[offset + unitBytes.Length]).IsEqualTo(0);  // NUL after "UNIT"

        offset += unitBytes.Length + 1;

        var meterBytes = System.Text.Encoding.ASCII.GetBytes("METER");
        _ = await Assert.That(buffer.AsSpan(offset, meterBytes.Length).ToArray()).IsEquivalentTo(meterBytes);
        _ = await Assert.That(buffer[offset + meterBytes.Length]).IsEqualTo(0); // NUL after "METER"
    }

    [Test]
    public async Task SingleString()
    {
        var record = new GeoAsciiParamsTag("EARTH");
        var buffer = new byte[record.Size];

        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(record.Size);

        int offset = VariableLengthRecordHeader.Size;
        var earthBytes = System.Text.Encoding.ASCII.GetBytes("EARTH");
        _ = await Assert.That(buffer.AsSpan(offset, earthBytes.Length).ToArray()).IsEquivalentTo(earthBytes);
        _ = await Assert.That(buffer[offset + earthBytes.Length]).IsEqualTo(0); // NUL after "EARTH"
    }

    [Test]
    public async Task ThreeStrings()
    {
        var record = new GeoAsciiParamsTag("A", "BC", "DEF");
        var buffer = new byte[record.Size];

        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(record.Size);

        int offset = VariableLengthRecordHeader.Size;

        // "A" (1 byte) + NUL
        _ = await Assert.That(buffer[offset]).IsEqualTo((byte)'A');
        _ = await Assert.That(buffer[offset + 1]).IsEqualTo(0);
        offset += 2;

        // "BC" (2 bytes) + NUL
        _ = await Assert.That(buffer[offset]).IsEqualTo((byte)'B');
        _ = await Assert.That(buffer[offset + 1]).IsEqualTo((byte)'C');
        _ = await Assert.That(buffer[offset + 2]).IsEqualTo(0);
        offset += 3;

        // "DEF" (3 bytes) + NUL — last byte of buffer
        _ = await Assert.That(buffer[offset]).IsEqualTo((byte)'D');
        _ = await Assert.That(buffer[offset + 1]).IsEqualTo((byte)'E');
        _ = await Assert.That(buffer[offset + 2]).IsEqualTo((byte)'F');
        _ = await Assert.That(buffer[offset + 3]).IsEqualTo(0);
    }

    [Test]
    public async Task BufferOfExactSize()
    {
        var record = new GeoAsciiParamsTag("UNIT", "METER");
        var bufferSize = VariableLengthRecordHeader.Size + (4 + 1) + 5 + 1;

        var buffer = new byte[bufferSize];
        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(bufferSize);
        _ = await Assert.That(buffer[^1]).IsEqualTo(0);
    }
}

#if LAS1_4_OR_GREATER
public sealed class OgcCoordinateSystemWktTests
{
    [Test]
    public async Task NullTerminatorImmediatelyAfterWkt()
    {
        var record = new OgcCoordinateSystemWkt("GEOGCS[4326]");
        var buffer = new byte[record.Size];

        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(record.Size);

        // The last byte of an exactly-sized buffer must be the NUL terminator.
        // The old buggy code wrote the NUL far beyond this position (IndexOutOfRangeException).
        _ = await Assert.That(buffer[^1]).IsEqualTo(0);
    }

    [Test]
    public async Task ContentIsNonZero()
    {
        var record = new OgcCoordinateSystemWkt("GEOGCS[4326]");
        var wktByteLength = record.Header.RecordLengthAfterHeader - 1; // excludes the NUL
        var nulOffset = VariableLengthRecordHeader.Size + wktByteLength;

        _ = await Assert.That(record.Size).IsEqualTo(nulOffset + 1);

        var buffer = new byte[record.Size];
        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(record.Size);
        _ = await Assert.That(buffer[nulOffset]).IsEqualTo(0);
        _ = await Assert.That(buffer[VariableLengthRecordHeader.Size]).IsNotEqualTo(0); // WKT content
    }

    [Test]
    public async Task BufferOfExactSize()
    {
        var record = new OgcCoordinateSystemWkt("GEOGCS[4326]");
        var buffer = new byte[record.Size];

        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(record.Size);
        _ = await Assert.That(buffer[^1]).IsEqualTo(0);
    }
}

public sealed class OgcMathTransformWktTests
{
    [Test]
    public async Task NullTerminatorImmediatelyAfterWkt()
    {
        var record = new OgcMathTransformWkt("GEOGCS[4326]");
        var buffer = new byte[record.Size];

        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(record.Size);
        _ = await Assert.That(buffer[^1]).IsEqualTo(0);
    }

    [Test]
    public async Task ContentIsNonZero()
    {
        var record = new OgcMathTransformWkt("GEOGCS[4326]");
        var wktByteLength = record.Header.RecordLengthAfterHeader - 1;
        var nulOffset = VariableLengthRecordHeader.Size + wktByteLength;

        _ = await Assert.That(record.Size).IsEqualTo(nulOffset + 1);

        var buffer = new byte[record.Size];
        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(record.Size);
        _ = await Assert.That(buffer[nulOffset]).IsEqualTo(0);
        _ = await Assert.That(buffer[VariableLengthRecordHeader.Size]).IsNotEqualTo(0);
    }

    [Test]
    public async Task BufferOfExactSize()
    {
        var record = new OgcMathTransformWkt("GEOGCS[4326]");
        var buffer = new byte[record.Size];

        var bytesWritten = record.CopyTo(buffer);

        _ = await Assert.That(bytesWritten).IsEqualTo(record.Size);
        _ = await Assert.That(buffer[^1]).IsEqualTo(0);
    }
}
#endif