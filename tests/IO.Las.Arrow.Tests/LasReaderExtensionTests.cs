namespace Altemiq.IO.Las.Arrow;

using Apache.Arrow.Types;

public class LasReaderExtensionTests
{
    [Test]
    public async Task GetSchema()
    {
        var reader = new Data.MockLasReader();

        var schema = reader.GetArrowSchema();

        // ensure we have some byte, and ushort fields
        _ = await Assert.That(schema.FieldsList)
            .Contains(static p => p.DataType.TypeId is ArrowTypeId.UInt8).And
            .Contains(static p => p.DataType.TypeId is ArrowTypeId.UInt16);
    }

    [Test]
    public async Task ToArrowBatches()
    {
        var reader = new Data.MockLasReader();

        var batches = reader.ToArrowBatches();

        _ = await Assert.That(batches).Count().IsEqualTo(1);
    }


}