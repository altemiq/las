#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
namespace Altemiq.IO.Las.Arrow;

[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "InvokeAsExtensionMember")]
public class AccessorExtensionTests
{
    [Test]
    public async Task SetValues()
    {
        _ = await Assert.That(static () => PointDataRecordFieldAccessorsExtensions.SetClassification(new byte[PointDataRecord.Size], Classification.HighVegetation)).ThrowsNothing();
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task SetExtendedValues()
    {
        _ = await Assert.That(static () => ExtendedPointDataRecordFieldAccessorsExtensions.SetClassification(new byte[ExtendedGpsPointDataRecord.Size], ExtendedClassification.HighNoise)).ThrowsNothing();
    }
#endif
}
#endif