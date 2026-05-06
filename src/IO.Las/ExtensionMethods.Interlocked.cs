// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.Interlocked.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="System.Threading.Interlocked"/> extensions.
/// </summary>
public static partial class ExtensionMethods
{
    extension(System.Threading.Interlocked)
    {
#if NET9_0_OR_GREATER
        /// <summary>
        /// Compares two instances of the specified type T and, if the value is less, replaces the first one.
        /// </summary>
        /// <typeparam name="T">The type to be used for <paramref name="location1"/>, and <paramref name="value"/>.</typeparam>
        /// <param name="location1">The destination, whose value is compared with comparand and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if this value is less than <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        public static bool MinExchange<T>(ref T location1, T value)
            where T : System.Numerics.IComparisonOperators<T, T, bool>
        {
            T snapshot;
            bool stillLess;
            do
            {
                snapshot = location1;
                stillLess = value < snapshot;
            }
            while (stillLess && !Interlocked.CompareExchange(ref location1, value, snapshot).Equals(snapshot));

            return stillLess;
        }

        /// <summary>
        /// Compares two instances of the specified type T and, if the value is greater, replaces the first one.
        /// </summary>
        /// <typeparam name="T">The type to be used for <paramref name="location1"/>, and <paramref name="value"/>.</typeparam>
        /// <param name="location1">The destination, whose value is compared with comparand and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if this value is greater than <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        public static bool MaxExchange<T>(ref T location1, T value)
            where T : System.Numerics.IComparisonOperators<T, T, bool>
        {
            T snapshot;
            bool stillLess;
            do
            {
                snapshot = location1;
                stillLess = value > snapshot;
            }
            while (stillLess && !Interlocked.CompareExchange(ref location1, value, snapshot).Equals(snapshot));

            return stillLess;
        }
#else
        /// <summary>
        /// Compares double-precision floating point numbers and, if the value is less, replaces the first one.
        /// </summary>
        /// <param name="location1">The destination, whose value is compared with comparand and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if this value is less than <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        public static bool MinExchange(ref double location1, double value)
        {
            double snapshot;
            bool stillLess;
            do
            {
                snapshot = location1;
                stillLess = value < snapshot;
            }
            while (stillLess && !Interlocked.CompareExchange(ref location1, value, snapshot).Equals(snapshot));

            return stillLess;
        }

        /// <summary>
        /// Compares two double-precision floating point numbers and, if the value is greater, replaces the first one.
        /// </summary>
        /// <param name="location1">The destination, whose value is compared with comparand and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if this value is greater than <paramref name="location1"/>.</param>
        /// <returns>The original value in <paramref name="location1"/>.</returns>
        public static bool MaxExchange(ref double location1, double value)
        {
            double snapshot;
            bool stillMore;
            do
            {
                snapshot = location1;
                stillMore = value > snapshot;
            }
            while (stillMore && !Interlocked.CompareExchange(ref location1, value, snapshot).Equals(snapshot));

            return stillMore;
        }
#endif
    }
}