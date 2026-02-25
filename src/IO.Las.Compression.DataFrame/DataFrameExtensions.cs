// -----------------------------------------------------------------------
// <copyright file="DataFrameExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable SA1101

/// <summary>
/// <see cref="DataFrame"/> extensions.
/// </summary>
public static class DataFrameExtensions
{
    extension(DataFrame)
    {
        /// <summary>
        /// Creates a <see cref="DataFrame"/> from the <see cref="ILasReader"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The data frame.</returns>
        public static DataFrame ReadLaz<T>(T reader)
            where T : ILazReader, ILasReader
        {
            var schema = reader.GetArrowSchema().ToPolarsCompatibleSchema();

            var batches = reader.ToArrowBatches(schema);

            return DataFrame.ReadRecordBatches(batches, schema);
        }
    }
}