namespace Altemiq.IO.Las.Compression.DataFrame;

using static Polars.CSharp.Polars;

public class DataFrameTests
{
    [Test]
    [Arguments("fusa.laz")]
#if LAS1_4_OR_GREATER
    [Arguments("fusa_height.laz")]
    [Arguments("fusa_height_7.laz")]
#endif
    public async Task ReadLaz(string filename)
    {
        var stream = typeof(DataFrameTests).Assembly.GetManifestResourceStream(typeof(DataFrameTests), filename)
                                 ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        await using LazReader reader = new(stream);

        var data = Polars.CSharp.DataFrame.ReadLaz(reader);

        await Assert.That(data).IsNotNull();

        await Assert.That(data.Filter(Col(Constants.Columns.ReturnNumber) == 3))
            .IsNotNull().And
            .Member(static m => ((Polars.CSharp.DataFrame)m).Height, static height => height.IsEqualTo(281));
    }
}