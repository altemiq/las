// -----------------------------------------------------------------------
// <copyright file="IndexingExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace Altemiq.IO.Las;
#pragma warning restore IDE0130, CheckNamespace

/// <summary>
/// <see cref="Indexing"/> extensions.
/// </summary>
public static class IndexingExtensions
{
#if LAS1_4_OR_GREATER
    /// <summary>
    /// Registers index VLRs.
    /// </summary>
    /// <param name="processor">The VLR processor.</param>
    public static void RegisterIndexing(this VariableLengthRecordProcessor processor) => processor.Register(Indexing.LaxTag.TagRecordId, ProcessLaxTag);

    /// <summary>
    /// Registers index VLRs.
    /// </summary>
    /// <param name="processor">The VLR processor.</param>
    /// <returns><see langword="true" /> when the compression processors are successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processors, in which case nothing gets added.</returns>
    public static bool TryRegisterIndexing(this VariableLengthRecordProcessor processor) => processor.TryRegister(Indexing.LaxTag.TagRecordId, ProcessLaxTag);
#endif

    /// <summary>
    /// Reads the point data records within the specified box, using the index.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index to use.</param>
    /// <param name="box">The bounding box.</param>
    /// <returns>The point data records within <paramref name="box"/>.</returns>
    public static IEnumerable<IBasePointDataRecord> ReadPointDataRecords(this LasReader reader, Indexing.LasIndex index, BoundingBox box)
    {
        var header = reader.Header;
        var quantizer = new PointDataRecordQuantizer(header);
        return GetPointDataRecordIndexes(index, box)
            .Select(idx => reader.ReadPointDataRecord(idx).PointDataRecord!)
            .Where(point =>
            {
                var value = quantizer.GetX(point!.X);
                if (box.Left > value || box.Right < value)
                {
                    return false;
                }

                value = quantizer.GetY(point.Y);
                if (box.Bottom > value || box.Top < value)
                {
                    return false;
                }

                value = quantizer.GetZ(point.Z);
                return box.Front <= value && box.Back >= value;
            });
    }

    /// <summary>
    /// Gets the point indexes from the index.
    /// </summary>
    /// <param name="index">The index to get the indexes from.</param>
    /// <param name="box">The bounding box.</param>
    /// <returns>The point indexes within <paramref name="box"/>.</returns>
    public static IEnumerable<uint> GetPointDataRecordIndexes(this Indexing.LasIndex index, BoundingBox box) => index.WithinRectangle(box.Left, box.Bottom, box.Right, box.Top).SelectMany(Indexing.RangeExtensions.GetIndexes);

#if LAS1_4_OR_GREATER
    private static Indexing.LaxTag ProcessLaxTag(ExtendedVariableLengthRecordHeader header, IEnumerable<VariableLengthRecord> records, long position, ReadOnlySpan<byte> data) => new(header, data);
#endif
}