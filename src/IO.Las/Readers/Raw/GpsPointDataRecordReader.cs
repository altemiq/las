// -----------------------------------------------------------------------
// <copyright file="GpsPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="GpsPointDataRecord"/> reader.
/// </summary>
internal sealed class GpsPointDataRecordReader() : PointDataRecordReader<GpsPointDataRecord>(GpsPointDataRecord.Size)
{
    /// <inheritdoc />
    public override GpsPointDataRecord Read(ReadOnlySpan<byte> source) => GpsPointDataRecord.Create(source);
}