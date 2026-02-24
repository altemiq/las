// -----------------------------------------------------------------------
// <copyright file="LasReaderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable MA0102, RCS1263, SA1101

/// <summary>
/// The <see cref="LasReader"/> extensions.
/// </summary>
public static class LasReaderExtensions
{
    /// <content>
    /// The <see cref="LasReader"/> extensions.
    /// </content>
    /// <param name="reader">The reader.</param>
    extension(LasReader reader)
    {
        /// <inheritdoc cref="IEnumerable{LasPointSpan}.GetEnumerator" />
        public LasPointSpanEnumerator GetEnumerator() => new(reader);

        /// <inheritdoc cref="IAsyncEnumerable{LasPointMemory}.GetAsyncEnumerator" />
        public LasPointMemoryEnumerator GetAsyncEnumerator(CancellationToken cancellationToken = default) => new(reader, cancellationToken);
    }

    /// <summary>
    /// The <see cref="LasReader"/> <see cref="IEnumerator{LasPointSpan}"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    public ref struct LasPointSpanEnumerator(LasReader reader)
    {
        /// <inheritdoc cref="IEnumerator{LasPointSpan}.Current" />
        public LasPointSpan Current { get; private set; }

        /// <inheritdoc cref="System.Collections.IEnumerator.MoveNext()" />
        public bool MoveNext()
        {
            this.Current = reader.ReadPointDataRecord();
            return this.Current.PointDataRecord is not null;
        }

        /// <inheritdoc cref="System.Collections.IEnumerator.Reset()" />
        public readonly void Reset() => reader.ResetReading();
    }

    /// <summary>
    /// The <see cref="LasReader"/> <see cref="IAsyncEnumerator{LasPointMemory}"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public sealed class LasPointMemoryEnumerator(LasReader reader, CancellationToken cancellationToken) : IAsyncEnumerator<LasPointMemory>
    {
        private LasPointMemory current;

        /// <inheritdoc />
        public LasPointMemory Current => this.current;

        /// <inheritdoc />
        public ValueTask DisposeAsync() => default;

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            this.current = await reader.ReadPointDataRecordAsync(cancellationToken).ConfigureAwait(false);
            return this.current.PointDataRecord is not null;
        }
    }
}