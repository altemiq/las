// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="ExtendedGpsPointDataRecord"/> reader.
/// </summary>
public sealed class ExtendedGpsPointDataRecordReader : PointDataRecordReader<ExtendedGpsPointDataRecord>
{
    /// <inheritdoc />
    public override ExtendedGpsPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsPointDataRecord.Create(source);
}