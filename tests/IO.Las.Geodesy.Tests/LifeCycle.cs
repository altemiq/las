namespace Altemiq.IO.Las.Geodesy;

public static class LifeCycle
{
    public static string TestLocation { get; private set; }

    [Before(Assembly)]
    public static void Setup(AssemblyHookContext context)
    {
        using var manifestStream = context.Assembly.GetManifestResourceStream(typeof(ProjContextTests), "proj.db")
                                   ?? throw new System.Diagnostics.UnreachableException();

        TestLocation = Path.GetTempFileName();
        context.OutputWriter.WriteLine($"Writing temp proj.db to {TestLocation}");
        using var fileStream = File.OpenWrite(TestLocation);

        manifestStream.CopyTo(fileStream);
    }

    [After(Assembly)]
    public static void Close(AssemblyHookContext context)
    {
        if (TestLocation is null || !File.Exists(TestLocation))
        {
            return;
        }

        context.OutputWriter.WriteLine("Removing temp proj.db");
        File.Delete(TestLocation);
    }
}