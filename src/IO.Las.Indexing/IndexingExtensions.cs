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
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "False positive")]
public static class IndexingExtensions
{
#if LAS1_4_OR_GREATER
    /// <summary>
    /// The <see cref="VariableLengthRecordProcessor"/> extensions.
    /// </summary>
    extension(VariableLengthRecordProcessor processor)
    {
        /// <summary>
        /// Registers index VLRs.
        /// </summary>
        public void RegisterIndexing() => processor.Register(Indexing.LaxTag.TagRecordId, VariableLengthRecordProcessor.ProcessLaxTag);

        /// <summary>
        /// Registers index VLRs.
        /// </summary>
        /// <returns><see langword="true" /> when the compression processors are successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processors, in which case nothing gets added.</returns>
        public bool TryRegisterIndexing() => processor.TryRegister(Indexing.LaxTag.TagRecordId, VariableLengthRecordProcessor.ProcessLaxTag);

        private static Indexing.LaxTag ProcessLaxTag(ExtendedVariableLengthRecordHeader header, IEnumerable<VariableLengthRecord> records, long position, ReadOnlySpan<byte> data) => new(header, data);
    }
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
        return index
            .GetPointDataRecordIndexes(box)
            .Select(idx => reader.ReadPointDataRecord(idx).PointDataRecord!)
            .Where(point =>
            {
                var value = quantizer.GetX(point.X);
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
}