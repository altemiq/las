// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Extension methods.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1708: Identifiers should differ by more than case", Justification = "Checked")]
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
    }

    /// <summary>
    /// Gets the point data record length.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <returns>The point data record length.</returns>
    /// <exception cref="InvalidOperationException">Invalid <see cref="HeaderBlock.PointDataFormatId"/> for <see cref="HeaderBlock.Version"/>.</exception>
    /// <exception cref="InvalidCastException">Invalid <see cref="HeaderBlock.PointDataFormatId"/> value.</exception>
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
        _ => throw new InvalidCastException(),
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

    private static void MoveToPosition(Stream stream, long currentPosition, long position)
    {
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

            var delta = position - currentPosition;
            while (delta > 0)
            {
                _ = stream.ReadByte();
                delta--;
            }
        }
    }
}