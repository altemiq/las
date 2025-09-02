// -----------------------------------------------------------------------
// <copyright file="IPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers;

/// <summary>
/// The <see cref="IBasePointDataRecord"/> reader.
/// </summary>
public interface IPointDataRecordReader
{
    /// <summary>
    /// Reads the point data record.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The point data record.</returns>
    IBasePointDataRecord Read(ReadOnlySpan<byte> source);
}