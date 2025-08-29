namespace Altemiq.IO.Las;

public class ClassificationLookupTests
{
    [Test]
    public async Task CreationFromValues()
    {
        ClassificationLookup lookup = new(
            new ClassificationLookupItem { ClassNumber = 112, Description = "Classification 112" },
            new ClassificationLookupItem { ClassNumber = 113, Description = "Classification 113" },
            new ClassificationLookupItem { ClassNumber = 132, Description = "Classification 132" });

        _ = await Assert.That(lookup[0].ClassNumber).IsEqualTo((byte)112);
        _ = await Assert.That(lookup[1].ClassNumber).IsEqualTo((byte)113);
        _ = await Assert.That(lookup[2].ClassNumber).IsEqualTo((byte)132);
    }

    [Test]
    public async Task ToBytes()
    {
        ClassificationLookup lookup = new(
            new ClassificationLookupItem { ClassNumber = 112, Description = "Classification 112" },
            new ClassificationLookupItem { ClassNumber = 113, Description = "Classification 113" },
            new ClassificationLookupItem { ClassNumber = 132, Description = "Classification 132" });

        var bytes = new byte[5000];

        lookup.Write(bytes);

        await Assert.That(bytes[VariableLengthRecordHeader.Size]).IsEqualTo((byte)112);
        await Assert.That(bytes[VariableLengthRecordHeader.Size + 16]).IsEqualTo((byte)113);
    }
}