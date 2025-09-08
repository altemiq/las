// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsColorNearInfraredWaveformPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> reader.
/// </summary>
internal sealed class ExtendedGpsColorNearInfraredWaveformPointDataRecordReader() : PointDataRecordReader<ExtendedGpsColorNearInfraredWaveformPointDataRecord>(ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size)
{
    /// <inheritdoc />
    public override ExtendedGpsColorNearInfraredWaveformPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsColorNearInfraredWaveformPointDataRecord.Create(source);
}