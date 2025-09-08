// -----------------------------------------------------------------------
// <copyright file="ColorPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="ColorPointDataRecord"/> reader.
/// </summary>
internal sealed class ColorPointDataRecordReader() : PointDataRecordReader<ColorPointDataRecord>(ColorPointDataRecord.Size)
{
    /// <inheritdoc />
    public override ColorPointDataRecord Read(ReadOnlySpan<byte> source) => ColorPointDataRecord.Create(source);
}