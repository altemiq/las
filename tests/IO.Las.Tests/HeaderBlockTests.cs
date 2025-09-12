namespace Altemiq.IO.Las;

public class HeaderBlockTests
{
    private const double X = 123.123;
    private const double Y = 456.456;
    private const double Z = 789.789;

    private const int MaxReturnNumbers =
#if LAS1_4_OR_GREATER
        15;
#else
        5;
#endif

    [Test]
    public async Task Equal()
    {
        HeaderBlockBuilder builder = new();
        var first = builder.HeaderBlock;
        var second = builder.HeaderBlock;

        _ = await Assert.That(first).IsEqualTo(second);
    }

    [Test]
    public async Task NotEqual()
    {
        HeaderBlockBuilder builder = new();
        var first = builder.HeaderBlock;
        builder.ProjectId = Guid.NewGuid();
        var second = builder.HeaderBlock;

        _ = await Assert.That(first).IsNotEqualTo(second);
    }

    [Test]
    public async Task InvalidVersion()
    {
        await Assert.That(
#if LAS1_5_OR_GREATER
            () => new HeaderBlock(default, default, default, new(0, 9), default, default, default, default, default, [], default, default, default, [], default, default, default, default, default)
#elif LAS1_4_OR_GREATER
            () => new HeaderBlock(default, default, default, new(0, 9), default, default, default, default, default, [], default, default, default, [], default, default)
#elif LAS1_2_OR_GREATER
            () => new HeaderBlock(default, default, default, new(0, 9), default, default, default, default, default, [], default, default, default, default)
#else
            () => new HeaderBlock(default, default, new(0, 9), default, default, default, default, default, [], default, default, default, default)
#endif
        ).Throws<ArgumentOutOfRangeException>().WithParameterName("version");
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task InvalidLegacyPointReturns()
    {
        await Assert.That(
#if LAS1_5_OR_GREATER
            () => new HeaderBlock(default, default, default, new(1, 4), default, default, default, default, default, [], default, default, default, [], default, default, default, default, default)
#else
            () => new HeaderBlock(default, default, default, new(1, 4), default, default, default, default, default, [], default, default, default, [], default, default)
#endif
        ).Throws<ArgumentOutOfRangeException>().WithParameterName("legacyPointsByReturn");
    }

    [Test]
    public async Task InvalidPointReturns()
    {
        await Assert.That(
#if LAS1_5_OR_GREATER
            () => new HeaderBlock(default, default, default, new(1, 4), default, default, default, default, default, [0, 0, 0, 0, 0], default, default, default, [], default, default, default, default, default)
#else
            () => new HeaderBlock(default, default, default, new(1, 4), default, default, default, default, default, [0, 0, 0, 0, 0], default, default, default, [], default, default)
#endif
        ).Throws<ArgumentOutOfRangeException>().WithParameterName("pointsByReturn");
    }
#elif LAS1_2_OR_GREATER
    [Test]
    public async Task InvalidPointReturns()
    {
        await Assert.That(() => new HeaderBlock(default, default, default, new(1, 2), default, default, default, default, default, [], default, default, default, default)).Throws<ArgumentOutOfRangeException>().WithParameterName("pointsByReturn");
    }
#else
    [Test]
    public async Task InvalidPointReturns()
    {
        await Assert.That(() => new HeaderBlock(default, default, new(1, 1), default, default, default, default, default, [], default, default, default, default)).Throws<ArgumentOutOfRangeException>().WithParameterName("pointsByReturn");
    }
#endif

    [Test]
    public async Task HashCode()
    {
        HeaderBlockBuilder builder = new();
        _ = await Assert.That(builder.HeaderBlock.GetHashCode()).IsNotEqualTo(0);
    }

    [Test]
    public async Task SetOffset()
    {
        HeaderBlockBuilder headerBlockBuilder = new();
        headerBlockBuilder.SetOffset(123456.123, 234567.321, 123.456);
        _ = await Assert.That(headerBlockBuilder.Offset).IsEqualTo(new(123000D, 234000D, 100D));
    }

    [Test]
    [MatrixDataSource]
    public async Task AddLegacy([Matrix(1, 2, 3, 4, 5)] int returnNumber, bool checkHeader)
    {
#if LAS1_5_OR_GREATER
        var headerBlockBuilder = new HeaderBlockBuilder(PointDataRecord.Id);
#else
        HeaderBlockBuilder headerBlockBuilder = new();
#endif
        if (checkHeader)
        {
            await Assert.That(headerBlockBuilder.HeaderBlock.Min).IsEqualTo(default);
            await Assert.That(headerBlockBuilder.HeaderBlock.Max).IsEqualTo(default);
        }

        headerBlockBuilder.Add(X, Y, Z, returnNumber);
#if LAS1_4_OR_GREATER
        await Assert.That(headerBlockBuilder.LegacyNumberOfPointRecords).IsEqualTo(1U);
        await Assert.That(headerBlockBuilder.LegacyNumberOfPointsByReturn).Satisfies(x => x.ElementAt(returnNumber - 1), static x => x.IsEqualTo(1U));
#else
        await Assert.That(headerBlockBuilder.NumberOfPointRecords).IsEqualTo(1U);
        await Assert.That(headerBlockBuilder.NumberOfPointsByReturn).Satisfies(x => x.ElementAt(returnNumber - 1), x => x.IsEqualTo(1U));
#endif
        await Assert.That(headerBlockBuilder.Min).IsEqualTo(new(X, Y, Z));
        await Assert.That(headerBlockBuilder.Max).IsEqualTo(new(X, Y, Z));
    }

    [Test]
    [MethodDataSource(nameof(GetReturnNumbers))]
    public async Task Add(int returnNumber, bool checkHeader)
    {
        HeaderBlockBuilder headerBlockBuilder = new() { PointDataFormatId = 6 };
        if (checkHeader)
        {
            await Assert.That(headerBlockBuilder.HeaderBlock.Min).IsEqualTo(default);
            await Assert.That(headerBlockBuilder.HeaderBlock.Max).IsEqualTo(default);
        }

        headerBlockBuilder.Add(X, Y, Z, returnNumber);
#if LAS1_4_OR_GREATER
        await Assert.That(headerBlockBuilder.NumberOfPointRecords).IsEqualTo(1UL);
        await Assert.That(headerBlockBuilder.NumberOfPointsByReturn).Satisfies(x => x.ElementAt(returnNumber - 1), static x => x.IsEqualTo(1UL));
#else
        await Assert.That(headerBlockBuilder.NumberOfPointRecords).IsEqualTo(1U);
        await Assert.That(headerBlockBuilder.NumberOfPointsByReturn).Satisfies(x => x.ElementAt(returnNumber - 1), static x => x.IsEqualTo(1U));
#endif
        await Assert.That(headerBlockBuilder.Min).IsEqualTo(new(X, Y, Z));
        await Assert.That(headerBlockBuilder.Max).IsEqualTo(new(X, Y, Z));
    }

    [Test]
    [MethodDataSource(nameof(GetDefaultValues))]
#if LAS1_2_OR_GREATER
    public async Task GetDefault(byte pointDataTypeId, GlobalEncoding globalEncoding)
#else

    public async Task GetDefault(byte pointDataTypeId)
#endif
    {
        HeaderBlockBuilder builder = new(pointDataTypeId);
        var header = builder.HeaderBlock;
        await Assert.That(header.FileSourceId).IsEqualTo((ushort)0);
        await Assert.That(header.ProjectId).IsEqualTo(Guid.Empty);
#if LAS1_2_OR_GREATER
        await Assert.That(header.GlobalEncoding).IsEqualTo(globalEncoding);
#endif
        await Assert.That(header.Version).IsEqualTo(HeaderBlock.DefaultVersion);
        await Assert.That(header.SystemIdentifier).IsNull();
        await Assert.That(header.GeneratingSoftware).IsNull();
        await Assert.That(header.PointDataFormatId).IsEqualTo(pointDataTypeId);
#if LAS1_4_OR_GREATER
        await Assert.That(header.LegacyNumberOfPointRecords).IsEqualTo(0U);
        await Assert.That(header.LegacyNumberOfPointsByReturn).IsEquivalentTo(Enumerable.Repeat(0U, 5));
#else
        await Assert.That(header.NumberOfPointRecords).IsEqualTo(0U);
        await Assert.That(header.NumberOfPointsByReturn).IsEquivalentTo(Enumerable.Repeat(0U, 5));
#endif
        await Assert.That(header.ScaleFactor).IsEqualTo(new(0.001));
        await Assert.That(header.Min).IsEqualTo(Vector3D.Zero);
        await Assert.That(header.Max).IsEqualTo(Vector3D.Zero);
        await Assert.That(header.Offset).IsEqualTo(Vector3D.Zero);
#if LAS1_4_OR_GREATER
        await Assert.That(header.NumberOfPointRecords).IsEqualTo(0UL);
        await Assert.That(header.NumberOfPointsByReturn).IsEquivalentTo(Enumerable.Repeat(0UL, 15));
#endif
    }

    [Test]
    [MethodDataSource(nameof(GetPointTypes))]
    public async Task CreateFromType(Type type, byte number)
    {
        _ = await Assert.That(typeof(HeaderBlockBuilder).GetMethod(nameof(HeaderBlockBuilder.FromPointType))!.MakeGenericMethod(type).Invoke(default, default)).IsTypeOf<HeaderBlockBuilder>().And.Satisfies(x => x.PointDataFormatId, pointDataFormatId => pointDataFormatId.IsEqualTo(number));
    }

    public static IEnumerable<Func<(int, bool)>> GetReturnNumbers()
    {
        foreach (var value in Enumerable.Range(1, MaxReturnNumbers))
        {
            yield return () => (value, true);
            yield return () => (value, false);
        }
    }

#if LAS1_2_OR_GREATER
    public static IEnumerable<Func<(byte, GlobalEncoding)>> GetDefaultValues()
    {
        yield return () => (GpsPointDataRecord.Id, GlobalEncoding.StandardGpsTime);
#if LAS1_4_OR_GREATER
        yield return () => (ExtendedGpsPointDataRecord.Id, GlobalEncoding.StandardGpsTime | GlobalEncoding.Wkt);
#endif
    }
#else
    public static IEnumerable<Func<byte>> GetDefaultValues()
    {
        yield return () => GpsPointDataRecord.Id;
    }
#endif

    public static IEnumerable<Func<(Type, byte)>> GetPointTypes()
    {
        yield return () => (typeof(PointDataRecord), PointDataRecord.Id);
        yield return () => (typeof(GpsPointDataRecord), GpsPointDataRecord.Id);
#if LAS1_2_OR_GREATER
        yield return () => (typeof(ColorPointDataRecord), ColorPointDataRecord.Id);
        yield return () => (typeof(GpsColorPointDataRecord), GpsColorPointDataRecord.Id);
#endif
#if LAS1_3_OR_GREATER
        yield return () => (typeof(GpsWaveformPointDataRecord), GpsWaveformPointDataRecord.Id);
        yield return () => (typeof(GpsColorWaveformPointDataRecord), GpsColorWaveformPointDataRecord.Id);
#endif
#if LAS1_4_OR_GREATER
        yield return () => (typeof(ExtendedGpsPointDataRecord), ExtendedGpsPointDataRecord.Id);
        yield return () => (typeof(ExtendedGpsColorPointDataRecord), ExtendedGpsColorPointDataRecord.Id);
        yield return () => (typeof(ExtendedGpsColorNearInfraredPointDataRecord), ExtendedGpsColorNearInfraredPointDataRecord.Id);
        yield return () => (typeof(ExtendedGpsWaveformPointDataRecord), ExtendedGpsWaveformPointDataRecord.Id);
        yield return () => (typeof(ExtendedGpsColorNearInfraredWaveformPointDataRecord), ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id);
#endif
    }
}