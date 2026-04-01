namespace Altemiq.IO.Las.CodeGeneration;

using Microsoft.CodeAnalysis;

public class ColorSourceGeneratorTests
{
    [Test]
    public async Task TestCaching()
    {
        // get the source directory
        IEnumerable<SyntaxTree> syntaxTrees = [];
        if (GetSourceDirectory() is { } sourceDirectory)
        {
            var file = Path.Combine(sourceDirectory, "IO.Las", "Color.cs");
            syntaxTrees = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(await File.ReadAllTextAsync(file), path: file)];
        }

        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("TestProject",
            syntaxTrees,
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Drawing.Color).Assembly.Location),
            ],
            new(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ColorSourceGenerator();
        var sourceGenerator = generator.AsSourceGenerator();

        // trackIncrementalGeneratorSteps allows to report info about each step of the generator
        GeneratorDriver driver = Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver.Create(
                generators: [sourceGenerator],
                driverOptions: new(default, trackIncrementalGeneratorSteps: true));

        // Run the generator
        driver = driver.RunGenerators(compilation);

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

        static string GetSourceDirectory()
        {
            var current = typeof(ColorSourceGenerator).Assembly.Location;

            while (current is not null)
            {
                var test = Path.Combine(current, "src");
                if (Directory.Exists(test))
                {
                    return test;
                }

                current = Path.GetDirectoryName(current);
            }

            return default;
        }
    }
}