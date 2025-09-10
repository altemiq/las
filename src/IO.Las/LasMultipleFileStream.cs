// -----------------------------------------------------------------------
// <copyright file="LasMultipleFileStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The LAS <see cref="FileStream"/> <see cref="MultipleStream"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Mezianou", "MA0053:Make class sealed", Justification = "This is used in other projects.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required for automated cleanup")]
internal class LasMultipleFileStream : MultipleStream
{
    /// <summary>
    /// The file extension.
    /// </summary>
    public const string Extension = ".dat";

    /// <summary>
    /// The header file name.
    /// </summary>
    public const string HeaderFileName = LasStreams.Header + Extension;

    /// <summary>
    /// The variable length record file name.
    /// </summary>
    public const string VariableLengthRecordFileName = LasStreams.VariableLengthRecord + Extension;

#if LAS1_3_OR_GREATER
    /// <summary>
    /// The extended variable length record file name.
    /// </summary>
    public const string ExtendedVariableLengthRecordFileName = LasStreams.ExtendedVariableLengthRecord + Extension;
#endif

    /// <summary>
    /// The point data file name.
    /// </summary>
    public const string PointDataFileName = LasStreams.PointData + Extension;

    private readonly string baseDirectory;

    private readonly FileMode fileMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasMultipleFileStream"/> class.
    /// </summary>
    /// <param name="directory">The directory.</param>
    /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
    public LasMultipleFileStream(string directory, FileMode mode)
        : this(directory, mode, load: true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasMultipleFileStream"/> class.
    /// </summary>
    /// <param name="directory">The directory.</param>
    /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
    /// <param name="load">Set to <see langword="true"/> to load files in the directory; otherwise <see langword="false"/>.</param>
    protected LasMultipleFileStream(string directory, FileMode mode, bool load)
        : this(LasStreams.Comparer, directory, mode, load)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasMultipleFileStream"/> class.
    /// </summary>
    /// <param name="comparer">The <see cref="IComparer{T}"/> implementation to use when comparing keys.</param>
    /// <param name="directory">The directory.</param>
    /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
    /// <param name="load">Set to <see langword="true"/> to load files in the directory; otherwise <see langword="false"/>.</param>
    protected LasMultipleFileStream(IComparer<string> comparer, string directory, FileMode mode, bool load)
        : this(new LasStreamDictionary(comparer), directory, mode, load)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasMultipleFileStream"/> class.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="directory">The directory.</param>
    /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
    /// <param name="load">Set to <see langword="true"/> to load files in the directory; otherwise <see langword="false"/>.</param>
    protected LasMultipleFileStream(IDictionary<string, Stream> dictionary, string directory, FileMode mode, bool load)
        : base(dictionary)
    {
        (this.baseDirectory, this.fileMode) = (directory, mode);

        if (load)
        {
            this.AddIfExists(LasStreams.Header, HeaderFileName);
            this.AddIfExists(LasStreams.VariableLengthRecord, VariableLengthRecordFileName);
            this.AddIfExists(LasStreams.PointData, PointDataFileName);
#if LAS1_3_OR_GREATER
            this.AddIfExists(LasStreams.ExtendedVariableLengthRecord, ExtendedVariableLengthRecordFileName);
#endif
        }
    }

    /// <summary>
    /// Opens the <see cref="LasMultipleFileStream"/> for with read/write access with no sharing.
    /// </summary>
    /// <param name="directory">The directory containing the files.</param>
    /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
    /// <returns>The <see cref="LasMultipleFileStream"/> representing <paramref name="directory"/>.</returns>
    public static LasMultipleFileStream Open(string directory, FileMode mode) => new(directory, mode);

    /// <summary>
    /// Opens the <see cref="LasMultipleFileStream"/> for reading.
    /// </summary>
    /// <param name="directory">The directory containing the files.</param>
    /// <returns>The <see cref="LasMultipleFileStream"/> representing <paramref name="directory"/>.</returns>
    public static LasMultipleFileStream OpenRead(string directory) => new(directory, FileMode.Open);

    /// <summary>
    /// Opens the <see cref="LasMultipleFileStream"/> for writing.
    /// </summary>
    /// <param name="directory">The directory containing the files.</param>
    /// <returns>The <see cref="LasMultipleFileStream"/> representing <paramref name="directory"/>.</returns>
    public static LasMultipleFileStream OpenWrite(string directory)
    {
        // clean the current directory
        DeleteIfExists(directory, HeaderFileName, VariableLengthRecordFileName, PointDataFileName);
#if LAS1_3_OR_GREATER
        DeleteIfExists(directory, ExtendedVariableLengthRecordFileName);
#endif
        return new(directory, FileMode.Create);

        static void DeleteIfExists(string directory, params string[] names)
        {
            foreach (var name in names)
            {
                var path = Path.Combine(directory, name);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }

    /// <inheritdoc/>
    protected sealed override Stream CreateStream(string name) => File.Open(Path.Combine(this.baseDirectory, name + Extension), this.fileMode);

    /// <summary>
    /// Adds the file if it exists.
    /// </summary>
    /// <param name="name">The name of the stream.</param>
    /// <param name="fileName">The file name.</param>
    protected void AddIfExists(string name, string fileName) => this.AddIfExists(name, this.baseDirectory, fileName, this.fileMode);

    /// <summary>
    /// Adds the file if it exists.
    /// </summary>
    /// <param name="name">The name of the stream.</param>
    /// <param name="directory">The directory.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="mode">The mode.</param>
    protected void AddIfExists(string name, string directory, string fileName, FileMode mode)
    {
        var path = Path.Combine(directory, fileName);
        if (File.Exists(path))
        {
            this.Add(name, File.Open(path, mode));
        }
    }
}