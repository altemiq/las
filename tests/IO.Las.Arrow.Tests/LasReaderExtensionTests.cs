using Apache.Arrow.Types;

namespace Altemiq.IO.Las.Arrow;

public class LasReaderExtensionTests
{
    [Test]
    public async Task GetSchema()
    {
        var reader = new Data.MockLasReader();

        var schema = reader.GetArrowSchema();
        
        // ensure we have some byte, and ushort fields
        await Assert.That(schema.FieldsList)
            .Contains(p => p.DataType.TypeId is ArrowTypeId.UInt8).And
            .Contains(p => p.DataType.TypeId is ArrowTypeId.UInt16);
    }

    [Test]
    public async Task ToArrowBatches()
    {
        var reader = new Data.MockLasReader();

        var batches = reader.ToArrowBatches();

        await Assert.That(batches).Count().IsEqualTo(1);
    }
    
    
}