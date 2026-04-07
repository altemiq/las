namespace Altemiq.IO.Las.CodeGeneration;

using Microsoft.CodeAnalysis;

public class ModelSourceGeneratorTests
{
    [Test]
    public async Task TestCaching()
    {
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            "TestProject",
            Helpers.GetSyntaxTrees("IO.Las", "Model.cs"),
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ModelSourceGenerator();
        var sourceGenerator = generator.AsSourceGenerator();

        // trackIncrementalGeneratorSteps allows to report info about each step of the generator
        GeneratorDriver driver = Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver.Create(
            generators: [sourceGenerator],
            driverOptions: new(default, trackIncrementalGeneratorSteps: true))
            .AddAdditionalTexts([ResourceAdditionalText.Create("models.csv")]);

        // Run the generator
        driver = driver.RunGenerators(compilation);

        // Ensure we generated something
        await Assert.That(driver.GetRunResult().Results.Single().GeneratedSources).IsNotEmpty();

        // Update the compilation and rerun the generator
        compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("// dummy"));
        driver = driver.RunGenerators(compilation);

        // Assert the driver doesn't recompute the output
        var result = driver.GetRunResult().Results.Single();
        var allOutputs = result
            .TrackedOutputSteps
            .SelectMany(outputStep => outputStep.Value)
            .SelectMany(output => output.Outputs);
        await Assert.That(allOutputs).All(output => output.Reason is IncrementalStepRunReason.Cached);
    }
}