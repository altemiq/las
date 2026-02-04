using Apache.Arrow;
using Apache.Arrow.Types;

namespace Altemiq.IO.Las.Arrow;

public class ArrowLasReaderTests
{
    [Test]
    public async Task ReadFromArrowBatch()
    {
        var reader = new Data.MockLasReader();

        var batches = reader.ToArrowBatches();

        var arrowReader = new ArrowLasReader(batches);

        var first = arrowReader.ReadPointDataRecord().PointDataRecord;

        await Assert.That(first).IsNotNull().And.IsEqualTo(Data.MockLasReader.First);
    }

    [Test]
    public async Task ReadFromArrowBatchByIndex()
    {
        var reader = new Data.MockLasReader();

        var batches = reader.ToArrowBatches();

        var arrowReader = new ArrowLasReader(batches);

        var second = arrowReader.ReadPointDataRecord(1).PointDataRecord;

        await Assert.That(second).IsNotNull().And.IsEqualTo(Data.MockLasReader.Second);
    }

    [Test]
    public async Task ReadAcrossBatches()
    {
        var reader = new Data.MockLasReader();

        var schema = reader.GetArrowSchema();

        var batches = CreateRandomData(schema);

        var arrowReader = new ArrowLasReader(batches);

        IBasePointDataRecord record = default;
        for (var i = 0; i < 15000; i++)
        {
            record = arrowReader.ReadPointDataRecord().PointDataRecord;
        }

        await Assert.That(record).IsNotNull();
    }

    [Test]
    public async Task ReadAcrossBatchesByIndex()
    {
        var reader = new Data.MockLasReader();

        var schema = reader.GetArrowSchema();

        var batches = CreateRandomData(schema);

        var arrowReader = new ArrowLasReader(batches);

        var record = arrowReader.ReadPointDataRecord(15000).PointDataRecord;

        await Assert.That(record).IsNotNull();
    }

    private static IEnumerable<RecordBatch> CreateRandomData(Schema schema, int batchSize = 10000, int batches = 10)
    {
        for (int i = 0; i < batches; i++)
        {
            yield return CreateRecordBatch(schema, batchSize);
        }

        static RecordBatch CreateRecordBatch(Schema schema, int size = 10000)
        {
            var random = new Random((int)DateTime.UtcNow.Ticks);

            var arrays = schema.FieldsList.Select<Field, IArrowArray>(field => field.DataType.TypeId switch
                {
                    ArrowTypeId.Boolean => GetArray(random, new BooleanArray.Builder(), size, random => random.NextDouble() > 0.5),
                    ArrowTypeId.UInt8 => GetArray(random, new UInt8Array.Builder(), size, random => (byte)random.Next(byte.MinValue, byte.MaxValue)),
                    ArrowTypeId.Int8 => GetArray(random, new Int8Array.Builder(), size, random => (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue)),
                    ArrowTypeId.UInt16 => GetArray(random, new UInt16Array.Builder(), size, random => (ushort)random.Next(ushort.MinValue, ushort.MaxValue)),
                    ArrowTypeId.Int16 => GetArray(random, new Int16Array.Builder(), size, random => (short)random.Next(short.MinValue, short.MaxValue)),
                    ArrowTypeId.UInt32 => GetArray(random, new UInt32Array.Builder(), size, random => (uint)random.NextInt64(uint.MinValue, uint.MaxValue)),
                    ArrowTypeId.Int32 => GetArray(random, new Int32Array.Builder(), size, random => random.Next()),
                    ArrowTypeId.UInt64 => GetArray(random, new UInt64Array.Builder(), size, random => (ulong)random.NextInt64(uint.MinValue, uint.MaxValue)),
                    ArrowTypeId.Int64 => GetArray(random, new Int64Array.Builder(), size, random => random.NextInt64()),
                    ArrowTypeId.Float => GetArray(random, new FloatArray.Builder(), size, random => random.NextSingle() * ushort.MaxValue),
                    ArrowTypeId.Double => GetArray(random, new DoubleArray.Builder(), size, random => random.NextDouble() * int.MaxValue),
                    _ => throw new System.Diagnostics.UnreachableException(),
                })
                .ToList();

            return new(schema, arrays, size);

            static TArray GetArray<T, TArray, TBuilder>(Random random, IArrowArrayBuilder<T, TArray, TBuilder> builder, int size, Func<Random, T> randomizer)
                where TArray : IArrowArray
                where TBuilder : IArrowArrayBuilder<TArray>
            {
                for (var i = 0; i < size; i++)
                {
                    builder.Append(randomizer(random));
                }

                return builder.Build(null);
            }
        }
    }
}