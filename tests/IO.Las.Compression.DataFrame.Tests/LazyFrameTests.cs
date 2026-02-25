namespace Altemiq.IO.Las.Compression.DataFrame;

using static Polars.CSharp.Polars;

public class LazyFrameTests
{
    [Test]
    [Arguments("fusa.laz")]
#if LAS1_4_OR_GREATER
    [Arguments("fusa_height.laz")]
    [Arguments("fusa_height_7.laz")]
#endif
    public async Task ScanLaz(string filename)
    {
        var stream = typeof(LazyFrameTests).Assembly.GetManifestResourceStream(typeof(LazyFrameTests), filename)
                                 ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        await using LazReader reader = new(stream);

        var lazy = Polars.CSharp.LazyFrame.ScanLaz(reader);

        await Assert.That(lazy).IsNotNull();

        await Assert.That(await lazy.Filter(Col(Constants.Columns.ReturnNumber) == 3).CollectAsync())
            .IsNotNull().And
            .Member(static m => ((Polars.CSharp.DataFrame)m).Height, static height => height.IsEqualTo(281));
    }
}