namespace Altemiq.IO.Las.DataFrame;

using static Polars.CSharp.Polars;

public class DataFrameTests
{
    [Test]
    [Arguments("fusa_0.las")]
#if LAS1_1_OR_GREATER
    [Arguments("fusa.las")]
#endif
    public async Task ReadLas(string name)
    {
        var stream = typeof(DataFrameTests).Assembly.GetManifestResourceStream(typeof(DataFrameTests), name)
                                 ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        await using LasReader reader = new(stream);

        var data = Polars.CSharp.DataFrame.ReadLas(reader);

        _ = await Assert.That(data).IsNotNull();

        _ = await Assert.That(data.Filter(Col(Arrow.Constants.Columns.ReturnNumber) == 3))
            .IsNotNull().And
            .Member(static m => ((Polars.CSharp.DataFrame)m).Height, static height => height.IsEqualTo(281));
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

        var data = Polars.CSharp.DataFrame.ReadLas(reader);

        IBasePointDataRecord record = default;
        data.WriteTo(arrowReader => record = arrowReader.ReadPointDataRecord().PointDataRecord);

        _ = await Assert.That(record).IsNotNull();
    }
}