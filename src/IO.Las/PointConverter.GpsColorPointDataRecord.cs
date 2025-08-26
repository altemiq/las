// -----------------------------------------------------------------------
// <copyright file="PointConverter.GpsColorPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// <see cref="GpsColorPointDataRecord"/> convertion methods.
/// </content>
internal static partial class PointConverter
{
    /// <summary>
    /// <see cref="GpsColorPointDataRecord"/> convertion methods.
    /// </summary>
    public static class ToGpsColorPointDataRecord
    {
        /// <summary>
        /// Creates a <see cref="GpsColorPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorPointDataRecord"/>.</returns>
        public static GpsColorPointDataRecord FromPointDataRecord(IPointDataRecord pointDataRecord) =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = pointDataRecord.Classification,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                ScanAngleRank = pointDataRecord.ScanAngleRank,
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = default,
                Color = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorPointDataRecord"/>.</returns>
        public static GpsColorPointDataRecord FromGpsPointDataRecord<T>(T pointDataRecord)
            where T : IPointDataRecord, IGpsPointDataRecord =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = pointDataRecord.Classification,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                ScanAngleRank = pointDataRecord.ScanAngleRank,
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = pointDataRecord.GpsTime,
                Color = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorPointDataRecord"/>.</returns>
        public static GpsColorPointDataRecord FromColorPointDataRecord<T>(T pointDataRecord)
            where T : IPointDataRecord, IColorPointDataRecord =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = pointDataRecord.Classification,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                ScanAngleRank = pointDataRecord.ScanAngleRank,
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = default,
                Color = pointDataRecord.Color,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorPointDataRecord"/>.</returns>
        public static GpsColorPointDataRecord FromGpsColorPointDataRecord<T>(T pointDataRecord)
            where T : IPointDataRecord, IGpsPointDataRecord, IColorPointDataRecord =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = pointDataRecord.Classification,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                ScanAngleRank = pointDataRecord.ScanAngleRank,
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = pointDataRecord.GpsTime,
                Color = pointDataRecord.Color,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorPointDataRecord"/>.</returns>
        public static GpsColorPointDataRecord FromExtendedPointDataRecord(IExtendedPointDataRecord pointDataRecord) =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = (Classification)pointDataRecord.Classification,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                ScanAngleRank = ScanAngleToScanAngleRank(pointDataRecord.ScanAngle),
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = pointDataRecord.GpsTime,
                Color = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorPointDataRecord"/>.</returns>
        public static GpsColorPointDataRecord FromExtendedColorPointDataRecord<T>(T pointDataRecord)
            where T : IExtendedPointDataRecord, IColorPointDataRecord =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = (Classification)pointDataRecord.Classification,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                ScanAngleRank = ScanAngleToScanAngleRank(pointDataRecord.ScanAngle),
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = pointDataRecord.GpsTime,
                Color = pointDataRecord.Color,
            };
    }
}