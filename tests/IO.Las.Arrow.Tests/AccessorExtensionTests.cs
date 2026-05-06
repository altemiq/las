#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
namespace Altemiq.IO.Las.Arrow;

[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("ReSharper", "InvokeAsExtensionMember")]
public class AccessorExtensionTests
{
    [Test]
    public async Task SetValues()
    {
        await Assert.That(() => PointDataRecordFieldAccessorsExtensions.SetClassification(new byte[PointDataRecord.Size], Classification.HighVegetation)).ThrowsNothing();
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task SetExtendedValues()
    {
        await Assert.That(() => ExtendedPointDataRecordFieldAccessorsExtensions.SetClassification(new byte[ExtendedGpsPointDataRecord.Size], ExtendedClassification.HighNoise)).ThrowsNothing();
    }
#endif
}
#endif