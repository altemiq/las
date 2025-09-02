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
            MoveForward(stream, streamPosition, position);
        }
    }

    private static void MoveForward(Stream stream, long baseStreamPosition, long position)
    {
        if (stream.CanSeek)
        {
            stream.Position = position;
        }
        else
        {
            var delta = position - baseStreamPosition;
            while (delta > 0)
            {
                _ = stream.ReadByte();
                delta--;
            }
        }
    }
}