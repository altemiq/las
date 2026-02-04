namespace Altemiq.IO.Las.DataFrame;

using static Polars.CSharp.Polars;

public class DataFrameTests
{
    [Test]
    public async Task ReadLas()
    {
        var stream = typeof(DataFrameTests).Assembly.GetManifestResourceStream(typeof(DataFrameTests), "fusa.las")
                                 ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        
        await using LasReader reader = new(stream);
        
        var data = Polars.CSharp.DataFrame.ReadLas(reader);

        await Assert.That(data).IsNotNull();
        
        await Assert.That(data.Filter(Col(Constants.Columns.ReturnNumber) == 3))
            .IsNotNull().And
            .Member(static m => ((Polars.CSharp.DataFrame)m).Height, static height => height.IsEqualTo(281));
    }
}