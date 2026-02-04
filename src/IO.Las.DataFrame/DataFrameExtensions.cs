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
    extension(DataFrame dataFrame)
    {
        /// <summary>
        /// Creates a <see cref="DataFrame"/> from the <see cref="ILasReader"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="batchSize">The batch size.</param>
        /// <returns>The data frame.</returns>
        public static DataFrame ReadLas(ILasReader reader, int batchSize = 50_000)
        {
            var schema = reader.GetArrowSchema();

            var batchEnumerable = reader.ToArrowBatches(schema, batchSize);

            var enumerator = batchEnumerable.GetEnumerator();

            try
            {
                var handle = Polars.NET.Core.Arrow.ArrowStreamInterop.ImportEager(enumerator, schema);
                return NewDataFrame(handle);
            }
            catch
            {
                enumerator.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Common Write Interface:Transform DataFrame to <see cref="ILasReader"/>.
        /// </summary>
        /// <param name="writerAction">Callback that receives <see cref="ILasReader"/> (executed in a separate thread).</param>
        /// <param name="bufferSize">Buffer size (number of Batches).</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "This is disposed after await for the task")]
        public void WriteTo(Action<ILasReader> writerAction, int bufferSize = 5)
        {
            using var buffer = new System.Collections.Concurrent.BlockingCollection<Apache.Arrow.RecordBatch>(bufferSize);

            var consumerTask = Task.Run(() =>
            {
                using var reader = new ArrowLasReader(buffer.GetConsumingEnumerable());

                writerAction(reader);
            });

            try
            {
                dataFrame.ExportBatches(buffer.Add);
            }
            finally
            {
                buffer.CompleteAdding();
            }

            consumerTask.Wait();
        }
    }

    [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Constructor)]
    private static extern DataFrame NewDataFrame(Polars.NET.Core.DataFrameHandle handle);
}