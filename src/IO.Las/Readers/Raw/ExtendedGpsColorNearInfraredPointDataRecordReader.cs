// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsColorNearInfraredPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Raw;

/// <summary>
/// The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> reader.
/// </summary>
internal sealed class ExtendedGpsColorNearInfraredPointDataRecordReader : PointDataRecordReader<ExtendedGpsColorNearInfraredPointDataRecord>
{
    /// <inheritdoc />
    public override ExtendedGpsColorNearInfraredPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsColorNearInfraredPointDataRecord.Create(source);
}