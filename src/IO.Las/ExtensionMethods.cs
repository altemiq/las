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
                throw new InvalidOperationException($"Cannot move to {currentPosition}.");
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