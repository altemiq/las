namespace Altemiq.IO.Las.DataFrame;

using static Polars.CSharp.Polars;

public class LazyFrameTests
{
    [Test]
    [Arguments("fusa_0.las")]
#if LAS1_1_OR_GREATER
    [Arguments("fusa.las")]
#endif
    public async Task ScanLas(string name)
    {
        var stream = typeof(LazyFrameTests).Assembly.GetManifestResourceStream(typeof(LazyFrameTests), name)
                                 ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        await using LasReader reader = new(stream);

        var lazy = Polars.CSharp.LazyFrame.ScanLas(reader);

        _ = await Assert.That(lazy).IsNotNull();

        _ = await Assert.That(lazy.Filter(Col(Arrow.Constants.Columns.ReturnNumber) == 3).CollectAsync())
            .IsTypeOf<Polars.CSharp.DataFrame>().And
            .Member(static m => m.Height, static height => height.IsEqualTo(281));
    }

    [Test]
    [Arguments("fusa_0.las")]
#if LAS1_1_OR_GREATER
    [Arguments("fusa.las")]
#endif
    public async Task WriteTo(string name)
    {
        var stream = typeof(DataFrameTests).Assembly.GetManifestResourceStream(typeof(DataFrameTests), name)
                     ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        await using LasReader reader = new(stream);

        var data = Polars.CSharp.LazyFrame.ScanLas(reader);

        IBasePointDataRecord record = default;
        data.SinkTo(arrowReader => record = arrowReader.ReadPointDataRecord().PointDataRecord);

        _ = await Assert.That(record).IsNotNull();
    }
}