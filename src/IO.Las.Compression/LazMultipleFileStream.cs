// -----------------------------------------------------------------------
// <copyright file="LazMultipleFileStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The LAZ <see cref="FileStream"/> <see cref="MultipleStream"/>.
/// </summary>
internal sealed class LazMultipleFileStream : LasMultipleFileStream
{
    /// <summary>
    /// The chunk table header name.
    /// </summary>
    public const string ChunkTablePositionFileName = LazStreams.ChunkTablePosition + Extension;

    /// <summary>
    /// The chunk table name.
    /// </summary>
    public const string ChunkTableFileName = LazStreams.ChunkTable + Extension;

    /// <summary>
    /// The chunk file glob.
    /// </summary>
    public const string ChunkFileGlob = LazStreams.Chunk + "_*" + Extension;

#if LAS1_3_OR_GREATER
    /// <summary>
    /// The special extended variable length record name.
    /// </summary>
    public const string SpecialExtendedVariableLengthRecordFileName = LazStreams.SpecialExtendedVariableLengthRecord + Extension;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="LazMultipleFileStream"/> class.
    /// </summary>
    /// <param name="directory">The directory.</param>
    /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
    public LazMultipleFileStream(string directory, FileMode mode)
        : base(LazStreams.Comparer, directory, mode, load: false)
    {
        this.AddIfExists(LasStreams.Header, HeaderFileName);
        this.AddIfExists(LasStreams.VariableLengthRecord, VariableLengthRecordFileName);
        this.AddIfExists(LazStreams.ChunkTablePosition, ChunkTablePositionFileName);
        this.AddIfExists(LasStreams.PointData, PointDataFileName);

        // open any chunk file
        foreach (var file in Directory
            .EnumerateFiles(directory, ChunkFileGlob)
            .OrderBy(static p => LazStreams.ParseChunkNumber(Path.GetFileNameWithoutExtension(p))))
        {
            this.AddIfExists(Path.GetFileNameWithoutExtension(file), file);
        }

        this.AddIfExists(LazStreams.ChunkTable, ChunkTableFileName);
#if LAS1_3_OR_GREATER
        this.AddIfExists(LasStreams.ExtendedVariableLengthRecord, ExtendedVariableLengthRecordFileName);
        this.AddIfExists(LasStreams.ExtendedVariableLengthRecord, SpecialExtendedVariableLengthRecordFileName);
#endif
    }

    /// <summary>
    /// Opens the <see cref="LazMultipleFileStream"/> for with read/write access with no sharing.
    /// </summary>
    /// <param name="directory">The directory containing the files.</param>
    /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
    /// <returns>The <see cref="LazMultipleFileStream"/> representing <paramref name="directory"/>.</returns>
    public static new LazMultipleFileStream Open(string directory, FileMode mode) => new(directory, mode);

    /// <summary>
    /// Opens the <see cref="LazMultipleFileStream"/> for reading.
    /// </summary>
    /// <param name="directory">The directory containing the files.</param>
    /// <returns>The <see cref="LazMultipleFileStream"/> representing <paramref name="directory"/>.</returns>
    public static new LazMultipleFileStream OpenRead(string directory) => new(directory, FileMode.Open);

    /// <summary>
    /// Opens the <see cref="LasMultipleFileStream"/> for writing.
    /// </summary>
    /// <param name="directory">The directory containing the files.</param>
    /// <returns>The <see cref="LasMultipleFileStream"/> representing <paramref name="directory"/>.</returns>
    public static new LazMultipleFileStream OpenWrite(string directory)
    {
        // clean the current directory
        DeleteIfExists(
            directory,
            HeaderFileName,
            VariableLengthRecordFileName,
            ChunkTablePositionFileName,
            PointDataFileName,
            ChunkTableFileName);
#if LAS1_3_OR_GREATER
        DeleteIfExists(
            directory,
            ExtendedVariableLengthRecordFileName,
            SpecialExtendedVariableLengthRecordFileName);
#endif
        foreach (var file in Directory.EnumerateFiles(directory, ChunkFileGlob))
        {
            File.Delete(file);
        }

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
}