// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable CA1708, SA1101

/// <summary>
/// Extension methods.
/// </summary>
public static partial class ExtensionMethods
{
    /// <summary>
    /// The <see cref="Stream"/> extensions.
    /// </summary>
    extension(Stream stream)
    {
        /// <summary>
        /// Moves to <paramref name="stream"/> to the specified <paramref name="position"/>.
        /// </summary>
        /// <param name="position">The position.</param>
        internal void MoveToPositionForwardsOnly(long position)
        {
            var streamPosition = stream.Position;
            if (streamPosition < position)
            {
                MoveToPosition(stream, streamPosition, position);
            }
        }

        /// <summary>
        /// Moves to <paramref name="stream"/> to the specified <paramref name="position"/>.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        internal ValueTask MoveToPositionForwardsOnlyAsync(long position, CancellationToken cancellationToken = default)
        {
            var streamPosition = stream.Position;
            return streamPosition < position
                ? new(MoveToPositionAsync(stream, streamPosition, position, cancellationToken))
                : default;
        }

        /// <summary>
        /// Moves the <paramref name="stream"/> to the specified <paramref name="position"/>.
        /// </summary>
        /// <param name="position">The position.</param>
        internal void MoveToPositionAbsolute(long position)
        {
            var streamPosition = stream.Position;
            if (streamPosition == position)
            {
                return;
            }

            MoveToPosition(stream, streamPosition, position);
        }

        /// <summary>
        /// Moves the <paramref name="stream"/> to the specified <paramref name="position"/>.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        internal ValueTask MoveToPositionAbsoluteAsync(long position, CancellationToken cancellationToken = default)
        {
            var streamPosition = stream.Position;
            return streamPosition != position
                ? new(MoveToPositionAsync(stream, streamPosition, position, cancellationToken))
                : default;
        }
    }

    /// <summary>
    /// Gets the point data record length.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <returns>The point data record length.</returns>
    /// <exception cref="InvalidOperationException">Invalid <see cref="HeaderBlock.PointDataFormatId"/> for <see cref="HeaderBlock.Version"/>.</exception>
    /// <exception cref="InvalidCastException">Invalid <see cref="HeaderBlock.PointDataFormatId"/> value.</exception>
    /// <exception cref="System.Diagnostics.UnreachableException">The header block version is invalid.</exception>
    internal static ushort GetPointDataRecordLength(this in HeaderBlock header) => header switch
    {
        { PointDataFormatId: PointDataRecord.Id, Version: { Major: 1, Minor: >= 0 and < 5 } } => PointDataRecord.Size,
        { PointDataFormatId: GpsPointDataRecord.Id, Version: { Major: 1, Minor: >= 0 and < 5 } } => GpsPointDataRecord.Size,
#if LAS1_2_OR_GREATER
        { PointDataFormatId: ColorPointDataRecord.Id, Version: { Major: 1, Minor: >= 2 and < 5 } } => ColorPointDataRecord.Size,
        { PointDataFormatId: GpsColorPointDataRecord.Id, Version: { Major: 1, Minor: >= 2 and < 5 } } => GpsColorPointDataRecord.Size,
#endif
#if LAS1_3_OR_GREATER
        { PointDataFormatId: GpsWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 3 and < 5 } } => GpsWaveformPointDataRecord.Size,
        { PointDataFormatId: GpsColorWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 3 and < 5 } } => GpsColorWaveformPointDataRecord.Size,
#endif
#if LAS1_4_OR_GREATER
        { PointDataFormatId: ExtendedGpsPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => ExtendedGpsPointDataRecord.Size,
        { PointDataFormatId: ExtendedGpsColorPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => ExtendedGpsColorPointDataRecord.Size,
        { PointDataFormatId: ExtendedGpsColorNearInfraredPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => ExtendedGpsColorNearInfraredPointDataRecord.Size,
        { PointDataFormatId: ExtendedGpsWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => ExtendedGpsWaveformPointDataRecord.Size,
        { PointDataFormatId: ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } => ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size,
#endif
        { Version: { Major: 1, Minor: <= 1 } } => throw new InvalidOperationException(Properties.v1_1.Resources.OnlyDataPointsAreAllowed),
#if LAS1_2_OR_GREATER
        { Version: { Major: 1, Minor: 2 } } => throw new InvalidOperationException(Properties.v1_2.Resources.OnlyDataPointsAreAllowed),
#endif
#if LAS1_3_OR_GREATER
        { Version: { Major: 1, Minor: 3 } } => throw new InvalidOperationException(Properties.v1_3.Resources.OnlyDataPointsAreAllowed),
#endif
#if LAS1_4_OR_GREATER
        { Version: { Major: 1, Minor: 4 } } => throw new InvalidOperationException(Properties.v1_4.Resources.OnlyDataPointsAreAllowed),
#endif
#if LAS1_5_OR_GREATER
        { Version: { Major: 1, Minor: 5 } } => throw new InvalidOperationException(Properties.v1_5.Resources.OnlyDataPointsAreAllowed),
#endif
        _ => throw new System.Diagnostics.UnreachableException(),
    };

    /// <summary>
    /// Converts the <see cref="ReadOnlySpan{T}"/> to a <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item in the source.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>The read-only list of <typeparamref name="T"/>.</returns>
    internal static IReadOnlyList<T> ToReadOnlyList<T>(this ReadOnlySpan<T> source)
    {
        var count = source.Length;

        var builder = new System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<T>(count);

        for (var i = 0; i < count; i++)
        {
            builder.Add(source[i]);
        }

        return builder.ToReadOnlyCollection();
    }

#if NET8_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1863:Use \'CompositeFormat\'", Justification = "This is for an exception")]
#endif
    private static void MoveToPosition(Stream stream, long currentPosition, long position)
    {
        const int BufferSize = 1024;
        if (stream.CanSeek)
        {
            stream.Position = position;
        }
        else
        {
            if (position < currentPosition)
            {
                throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.CannotMoveToPosition, currentPosition));
            }

            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BufferSize);
            var delta = position - currentPosition;
            while (delta > 0)
            {
                delta -= stream.Read(buffer, 0, (int)Math.Min(BufferSize, delta)) is not 0 and var read
                    ? read
                    : throw new InvalidOperationException(Properties.Resources.FailedToReadRequiredBytesFromStream);
            }

            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }
    }

#if NET8_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1863:Use \'CompositeFormat\'", Justification = "This is for an exception")]
#endif
    private static async Task MoveToPositionAsync(Stream stream, long currentPosition, long position, CancellationToken cancellationToken = default)
    {
        const int BufferSize = 1024;
        if (stream.CanSeek)
        {
            stream.Position = position;
        }
        else
        {
            if (position < currentPosition)
            {
                throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.CannotMoveToPosition, currentPosition));
            }

            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BufferSize);
            var delta = position - currentPosition;
            while (delta > 0)
            {
                delta -= await stream.ReadAsync(buffer.AsMemory(0, (int)Math.Min(BufferSize, delta)), cancellationToken).ConfigureAwait(false) is not 0 and var read
                    ? read
                    : throw new InvalidOperationException(Properties.Resources.FailedToReadRequiredBytesFromStream);
            }

            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}