// -----------------------------------------------------------------------
// <copyright file="ColorPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="ColorPointDataRecord"/> reader.
/// </summary>
public sealed class ColorPointDataRecordReader : PointDataRecordReader<ColorPointDataRecord>
{
    /// <inheritdoc />
    public override ColorPointDataRecord Read(ReadOnlySpan<byte> source) => ColorPointDataRecord.Create(source);
}