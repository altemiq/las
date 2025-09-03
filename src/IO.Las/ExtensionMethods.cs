// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Extension methods.
/// </summary>
public static partial class ExtensionMethods
{
    /// <summary>
    /// Moves to <paramref name="stream"/> to the specified <paramref name="position"/>.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="position">The position.</param>
    internal static void MoveToPositionForwardsOnly(this Stream stream, long position)
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
    /// <param name="stream">The stream.</param>
    /// <param name="position">The position.</param>
    internal static void MoveToPositionAbsolute(this Stream stream, long position)
    {
        var streamPosition = stream.Position;
        if (streamPosition == position)
        {
            return;
        }

        MoveToPosition(stream, streamPosition, position);
    }

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

        for (int i = 0; i < count; i++)
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