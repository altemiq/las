// -----------------------------------------------------------------------
// <copyright file="LazReaderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable RCS1263, SA1101

/// <summary>
/// <see cref="ILazReader"/> extensions.
/// </summary>
public static class LazReaderExtensions
{
    /// <content>
    /// <see cref="ILazReader"/> extensions.
    /// </content>
    /// <param name="reader">The reader.</param>
    extension<T>(T reader)
        where T : ILazReader, ILasReader
    {
        /// <summary>
        /// Convert <see cref="ILasReader"/> to <see cref="Apache.Arrow"/> <see cref="Apache.Arrow.RecordBatch"/> Stream.
        /// </summary>
        public IEnumerable<RecordBatch> ToArrowBatches() => reader.ToArrowBatches(reader.GetArrowSchema());

        /// <summary>
        /// Convert <see cref="ILasReader"/> to <see cref="Apache.Arrow"/> <see cref="Apache.Arrow.RecordBatch"/> Stream.
        /// </summary>
        public IEnumerable<RecordBatch> ToArrowBatches(Schema schema) => LazToArrowStream.ToArrowBatches(reader, schema);
    }
}