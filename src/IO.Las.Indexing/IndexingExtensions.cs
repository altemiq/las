// -----------------------------------------------------------------------
// <copyright file="IndexingExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable IDE0130, CheckNamespace
namespace Altemiq.IO.Las;
#pragma warning restore IDE0130, CheckNamespace

#pragma warning disable CA1708, RCS1263

/// <summary>
/// <see cref="Indexing"/> extensions.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "False positive")]
public static class IndexingExtensions
{
#if LAS1_4_OR_GREATER
    /// <content>
    /// The <see cref="VariableLengthRecordProcessor"/> extensions.
    /// </content>
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

    /// <content>
    /// The <see cref="LasReader"/> extensions.
    /// </content>
    /// <param name="reader">The reader.</param>
    extension(LasReader reader)
    {
        /// <summary>
        /// Reads the point data records within the specified box, using the index.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <param name="box">The bounding box.</param>
        /// <returns>The point data records within <paramref name="box"/>.</returns>
        public IEnumerable<IBasePointDataRecord> ReadPointDataRecords(Indexing.LasIndex index, BoundingBox box)
        {
            var header = reader.Header;
            var quantizer = new PointDataRecordQuantizer(header);
            return index
                .GetPointDataRecordIndexes(box)
                .Select(idx => reader.ReadPointDataRecord(idx).PointDataRecord!)
                .Where(point =>
                {
                    var (x, y, z) = quantizer.Get(point);
                    return box.Contains(x, y, z);
                });
        }
    }

    /// <content>
    /// The <see cref="Indexing.LasIndex"/> extensions.
    /// </content>
    /// <param name="index">The index to get the indexes from.</param>
    extension(Indexing.LasIndex index)
    {
        /// <summary>
        /// Gets the point indexes from the index.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns>The point indexes within <paramref name="box"/>.</returns>
        public IEnumerable<uint> GetPointDataRecordIndexes(BoundingBox box) => index.WithinRectangle((float)box.Left, (float)box.Bottom, (float)box.Right, (float)box.Top).SelectMany(Indexing.RangeExtensions.GetIndexes);
    }
}