// -----------------------------------------------------------------------
// <copyright file="LasReaderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable RCS1263, SA1101

/// <summary>
/// <see cref="ILasReader"/> extensions.
/// </summary>
public static class LasReaderExtensions
{
    /// <content>
    /// <see cref="ILasReader"/> extensions.
    /// </content>
    /// <param name="reader">The reader.</param>
    extension(ILasReader reader)
    {
        /// <summary>
        /// From <see cref="ILasReader"/> infer <see cref="Apache.Arrow"/> <see cref="Apache.Arrow.Schema"/>.
        /// </summary>
        public Schema GetArrowSchema() => Arrow.LasToArrowStream.GetArrowSchema(reader);

        /// <summary>
        /// Convert <see cref="ILasReader"/> to <see cref="Apache.Arrow"/> <see cref="Apache.Arrow.RecordBatch"/> Stream.
        /// </summary>
        /// <param name="batchSize">The batch size.</param>
        public IEnumerable<RecordBatch> ToArrowBatches(int batchSize = 50_000) => Arrow.LasToArrowStream.ToArrowBatches(reader, Arrow.LasToArrowStream.GetArrowSchema(reader), batchSize);

        /// <summary>
        /// Convert <see cref="ILasReader"/> to <see cref="Apache.Arrow"/> <see cref="Apache.Arrow.RecordBatch"/> Stream.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="batchSize">The batch size.</param>
        public IEnumerable<RecordBatch> ToArrowBatches(Schema schema, int batchSize = 50_000) => Arrow.LasToArrowStream.ToArrowBatches(reader, schema, batchSize);
    }
}