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
    /// Finds the <c>proj.db</c>.
    /// </summary>
    /// <returns>The path to <c>proj.db</c>.</returns>
    public static string FindProjDb() => FindResource("proj.db");

    /// <summary>
    /// Finds the resource.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <returns>The path to the file.</returns>
    /// <exception cref="FileNotFoundException">The resource was not found.</exception>
    public static string FindResource(string name) => GetResourcePaths(name).FirstOrDefault(File.Exists) ?? throw new FileNotFoundException($"Cannot find {name}");

    /// <summary>
    /// Gets the resource paths.
    /// </summary>
    /// <param name="name">The resource name.</param>
    /// <returns>The resource path.</returns>
    public static IEnumerable<string> GetResourcePaths(string name)
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