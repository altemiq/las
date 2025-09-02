// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsWaveformPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="ExtendedGpsWaveformPointDataRecord"/> reader.
/// </summary>
public sealed class ExtendedGpsWaveformPointDataRecordReader : PointDataRecordReader<ExtendedGpsWaveformPointDataRecord>
{
    /// <inheritdoc />
    public override ExtendedGpsWaveformPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsWaveformPointDataRecord.Create(source);
}