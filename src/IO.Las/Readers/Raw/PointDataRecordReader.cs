// -----------------------------------------------------------------------
// <copyright file="PointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="PointDataRecord"/> reader.
/// </summary>
internal sealed class PointDataRecordReader() : PointDataRecordReader<PointDataRecord>(PointDataRecord.Size)
{
    /// <inheritdoc />
    public override PointDataRecord Read(ReadOnlySpan<byte> source) => PointDataRecord.Create(source);
}