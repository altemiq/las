// -----------------------------------------------------------------------
// <copyright file="LasReaderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

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
        public LasPointMemoryEnumerator GetAsyncEnumerator(CancellationToken cancellationToken = default) => new(reader, continueOnCapturedContext: false, cancellationToken);

        /// <inheritdoc cref="TaskAsyncEnumerableExtensions.ConfigureAwait{T}(IAsyncEnumerable{T}, bool)" />
        public ConfiguredCancellableLasPointMemoryEnumerable ConfigureAwait(bool continueOnCapturedContext) => new(reader, continueOnCapturedContext, default);

        /// <inheritdoc cref="TaskAsyncEnumerableExtensions.WithCancellation{T}(IAsyncEnumerable{T}, CancellationToken)" />
        public ConfiguredCancellableLasPointMemoryEnumerable WithCancellation(CancellationToken cancellationToken = default) => new(reader, continueOnCapturedContext: false, cancellationToken);

        /// <summary>
        /// Returns the <see cref="LasReader"/> as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <returns>The <see cref="IAsyncEnumerable{T}"/>.</returns>
        public IAsyncEnumerable<LasPointMemory> AsAsyncEnumerable() => new LasPointMemoryAsyncEnumerable(reader);
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
    /// The <see cref="LasReader"/> <see cref="IAsyncEnumerable{LasPointMemory}"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="continueOnCapturedContext"><see langword="true" /> to capture and marshal back to the current context; otherwise, <see langword="false" />.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public readonly struct ConfiguredCancellableLasPointMemoryEnumerable(LasReader reader, bool continueOnCapturedContext, CancellationToken cancellationToken)
    {
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
        /// <inheritdoc cref="System.Runtime.CompilerServices.ConfiguredCancelableAsyncEnumerable{T}.ConfigureAwait(bool)"/>
        public ConfiguredCancellableLasPointMemoryEnumerable ConfigureAwait(bool continueOnCapturedContext) => new(reader, continueOnCapturedContext, cancellationToken);

        /// <inheritdoc cref="System.Runtime.CompilerServices.ConfiguredCancelableAsyncEnumerable{T}.WithCancellation(CancellationToken)"/>
        public ConfiguredCancellableLasPointMemoryEnumerable WithCancellation(CancellationToken cancellationToken) => new(reader, continueOnCapturedContext, cancellationToken);

        /// <inheritdoc cref="System.Runtime.CompilerServices.ConfiguredCancelableAsyncEnumerable{T}.GetAsyncEnumerator()"/>
        public LasPointMemoryEnumerator GetAsyncEnumerator() => new(reader, continueOnCapturedContext, cancellationToken);
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
    }

    private readonly struct LasPointMemoryAsyncEnumerable(LasReader reader) : IAsyncEnumerable<LasPointMemory>
    {
        IAsyncEnumerator<LasPointMemory> IAsyncEnumerable<LasPointMemory>.GetAsyncEnumerator(CancellationToken cancellationToken) => new LasReaderExtensions.LasPointMemoryEnumerator(reader, continueOnCapturedContext: false, cancellationToken);
    }

    /// <summary>
    /// The <see cref="LasReader"/> <see cref="IAsyncEnumerator{LasPointMemory}"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="continueOnCapturedContext"><see langword="true" /> to capture and marshal back to the current context; otherwise, <see langword="false" />.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public sealed class LasPointMemoryEnumerator(LasReader reader, bool continueOnCapturedContext, CancellationToken cancellationToken) : IAsyncEnumerator<LasPointMemory>
    {
        /// <inheritdoc />
        public LasPointMemory Current { get; private set; }

        /// <inheritdoc />
        public ValueTask DisposeAsync() => default;

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            this.Current = await reader.ReadPointDataRecordAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
            return this.Current.PointDataRecord is not null;
        }
    }
}