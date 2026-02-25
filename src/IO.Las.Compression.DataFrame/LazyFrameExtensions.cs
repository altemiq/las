// -----------------------------------------------------------------------
// <copyright file="LazyFrameExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable SA1101

/// <summary>
/// The <see cref="LazyFrame"/> extensions.
/// </summary>
public static class LazyFrameExtensions
{
    extension(LazyFrame lazyFrame)
    {
        /// <summary>
        /// Creates a <see cref="LazyFrame"/> from the <see cref="ILasReader"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The lazy frame.</returns>
        public static LazyFrame ScanLaz<T>(T reader)
            where T : ILazReader, ILasReader
        {
            var schema = reader.GetArrowSchema().ToPolarsCompatibleSchema();

            if (reader.IsChunked)
            {
                // ensure we move to the first chunk
                reader.MoveToChunk(0);
            }

            var batches = reader.ToArrowBatches(schema);

            return LazyFrame.ScanRecordBatches(batches, schema);
        }
    }
}