// -----------------------------------------------------------------------
// <copyright file="PointConverter.ExtendedGpsPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// <see cref="ExtendedGpsPointDataRecord"/> convertion methods.
/// </content>
internal static partial class PointConverter
{
    /// <summary>
    /// <see cref="ExtendedGpsPointDataRecord"/> convertion methods.
    /// </summary>
    public static class ToExtendedGpsPointDataRecord
    {
        /// <summary>
        /// Creates a <see cref="ExtendedGpsPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsPointDataRecord"/>.</returns>
        public static ExtendedGpsPointDataRecord FromPointDataRecord(IPointDataRecord pointDataRecord) =>
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
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsPointDataRecord"/>.</returns>
        public static ExtendedGpsPointDataRecord FromGpsPointDataRecord<T>(T pointDataRecord)
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
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsPointDataRecord"/>.</returns>
        public static ExtendedGpsPointDataRecord FromExtendedPointDataRecord(IExtendedPointDataRecord pointDataRecord) =>
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
            };
    }
}