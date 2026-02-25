// -----------------------------------------------------------------------
// <copyright file="LazToArrowStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using System.Collections;

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
    internal static IEnumerable<RecordBatch> ToArrowBatches<T>(T reader, Schema schema)
        where T : ILazReader
    {
        if (reader.IsChunked)
        {
            return new RecordBatchEnumerable(reader, schema);
        }

        if (reader is ILasReader lasReader)
        {
            return lasReader.ToArrowBatches(schema);
        }

        throw new NotSupportedException();
    }

    private sealed class RecordBatchEnumerable(ILazReader reader, Schema schema) : IEnumerable<RecordBatch>, IEnumerator<RecordBatch>
    {
        private readonly IReadOnlyList<ColumnBuffer> buffers = [.. schema.FieldsList.Select(f => ColumnBuffer.Create(f.DataType.TypeId, capacity: 50_000))];

        private RecordBatch? current;

        public RecordBatch Current => this.current!;

        object IEnumerator.Current => this.Current;

        public IEnumerator<RecordBatch> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(this.Current))]
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
                this.current = default;
                return false;
            }

            this.current = LasToArrowStream.FlushToRecordBatch(this.buffers, schema, rowCount);
#pragma warning disable CS8775 // Member must have a non-null value when exiting in some condition.
            return true;
#pragma warning restore CS8775 // Member must have a non-null value when exiting in some condition.
        }

        public void Reset()
        {
        }

        public void Dispose()
        {
        }
    }
}