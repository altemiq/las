// -----------------------------------------------------------------------
// <copyright file="Angle.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <summary>
/// Angle functions.
/// </summary>
internal static class Angle
{
    private const double MinutesInDegree = 60D;

    private const double SecondsInMinute = 60D;

    private const double SecondsInDegree = SecondsInMinute * MinutesInDegree;

    private const double DegreesInSecond = 1 / SecondsInDegree;

    private const double MinutesInSecond = 1 / SecondsInMinute;

    /// <summary>
    /// Angular units.
    /// </summary>
    private enum AngularUnit
    {
        /// <summary>
        /// Invalid angular unit.
        /// </summary>
        None = default,

        /// <summary>
        /// SI coherent derived unit (standard unit) for plane angle.
        /// </summary>
        Radian = 1,

        /// <summary>
        /// = pi/180 radians.
        /// </summary>
        Degree = 2,

        /// <summary>
        /// 1/60th degree = ((pi/180) / 60) radians.
        /// </summary>
        ArcMinute = 3,

        /// <summary>
        /// 1/60th arc-minute = ((pi/180) / 3600) radians.
        /// </summary>
        ArcSecond = 4,

        /// <summary>
        /// =pi/200 radians.
        /// </summary>
        Gradient = 5,

        /// <summary>
        /// =pi/200 radians.
        /// </summary>
        Gon = 6,

        /// <summary>
        /// Degree representation. Format: signed degrees (integer) - arc-minutes (integer) - arc-seconds (real, any precision). Different symbol sets are in use as field separators, for example º '' ". Convert to degrees using algorithm.
        /// </summary>
        DegreeMinuteSecond = 7,

        /// <summary>
        /// Degree representation. Format: degrees (integer) - arc-minutes (integer) - arc-seconds (real) - hemisphere abbreviation (single character N S E or W). Different symbol sets are in use as field separators for example º '' ". Convert to deg using algorithm.
        /// </summary>
        DegreeMinuteSecondHemisphere = 8,

        /// <summary>
        /// rad * 10E-6.
        /// </summary>
        MicroRadian = 9,

        /// <summary>
        /// Pseudo unit. Format: signed degrees - period - minutes (2 digits) - integer seconds (2 digits) - fraction of seconds (any precision). Must include leading zero in minutes and seconds and exclude decimal point for seconds. Convert to deg using algorithm.
        /// </summary>
        SexagesimalDegreeMinuteSecond = 10,

        /// <summary>
        /// Pseudo unit. Format: signed degrees - period - integer minutes (2 digits) - fraction of minutes (any precision). Must include leading zero in minutes and exclude decimal point for minutes. Convert to degree using algorithm.
        /// </summary>
        SexagesimalDegreeMinute = 11,

        /// <summary>
        /// 1/100 of a grad and gon = ((pi/200) / 100) radians.
        /// </summary>
        CentesimalMinute = 12,

        /// <summary>
        /// 1/100 of a centesimal minute or 1/10,000th of a grad and gon = ((pi/200) / 10000) radians.
        /// </summary>
        CentesimalSecond = 13,

        /// <summary>
        /// Angle subtended by 1/6400 part of a circle.  Approximates to 1/1000th radian.  Note that other approximations (notably 1/6300 circle and 1/6000 circle) also exist.
        /// </summary>
        Mil6400 = 14,

        /// <summary>
        /// Degree representation. Format: signed degrees (integer)  - arc-minutes (real, any precision). Different symbol sets are in use as field separators, for example º ''. Convert to degrees using algorithm.
        /// </summary>
        DegreeMinute = 15,

        /// <summary>
        /// Degree representation. Format: degrees (real, any precision) - hemisphere abbreviation (single character N S E or W). Convert to degrees using algorithm.
        /// </summary>
        DegreeHemisphere = 16,

