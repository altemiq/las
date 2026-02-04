namespace Altemiq.IO.Las.DataFrame;

using static Polars.CSharp.Polars;

public class LazyFrameTests
{
    [Test]
    public async Task ScanLas()
    {
        var stream = typeof(LazyFrameTests).Assembly.GetManifestResourceStream(typeof(LazyFrameTests), "fusa.las")
                                 ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        await using LasReader reader = new(stream);

        var lazy = Polars.CSharp.LazyFrame.ScanLas(reader);

        await Assert.That(lazy).IsNotNull();

        await Assert.That(await lazy.Filter(Col(Constants.Columns.ReturnNumber) == 3).CollectAsync())
            .IsNotNull().And
            .Member(static m => ((Polars.CSharp.DataFrame)m).Height, static height => height.IsEqualTo(281));
    }

    [Test]
    public async Task WriteTo()
    {
        var stream = typeof(DataFrameTests).Assembly.GetManifestResourceStream(typeof(DataFrameTests), "fusa.las")
                     ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        await using LasReader reader = new(stream);

        var data = Polars.CSharp.LazyFrame.ScanLas(reader);

        IBasePointDataRecord record = default;
        data.SinkTo(arrowReader =>
        {
            record = arrowReader.ReadPointDataRecord().PointDataRecord;
        });

        await Assert.That(record).IsNotNull();
    }
}