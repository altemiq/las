// -----------------------------------------------------------------------
// <copyright file="PointDataRecordReader{T}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers;

/// <summary>
/// The point data record reader.
/// </summary>
/// <typeparam name="T">The type of point.</typeparam>
public abstract class PointDataRecordReader<T> : IPointDataRecordReader
    where T : IBasePointDataRecord
{
    /// <inheritdoc/>
    IBasePointDataRecord IPointDataRecordReader.Read(ReadOnlySpan<byte> source) => this.Read(source);

    /// <inheritdoc cref="IPointDataRecordReader.Read"/>
    public abstract T Read(ReadOnlySpan<byte> source);
}