        /// <summary>
        /// Degree representation. Format: hemisphere abbreviation (single character N S E or W) - degrees (real, any precision). Convert to degrees using algorithm.
        /// </summary>
        HemisphereDegree = 17,

        /// <summary>
        /// Degree representation. Format: degrees (integer) - arc-minutes (real, any precision) - hemisphere abbreviation (single character N S E or W). Different symbol sets are in use as field separators, for example º ''. Convert to degrees using algorithm.
        /// </summary>
        DegreeMinuteHemisphere = 18,

        /// <summary>
        /// Degree representation. Format:  hemisphere abbreviation (single character N S E or W) - degrees (integer) - arc-minutes (real, any precision). Different symbol sets are in use as field separators, for example º ''. Convert to degrees using algorithm.
        /// </summary>
        HemisphereDegreeMinute = 19,

        /// <summary>
        /// Degree representation. Format: hemisphere abbreviation (single character N S E or W) - degrees (integer) - arc-minutes (integer) - arc-seconds (real). Different symbol sets are in use as field separators for example º '' ". Convert to deg using algorithm.
        /// </summary>
        HemisphereDegreeMinuteSecond = 20,

        /// <summary>
        /// Pseudo unit. Format: signed degrees - minutes (2 digits) - integer seconds (2 digits) - period - fraction of seconds (any precision). Must include leading zero in minutes and seconds and include decimal point for seconds. Convert to deg using algorithm.
        /// </summary>
        SexagesimalDegreesMinutesSecondsWithFraction = 21,

        /// <summary>
        /// = pi/180 radians. The degree representation (e.g. decimal, DMSH, etc.) must be clarified by suppliers of data associated with this code.
        /// </summary>
        SupplierDefinedDegree = 22,

        /// <summary>
        /// = ((pi/180) / 3600 / 1000) radians.
        /// </summary>
        MilliarcSecond = 1031 - 9000,
    }

    /// <summary>
    /// Gets the angle in the specified units in the target units.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="sourceUom">The source units of measure.</param>
    /// <param name="targetUom">The target units of measure.</param>
    /// <returns>The converted <paramref name="value"/> in <paramref name="targetUom"/>.</returns>
    public static double GetAngle(this double value, int sourceUom, int targetUom = 9102)
    {
        if (sourceUom == targetUom)
        {
            return value;
        }

        var sourceUnit = (AngularUnit)(sourceUom - 9100);
        var targetUnit = (AngularUnit)(targetUom - 9100);
        var factor = GetToFactor(sourceUnit);

        if (targetUnit is not AngularUnit.Radian)
        {
            factor *= GetFromFactor(targetUnit);
        }

        return ConvertTo(ConvertFrom(value, sourceUnit) * factor, targetUnit);
    }

    private static double GetToFactor(AngularUnit unit) => unit switch
    {
        AngularUnit.MilliarcSecond => Math.PI / 648000000D,
        AngularUnit.Radian => 1D,
        AngularUnit.Degree or AngularUnit.SupplierDefinedDegree or AngularUnit.DegreeHemisphere or AngularUnit.DegreeMinute or AngularUnit.DegreeMinuteHemisphere or AngularUnit.DegreeMinuteSecond or AngularUnit.DegreeMinuteSecondHemisphere or AngularUnit.HemisphereDegree
            or AngularUnit.HemisphereDegreeMinute or AngularUnit.HemisphereDegreeMinuteSecond or AngularUnit.SexagesimalDegreeMinuteSecond or AngularUnit.SexagesimalDegreeMinute => Math.PI / 180D,
        AngularUnit.ArcMinute => Math.PI / 10800D,
        AngularUnit.ArcSecond => Math.PI / 648000D,
        AngularUnit.Gradient or AngularUnit.Gon => Math.PI / 200D,
        AngularUnit.MicroRadian => 1D / 1000000D,
        AngularUnit.CentesimalMinute => Math.PI / 20000D,
        AngularUnit.CentesimalSecond => Math.PI / 2000000D,
        AngularUnit.Mil6400 => Math.PI / 3200D,
        _ => throw new ArgumentException(Geodesy.Properties.Resources.UnknownUom, nameof(unit)),
    };

