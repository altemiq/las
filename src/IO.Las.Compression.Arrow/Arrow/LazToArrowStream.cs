// -----------------------------------------------------------------------
// <copyright file="LazToArrowStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Arrow;

/// <summary>
/// The <see cref="ILazReader"/> to <see cref="Apache.Arrow"/> Stream.
/// </summary>
internal static class LazToArrowStream
{
    /// <summary>
    /// Converts the <see cref="ILazReader"/> into <see cref="RecordBatch"/> instances.
    /// </summary>
    /// <typeparam name="T">The type of reader.</typeparam>
    /// <param name="reader">The reader.</param>
    /// <param name="schema">The schema.</param>
    /// <returns>The <see cref="RecordBatch"/> instances.</returns>
    /// <exception cref="NotSupportedException"><paramref name="reader"/> is not supported.</exception>
    internal static IEnumerable<RecordBatch> ToArrowBatches<T>(T reader, Schema schema)
        where T : ILazReader => reader switch
        {
            { IsChunked: true } => new RecordBatchEnumerable(reader, schema),
            ILasReader lasReader => lasReader.ToArrowBatches(),
            _ => throw new NotSupportedException(),
        };

    private sealed class RecordBatchEnumerable(ILazReader reader, Schema schema) : IEnumerable<RecordBatch>, IEnumerator<RecordBatch>
    {
        private readonly IReadOnlyList<ColumnBuffer> buffers = [.. schema.FieldsList.Select(static f => ColumnBuffer.Create(f.DataType.TypeId, capacity: 50_000))];

        [System.Diagnostics.CodeAnalysis.AllowNull]
        public RecordBatch Current { get; private set; }

        [System.Diagnostics.CodeAnalysis.AllowNull]
        object System.Collections.IEnumerator.Current => this.Current;

        public IEnumerator<RecordBatch> GetEnumerator() => this;

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(System.Collections.IEnumerator.Current))]
        public bool MoveNext()
        {
            var rowCount = 0;
            var enumerator = reader.ReadChunk().GetEnumerator();
            while (enumerator.MoveNext() && enumerator.Current.PointDataRecord is { } pointDataRecord)
            {
                LasToArrowStream.AddPointDataRecordToBuffers(pointDataRecord, this.buffers);
                rowCount++;
            }

            if (rowCount is 0)
            {
                this.Current = default;
                return false;
            }

            this.Current = LasToArrowStream.FlushToRecordBatch(this.buffers, schema, rowCount);
            return true;
        }

        public void Reset()
        {
        }

        public void Dispose()
        {
        }
    }
}