// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsColorPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="ExtendedGpsColorPointDataRecord"/> reader.
/// </summary>
internal sealed class ExtendedGpsColorPointDataRecordReader() : PointDataRecordReader<ExtendedGpsColorPointDataRecord>(ExtendedGpsColorPointDataRecord.Size)
{
    /// <inheritdoc />
    protected override ExtendedGpsColorPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsColorPointDataRecord.Create(source);
}