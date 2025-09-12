// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="ExtendedGpsPointDataRecord"/> reader.
/// </summary>
internal sealed class ExtendedGpsPointDataRecordReader() : PointDataRecordReader<ExtendedGpsPointDataRecord>(ExtendedGpsPointDataRecord.Size)
{
    /// <inheritdoc />
    protected override ExtendedGpsPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsPointDataRecord.Create(source);
}