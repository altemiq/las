// -----------------------------------------------------------------------
// <copyright file="LazyFrameExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

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
        /// <typeparam name="T">The type of reader.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <returns>The lazy frame.</returns>
        public static LazyFrame ScanLaz<T>(T reader)
            where T : ILazReader, ILasReader
        {
            var schema = reader.GetArrowSchema().ToPolarsCompatibleSchema();

            var batches = reader.ToArrowBatches(schema);

            return LazyFrame.ScanRecordBatches(batches, schema);
        }
    }
}