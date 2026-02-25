namespace Altemiq.IO.Las.Compression.DataFrame;

using static Polars.CSharp.Polars;

public class DataFrameTests
{
    [Test]
    public async Task ReadLaz()
    {
        var stream = typeof(DataFrameTests).Assembly.GetManifestResourceStream(typeof(DataFrameTests), "fusa_height_7.laz")
                                 ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        await using LazReader reader = new(stream);

        var data = Polars.CSharp.DataFrame.ReadLaz(reader);

        await Assert.That(data).IsNotNull();

        await Assert.That(data.Filter(Col(Constants.Columns.ReturnNumber) == 3))
            .IsNotNull().And
            .Member(static m => ((Polars.CSharp.DataFrame)m).Height, static height => height.IsEqualTo(281));
    }
}