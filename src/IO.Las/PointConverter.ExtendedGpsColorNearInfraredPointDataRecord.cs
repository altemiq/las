// -----------------------------------------------------------------------
// <copyright file="PointConverter.ExtendedGpsColorNearInfraredPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> convertion methods.
/// </content>
internal static partial class PointConverter
{
    /// <summary>
    /// <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> convertion methods.
    /// </summary>
    public static class ToExtendedGpsColorNearInfraredPointDataRecord
    {
        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="PointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(IPointDataRecord pointDataRecord) =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                Overlap = pointDataRecord.Classification is Classification.OverlapPoints,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = (ExtendedClassification)pointDataRecord.Classification,
                ScanAngle = ScanAngleRankToScanAngle(pointDataRecord.ScanAngleRank),
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = default,
                Color = default,
                NearInfrared = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="PointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredPointDataRecord FromGpsPointDataRecord<T>(T pointDataRecord)
            where T : IPointDataRecord, IGpsPointDataRecord =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                Overlap = pointDataRecord.Classification is Classification.OverlapPoints,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = (ExtendedClassification)pointDataRecord.Classification,
                ScanAngle = ScanAngleRankToScanAngle(pointDataRecord.ScanAngleRank),
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = pointDataRecord.GpsTime,
                Color = default,
                NearInfrared = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="PointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredPointDataRecord FromColorPointDataRecord<T>(T pointDataRecord)
            where T : IPointDataRecord, IColorPointDataRecord =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                Overlap = pointDataRecord.Classification is Classification.OverlapPoints,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = (ExtendedClassification)pointDataRecord.Classification,
                ScanAngle = ScanAngleRankToScanAngle(pointDataRecord.ScanAngleRank),
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = default,
                Color = pointDataRecord.Color,
                NearInfrared = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="PointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredPointDataRecord FromGpsColorPointDataRecord<T>(T pointDataRecord)
            where T : IPointDataRecord, IGpsPointDataRecord, IColorPointDataRecord =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                Overlap = pointDataRecord.Classification is Classification.OverlapPoints,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = (ExtendedClassification)pointDataRecord.Classification,
                ScanAngle = ScanAngleRankToScanAngle(pointDataRecord.ScanAngleRank),
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = pointDataRecord.GpsTime,
                Color = pointDataRecord.Color,
                NearInfrared = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="PointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredPointDataRecord FromExtendedPointDataRecord(IExtendedPointDataRecord pointDataRecord) =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                Overlap = pointDataRecord.Overlap,
                ScannerChannel = pointDataRecord.ScannerChannel,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = pointDataRecord.Classification,
                ScanAngle = pointDataRecord.ScanAngle,
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = pointDataRecord.GpsTime,
                Color = default,
                NearInfrared = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="PointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredPointDataRecord FromExtendedColorPointDataRecord<T>(T pointDataRecord)
            where T : IExtendedPointDataRecord, IColorPointDataRecord =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                Overlap = pointDataRecord.Overlap,
                ScannerChannel = pointDataRecord.ScannerChannel,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = pointDataRecord.Classification,
                ScanAngle = pointDataRecord.ScanAngle,
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = pointDataRecord.GpsTime,
                Color = pointDataRecord.Color,
                NearInfrared = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="PointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredPointDataRecord FromExtendedNearInfraredPointDataRecord<T>(T pointDataRecord)
            where T : IExtendedPointDataRecord, INearInfraredPointDataRecord =>
            new()
            {
                X = pointDataRecord.X,
                Y = pointDataRecord.Y,
                Z = pointDataRecord.Z,
                Intensity = pointDataRecord.Intensity,
                ReturnNumber = pointDataRecord.ReturnNumber,
                NumberOfReturns = pointDataRecord.NumberOfReturns,
                Synthetic = pointDataRecord.Synthetic,
                KeyPoint = pointDataRecord.KeyPoint,
                Withheld = pointDataRecord.Withheld,
                Overlap = pointDataRecord.Overlap,
                ScannerChannel = pointDataRecord.ScannerChannel,
                ScanDirectionFlag = pointDataRecord.ScanDirectionFlag,
                EdgeOfFlightLine = pointDataRecord.EdgeOfFlightLine,
                Classification = pointDataRecord.Classification,
                ScanAngle = pointDataRecord.ScanAngle,
                UserData = pointDataRecord.UserData,
                PointSourceId = pointDataRecord.PointSourceId,
                GpsTime = pointDataRecord.GpsTime,
                Color = pointDataRecord.Color,
                NearInfrared = pointDataRecord.NearInfrared,
            };
    }
}