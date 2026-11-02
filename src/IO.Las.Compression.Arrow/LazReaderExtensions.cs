// -----------------------------------------------------------------------
// <copyright file="LazReaderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// <see cref="ILazReader"/> extensions.
/// </summary>
public static class LazReaderExtensions
{
    /// <content>
    /// <see cref="ILazReader"/> extensions.
    /// </content>
    /// <typeparam name="T">The type of reader.</typeparam>
    /// <param name="reader">The reader.</param>
    extension<T>(T reader)
        where T : ILazReader, ILasReader
    {
        /// <summary>
        /// Convert <see cref="ILazReader"/> to <see cref="Apache.Arrow"/> <see cref="Apache.Arrow.RecordBatch"/> Stream.
        /// </summary>
        /// <returns>The record batches.</returns>
        public IEnumerable<RecordBatch> ToArrowBatches() => reader.ToArrowBatches(reader.GetArrowSchema());

        /// <summary>
        /// Convert <see cref="ILazReader"/> to <see cref="Apache.Arrow"/> <see cref="Apache.Arrow.RecordBatch"/> Stream.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns>The record batches.</returns>
        public IEnumerable<RecordBatch> ToArrowBatches(Schema schema) => Arrow.LazToArrowStream.ToArrowBatches(reader, schema);
    }
}