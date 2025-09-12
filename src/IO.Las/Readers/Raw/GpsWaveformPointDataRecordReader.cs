// -----------------------------------------------------------------------
// <copyright file="GpsWaveformPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="GpsWaveformPointDataRecord"/> reader.
/// </summary>
internal sealed class GpsWaveformPointDataRecordReader() : PointDataRecordReader<GpsWaveformPointDataRecord>(GpsWaveformPointDataRecord.Size)
{
    /// <inheritdoc />
    protected override GpsWaveformPointDataRecord Read(ReadOnlySpan<byte> source) => GpsWaveformPointDataRecord.Create(source);
}