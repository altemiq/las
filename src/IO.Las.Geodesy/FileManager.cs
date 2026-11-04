// -----------------------------------------------------------------------
// <copyright file="FileManager.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <summary>
/// The file manager.
/// </summary>
internal static class FileManager
{
    private const string ProjData = "PROJ_DATA";

    private const string ProjLib = "PROJ_LIB";

    private static readonly string ProjDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "proj");

    /// <summary>
    /// Finds the <pre>proj.db</pre>.
    /// </summary>
    /// <returns>The path to <pre>proj.db</pre>.</returns>
    public static string FindProjDb() => FindResource("proj.db");

    private static string FindResource(string name) => GetResourcePaths(name).FirstOrDefault(File.Exists) ?? throw new FileNotFoundException($"Cannot find {name}", name);

    private static IEnumerable<string> GetResourcePaths(string name)
    {
        yield return ProjDirectory;

        if (Environment.GetEnvironmentVariable(ProjData) is { } projData)
        {
            foreach (var projDataPath in projData.Split(Path.PathSeparator))
            {
                yield return Path.Combine(projDataPath, name);
            }
        }

        if (Environment.GetEnvironmentVariable(ProjLib) is { } projLib)
        {
            if (Environment.GetEnvironmentVariable(ProjData) is null)
            {
                Console.Error.WriteLine($"{ProjLib} environment variable is deprecated, and will be removed in a future release. You are encouraged to set {ProjData} instead");
            }

            foreach (var projLibPath in projLib.Split(Path.PathSeparator))
            {
                yield return Path.Combine(projLibPath, name);
            }
        }

        yield return Path.Combine(Environment.CurrentDirectory, name);
        yield return Path.Combine(AppContext.BaseDirectory, name);
    }
}