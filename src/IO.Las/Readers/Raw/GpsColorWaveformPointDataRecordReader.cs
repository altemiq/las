// -----------------------------------------------------------------------
// <copyright file="GpsColorWaveformPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="GpsColorWaveformPointDataRecord"/> reader.
/// </summary>
internal sealed class GpsColorWaveformPointDataRecordReader() : PointDataRecordReader<GpsColorWaveformPointDataRecord>(GpsColorWaveformPointDataRecord.Size)
{
    /// <inheritdoc />
    public override GpsColorWaveformPointDataRecord Read(ReadOnlySpan<byte> source) => GpsColorWaveformPointDataRecord.Create(source);
}