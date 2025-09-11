// -----------------------------------------------------------------------
// <copyright file="PointConverter.GpsPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// <see cref="GpsPointDataRecord"/> convertion methods.
/// </content>
internal static partial class PointConverter
{
    /// <summary>
    /// <see cref="GpsPointDataRecord"/> convertion methods.
    /// </summary>
    public static class ToGpsPointDataRecord
    {
        /// <summary>
        /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
        public static GpsPointDataRecord FromPointDataRecord(IPointDataRecord pointDataRecord) =>
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
            };

        /// <summary>
        /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
        public static GpsPointDataRecord FromGpsPointDataRecord<T>(T pointDataRecord)
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
            };

#if LAS1_4_OR_GREATER
        /// <summary>
        /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
        public static GpsPointDataRecord FromExtendedPointDataRecord(IExtendedPointDataRecord pointDataRecord) =>
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
            };
#endif
    }
}