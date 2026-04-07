namespace Altemiq.IO.Las.CodeGeneration;

internal static class Helpers
{
    public static IEnumerable<Microsoft.CodeAnalysis.SyntaxTree> GetSyntaxTrees(string project, params IEnumerable<string> files)
    {
        if (GetSourceDirectory() is { } sourceDirectory)
        {
            return files.Select(file =>
            {
                var path = Path.Combine(sourceDirectory, project, file);
                return Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path);
            });
        }

        return [];

        static string GetSourceDirectory()
        {
            var current = typeof(Helpers).Assembly.Location;

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