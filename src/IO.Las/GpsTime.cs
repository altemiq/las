// -----------------------------------------------------------------------
// <copyright file="GpsTime.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The GPS Time methods.
/// </summary>
internal static class GpsTime
{
    /// <summary>
    /// The default time-offset.
    /// </summary>
    public const ushort DefaultTimeOffset = 1000;

    /// <summary>
    /// The standard offset.
    /// </summary>
    public const double StandardOffset = BaseOffset * DefaultTimeOffset;

    private const int SecondsInWeek = 60 * 60 * 24 * 7;

    private const double BaseOffset = 1e6;

    private const long BaseGpsTimeTicks = 624515616000000000;

    private static readonly DateTime BaseGpsTime = new(BaseGpsTimeTicks, DateTimeKind.Utc);

    /// <summary>
    /// Converts a GPS time to a date/time.
    /// </summary>
    /// <param name="gpsTime">The GPS time.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>The date/time.</returns>
    public static DateTime GpsTimeToDateTime(double gpsTime, double offset) => BaseGpsTime.AddTicks((long)((gpsTime + offset) * TimeSpan.TicksPerSecond));

    /// <summary>
    /// Converts a date/time to a GPS time.
    /// </summary>
    /// <param name="gpsTime">The GPS time.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>The offset GPS time.</returns>
    public static double DateTimeToGpsTime(DateTime gpsTime, double offset) => gpsTime.Subtract(BaseGpsTime).TotalSeconds - offset;

#if LAS1_5_OR_GREATER
    /// <summary>
    /// Converts a time offset GPS time to a date/time.
    /// </summary>
    /// <param name="gpsTime">The time offset GPS time.</param>
    /// <param name="timeOffset">The time offset.</param>
    /// <returns>The date/time for the time offset value.</returns>
    public static DateTime TimeOffsetGpsTimeToDateTime(double gpsTime, ushort timeOffset) => GpsTimeToDateTime(gpsTime, GetOffset(timeOffset));

    /// <summary>
    /// Converts a <see cref="DateTime"/> to a time offset GPS time.
    /// </summary>
    /// <param name="gpsTime">The GPS time.</param>
    /// <param name="timeOffset">The time offset.</param>
    /// <returns>The time offset GPS time.</returns>
    public static double DateTimeToTimeOffsetGpsTime(DateTime gpsTime, ushort timeOffset) => DateTimeToGpsTime(gpsTime, GetOffset(timeOffset));
#endif

    /// <summary>
    /// Gets the offset for the header block.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <returns>The offset.</returns>
    /// <exception cref="InvalidOperationException">Unable to determine offset.</exception>
    public static double GetOffset(in HeaderBlock header)
    {
#if LAS1_2_OR_GREATER
        return HasFlag(header.GlobalEncoding, GlobalEncoding.StandardGpsTime)
#if LAS1_5_OR_GREATER
            ? GetAdjusted(header)
#else
            ? StandardOffset
#endif
            : GetOffset(CalculateGpsWeek(header));

#if LAS1_5_OR_GREATER
        static double GetAdjusted(in HeaderBlock header)
        {
            return HasFlag(header.GlobalEncoding, GlobalEncoding.TimeOffsetFlag)
                ? header.TimeOffset
                : StandardOffset;
        }
#endif

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static bool HasFlag(GlobalEncoding globalEncoding, GlobalEncoding flag)
        {
            return (globalEncoding & flag) == flag;
        }
#else
        return GetOffset(CalculateGpsWeek(header));
#endif

        static int CalculateGpsWeek(in HeaderBlock header)
        {
            return header.FileCreation switch
            {
                { } fileCreation => (int)Math.Floor((fileCreation - BaseGpsTime).TotalDays / 7D),
                _ => throw new InvalidOperationException(Properties.Resources.CannotDetermineGpsWeek),
            };
        }
    }

    /// <summary>
    /// Gets the offset for the GPS week.
    /// </summary>
    /// <param name="gpsWeek">The GPS week.</param>
    /// <returns>The offset.</returns>
    public static double GetOffset(int gpsWeek) => gpsWeek * SecondsInWeek;

#if LAS1_5_OR_GREATER
    /// <summary>
    /// Gets the recommended offset for the specified date/time.
    /// </summary>
    /// <param name="dateTime">The date/time.</param>
    /// <returns>The recommended offset.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dateTime"/> is not a valid date/time.</exception>
    public static double GetOffset(DateTime dateTime) => GetOffset(GetTimeOffset(dateTime));

    /// <summary>
    /// Gets the offset for the time offset.
    /// </summary>
    /// <param name="timeOffset">The time offset.</param>
    /// <returns>The offset.</returns>
    public static double GetOffset(ushort timeOffset) => timeOffset * BaseOffset;

    /// <summary>
    /// Gets the recommended time offset for the current date/time.
    /// </summary>
    /// <returns>The recommended offset.</returns>
    public static ushort GetTimeOffset() => GetTimeOffset(DateTime.UtcNow);

    /// <summary>
    /// Gets the recommended time offset for the specified date/time.
    /// </summary>
    /// <param name="dateTime">The date/time.</param>
    /// <returns>The recommended offset.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dateTime"/> is not a valid date/time.</exception>
    public static ushort GetTimeOffset(DateTime dateTime) => GetTimeOffset(dateTime.Year);

    /// <summary>
    /// Gets the recommended time offset for the specified year.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <returns>The recommended offset.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="year"/> is not a valid date/time.</exception>
    public static ushort GetTimeOffset(int year) => year switch
    {
        >= 1991 and < 2000 => 500,
        >= 1999 and < 2008 => 750,
        >= 2007 and < 2015 => 100,
        >= 2015 and < 2023 => 125,
        >= 2023 and < 2031 => 150,
        >= 2031 and < 2039 => 175,
        >= 2039 and < 2047 => 200,
        _ => throw new ArgumentOutOfRangeException(nameof(year)),
    };
#endif

    /// <summary>
    /// Converts an adjusted standard GPS time to a date/time.
    /// </summary>
    /// <param name="gpsTime">The adjusted standard GPS time.</param>
    /// <returns>The date/time for the adjusted standard value.</returns>
    public static DateTime AdjustedStandardGpsTimeToDateTime(double gpsTime) => GpsTimeToDateTime(gpsTime, StandardOffset);

    /// <summary>
    /// Converts a <see cref="DateTime"/> to an adjusted standard GPS time.
    /// </summary>
    /// <param name="gpsTime">The GPS time.</param>
    /// <returns>The adjusted standard GPS time.</returns>
    public static double DateTimeToAdjustedStandardGpsTime(DateTime gpsTime) => DateTimeToGpsTime(gpsTime, StandardOffset);

    /// <summary>
    /// Converts a double GPS week value to a date/time.
    /// </summary>
    /// <param name="gpsTime">The double GPS time.</param>
    /// <param name="gpsWeek">The GPS week.</param>
    /// <returns>The date/time for the GPS week value.</returns>
    public static DateTime GpsWeekTimeToDateTime(double gpsTime, int gpsWeek) => GpsTimeToDateTime(gpsTime, GetOffset(gpsWeek));

    /// <summary>
    /// Converts a GPS week value to a double value.
    /// </summary>
    /// <param name="gpsTime">The GPS time.</param>
    /// <param name="gpsWeek">The GPS week.</param>
    /// <returns>The double GPS time value.</returns>
    public static double DateTimeToGpsWeekTime(DateTime gpsTime, int gpsWeek) => DateTimeToGpsTime(gpsTime, GetOffset(gpsWeek));
}