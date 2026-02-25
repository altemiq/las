namespace Altemiq.IO.Las.Compression.DataFrame;

using static Polars.CSharp.Polars;

public class LazyFrameTests
{
    [Test]
    public async Task ScanLaz()
    {
        var stream = typeof(LazyFrameTests).Assembly.GetManifestResourceStream(typeof(LazyFrameTests), "fusa_height_7.laz")
                                 ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        await using LazReader reader = new(stream);

        var lazy = Polars.CSharp.LazyFrame.ScanLaz(reader);

        await Assert.That(lazy).IsNotNull();

        await Assert.That(await lazy.Filter(Col(Constants.Columns.ReturnNumber) == 3).CollectAsync())
            .IsNotNull().And
            .Member(static m => ((Polars.CSharp.DataFrame)m).Height, static height => height.IsEqualTo(281));
    }
}