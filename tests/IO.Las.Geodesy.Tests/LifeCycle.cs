using System.Reflection;

namespace Altemiq.IO.Las.Geodesy;

public static class LifeCycle
{
    public static readonly string TestLocation = Path.GetTempFileName();
    
    [Before(HookType.Assembly)]
    public static void Setup(AssemblyHookContext context)
    {
        using var manifestStream = typeof(ProjContextTests).Assembly.GetManifestResourceStream(typeof(ProjContextTests), "proj.db")
                                   ?? throw new InvalidOperationException();
        using var fileStream = File.OpenWrite(TestLocation);
        
        manifestStream.CopyTo(fileStream);
    }

    [After(HookType.Assembly)]
    public static void Close(AssemblyHookContext context)
    {
        File.Delete(TestLocation);
    }
}