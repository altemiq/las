namespace Altemiq.IO.Las;

public class ExtraBytesValueTests
{
    [Test]
    public async Task DefaultShouldHaveNoValue()
    {
        ExtraBytesValue value = default;
        _ = await Assert.That(value.HasValue).IsFalse();
        _ = await Assert.That(value.Value).IsNull();
    }

    [Test]
    [MatrixDataSource]
    public async Task SetByte([Matrix((byte)(byte.MaxValue * 0.05), (byte)(byte.MaxValue * 0.5), (byte)(byte.MaxValue * 0.9))] byte v)
    {
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<byte>().And.IsEqualTo(v);
        _ = await Assert.That(value.TryGetValue(out byte output)).IsTrue();
        _ = await Assert.That(output).IsEqualTo(v);
    }

    [Test]
    [MatrixDataSource]
    public async Task SetSByte([Matrix((sbyte)(sbyte.MinValue * 0.9), (sbyte)(sbyte.MinValue * 0.5), (sbyte)(sbyte.MinValue * 0.05), (sbyte)(sbyte.MaxValue * 0.05), (sbyte)(sbyte.MaxValue * 0.5), (sbyte)(sbyte.MaxValue * 0.9))] sbyte v)
    {
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<sbyte>().And.IsEqualTo(v);
        _ = await Assert.That(value.TryGetValue(out sbyte output)).IsTrue();
        _ = await Assert.That(output).IsEqualTo(v);
    }

    [Test]
    [MatrixDataSource]
    public async Task SetUInt16([Matrix((ushort)(ushort.MaxValue * 0.05), (ushort)(ushort.MaxValue * 0.5), (ushort)(ushort.MaxValue * 0.9))] ushort v)
    {
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<ushort>().And.IsEqualTo(v);
        _ = await Assert.That(value.TryGetValue(out ushort output)).IsTrue();
        _ = await Assert.That(output).IsEqualTo(v);
    }

    [Test]
    [MatrixDataSource]
    public async Task SetInt16([Matrix((short)(short.MinValue * 0.9), (short)(short.MinValue * 0.5), (short)(short.MinValue * 0.05), (short)(short.MaxValue * 0.05), (short)(short.MaxValue * 0.5), (short)(short.MaxValue * 0.9))] short v)
    {
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<short>().And.IsEqualTo(v);
        _ = await Assert.That(value.TryGetValue(out short output)).IsTrue();
        _ = await Assert.That(output).IsEqualTo(v);
    }

    [Test]
    [MatrixDataSource]
    public async Task SetUInt32([Matrix((uint)(uint.MaxValue * 0.05), (uint)(uint.MaxValue * 0.5), (uint)(uint.MaxValue * 0.9))] uint v)
    {
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<uint>().And.IsEqualTo(v);
        _ = await Assert.That(value.TryGetValue(out uint output)).IsTrue();
        _ = await Assert.That(output).IsEqualTo(v);
    }

    [Test]
    [MatrixDataSource]
    public async Task SetInt32([Matrix((int)(int.MinValue * 0.9), (int)(int.MinValue * 0.5), (int)(int.MinValue * 0.05), (int)(int.MaxValue * 0.05), (int)(int.MaxValue * 0.5), (int)(int.MaxValue * 0.9))] int v)
    {
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<int>().And.IsEqualTo(v);
        _ = await Assert.That(value.TryGetValue(out int output)).IsTrue();
        _ = await Assert.That(output).IsEqualTo(v);
    }

    [Test]
    [MatrixDataSource]
    public async Task SetUInt64([Matrix((ulong)(ulong.MaxValue * 0.05), (ulong)(ulong.MaxValue * 0.5), (ulong)(ulong.MaxValue * 0.9))] ulong v)
    {
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<ulong>().And.IsEqualTo(v);
        _ = await Assert.That(value.TryGetValue(out ulong output)).IsTrue();
        _ = await Assert.That(output).IsEqualTo(v);
    }

    [Test]
    [MatrixDataSource]
    public async Task SetInt64([Matrix((long)(long.MinValue * 0.9), (long)(long.MinValue * 0.5), (long)(long.MinValue * 0.005), -12345, 12345, (long)(long.MaxValue * 0.005), (long)(long.MaxValue * 0.5), (long)(long.MaxValue * 0.9))] long v)
    {
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<long>().And.IsEqualTo(v);
        _ = await Assert.That(value.TryGetValue(out long output)).IsTrue();
        _ = await Assert.That(output).IsEqualTo(v);
    }

    [Test]
    [MatrixDataSource]
    public async Task SetSingle([Matrix((float)(float.MinValue * 0.9), (float)(float.MinValue * 0.5), (float)(float.MinValue * 0.05), (float)(float.MaxValue * 0.05), (float)(float.MaxValue * 0.5), (float)(float.MaxValue * 0.9))] float v)
    {
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<float>().And.IsEqualTo(v);
        _ = await Assert.That(value.TryGetValue(out float output)).IsTrue();
        _ = await Assert.That(output).IsEqualTo(v);
    }

    [Test]
    [MatrixDataSource]
    public async Task SetDouble([Matrix(double.MinValue * 0.9, double.MinValue * 0.5, double.MinValue * 0.005, double.MaxValue * 0.005, -125.5, 125.5, double.MaxValue * 0.5, double.MaxValue * 0.9)] double v)
    {
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<double>().And.IsEqualTo(v);
        _ = await Assert.That(value.TryGetValue(out double output)).IsTrue();
        _ = await Assert.That(output).IsEqualTo(v);
    }

    [Test]
    public async Task SetByteArray()
    {
        var v = new byte[1024];
        System.Security.Cryptography.RandomNumberGenerator.Fill(v);
        ExtraBytesValue value = new(v);
        _ = await Assert.That(value.Value).IsTypeOf<byte[]>().And.IsEquivalentTo(v);
        _ = await Assert.That(value.TryGetValue(out byte[] output)).IsTrue();
        _ = await Assert.That(output).IsEquivalentTo(v);
    }
}