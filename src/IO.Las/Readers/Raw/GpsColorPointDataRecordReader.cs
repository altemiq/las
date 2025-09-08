// -----------------------------------------------------------------------
// <copyright file="GpsColorPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="GpsColorPointDataRecord"/> reader.
/// </summary>
internal sealed class GpsColorPointDataRecordReader() : PointDataRecordReader<GpsColorPointDataRecord>(GpsColorPointDataRecord.Size)
{
    /// <inheritdoc />
    public override GpsColorPointDataRecord Read(ReadOnlySpan<byte> source) => GpsColorPointDataRecord.Create(source);
}