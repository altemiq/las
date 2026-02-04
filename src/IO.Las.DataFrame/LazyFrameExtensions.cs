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
        /// <param name="batchSize">The batch size.</param>
        /// <returns>The lazy frame.</returns>
        public static LazyFrame ScanLas(ILasReader reader, int batchSize = 50_000)
        {
            var schema = reader.GetArrowSchema();

            var batches = reader.ToArrowBatches(schema, batchSize);

            return LazyFrame.ScanRecordBatches(batches, schema);
        }

        /// <summary>
        /// Generic streaming Sink interface: Streamingly convert LazyFrame calculation results to <see cref="ILasReader"/> and hand it over to <paramref name="writerAction"/> writerAction for processing.
        /// </summary>
        /// <param name="writerAction">Callback that receives <see cref="ILasReader"/> (executed in a separate thread).</param>
        /// <param name="bufferSize">Buffer size (number of Batches).</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "This is disposed after await for the task")]
        public void SinkTo(Action<ILasReader> writerAction, int bufferSize = 5)
        {
            using var buffer = new System.Collections.Concurrent.BlockingCollection<Apache.Arrow.RecordBatch>(boundedCapacity: bufferSize);

            var consumerTask = Task.Run(() =>
            {
                using var reader = new ArrowLasReader(buffer.GetConsumingEnumerable());

                writerAction(reader);
            });

            try
            {
                lazyFrame.SinkBatches(buffer.Add);
            }
            finally
            {
                buffer.CompleteAdding();
            }

            consumerTask.Wait();
        }
    }
}