namespace Altemiq.IO.Las;

using System.Runtime.Intrinsics;

public class VectorTests
{
    [Test]
    public async Task Truncate()
    {
        await Assert.That(
            Vector256.Truncate(Vector256.Create(123.123, 234.234, 345.345, 456.456)))
            .IsEqualTo(
                Vector256.Create(123.0, 234.0, 345.0, 456.0));
    }
}