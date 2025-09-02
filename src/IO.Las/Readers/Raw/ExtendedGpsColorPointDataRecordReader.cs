// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsColorPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="ExtendedGpsColorPointDataRecord"/> reader.
/// </summary>
public sealed class ExtendedGpsColorPointDataRecordReader : PointDataRecordReader<ExtendedGpsColorPointDataRecord>
{
    /// <inheritdoc />
    public override ExtendedGpsColorPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsColorPointDataRecord.Create(source);
}