    private static double GetFromFactor(AngularUnit unit) => unit switch
    {
        AngularUnit.MilliarcSecond => 648000000D / Math.PI,
        AngularUnit.Radian => 1D,
        AngularUnit.Degree or AngularUnit.SupplierDefinedDegree or AngularUnit.DegreeHemisphere or AngularUnit.DegreeMinute or AngularUnit.DegreeMinuteHemisphere or AngularUnit.DegreeMinuteSecond or AngularUnit.DegreeMinuteSecondHemisphere or AngularUnit.HemisphereDegree
            or AngularUnit.HemisphereDegreeMinute or AngularUnit.HemisphereDegreeMinuteSecond or AngularUnit.SexagesimalDegreeMinuteSecond or AngularUnit.SexagesimalDegreeMinute => 180D / Math.PI,
        AngularUnit.ArcMinute => 10800D / Math.PI,
        AngularUnit.ArcSecond => 648000D / Math.PI,
        AngularUnit.Gradient or AngularUnit.Gon => 200D / Math.PI,
        AngularUnit.MicroRadian => 1000000D,
        AngularUnit.CentesimalMinute => 20000D / Math.PI,
        AngularUnit.CentesimalSecond => 2000000D / Math.PI,
        AngularUnit.Mil6400 => 3200D / Math.PI,
        _ => throw new ArgumentException(Geodesy.Properties.Resources.UnknownUom, nameof(unit)),
    };

    private static double ConvertTo(double value, AngularUnit unit)
    {
        return unit switch
        {
            AngularUnit.SexagesimalDegreeMinuteSecond => SexagesimalDMS(value),
            AngularUnit.SexagesimalDegreeMinute => SexagesimalDM(value),
            _ => value,
        };

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static double SexagesimalDMS(double value)
        {
            var (degrees, minutes, seconds) = GetDms(value);
            return degrees + (minutes / 100D) + (seconds / 10000D);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static double SexagesimalDM(double value)
        {
            var (degrees, minutes, seconds) = GetDms(value);
            return degrees + ((minutes + (seconds / 60D)) / 100D);
        }
    }

    private static double ConvertFrom(double value, AngularUnit unit)
    {
        return unit switch
        {
            AngularUnit.SexagesimalDegreeMinuteSecond => SexagesimalDMS(value),
            AngularUnit.SexagesimalDegreeMinute => SexagesimalDM(value),
            _ => value,
        };

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static double GetNext(ref decimal value)
        {
            var output = Math.Truncate(value);
            value -= output;
            value *= 100;
            return (double)output;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static double SexagesimalDMS(double value)
        {
            var sign = Math.Sign(value);
            var @decimal = (decimal)Math.Abs(value);
            var degrees = GetNext(ref @decimal);
            var minutes = GetNext(ref @decimal);
            var seconds = (double)@decimal;

            return sign * (degrees + (minutes / 60D) + (seconds / 3600D));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static double SexagesimalDM(double value)
        {
            var sign = Math.Sign(value);
            var @decimal = (decimal)Math.Abs(value);
            var degrees = GetNext(ref @decimal);
            var minutes = (double)@decimal;

            return sign * (degrees + (minutes / 60D));
        }
    }

    private static (int Degrees, int Minutes, float Seconds) GetDms(double value)
    {
        var totalSeconds = value * SecondsInDegree;
        var degrees = (int)Math.Truncate(totalSeconds * DegreesInSecond);
        var totalMinutes = totalSeconds * MinutesInSecond;
        var minutes = (int)Math.Truncate(totalMinutes % MinutesInDegree);
        var seconds = (float)(totalSeconds % SecondsInMinute);
        return (degrees, minutes, seconds);
    }
}