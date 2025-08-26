// -----------------------------------------------------------------------
// <copyright file="PointConverter.GpsWaveformPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// <see cref="GpsWaveformPointDataRecord"/> convertion methods.
/// </content>
internal static partial class PointConverter
{
    /// <summary>
    /// <see cref="GpsWaveformPointDataRecord"/> convertion methods.
    /// </summary>
    public static class ToGpsWaveformPointDataRecord
    {
        /// <summary>
        /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
        public static GpsWaveformPointDataRecord FromPointDataRecord(IPointDataRecord pointDataRecord) =>
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
        public static GpsWaveformPointDataRecord FromGpsPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
        public static GpsWaveformPointDataRecord FromGpsWaveformPointDataRecord<T>(T pointDataRecord)
            where T : IPointDataRecord, IWaveformPointDataRecord =>
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
                WavePacketDescriptorIndex = pointDataRecord.WavePacketDescriptorIndex,
                ByteOffsetToWaveformData = pointDataRecord.ByteOffsetToWaveformData,
                WaveformPacketSizeInBytes = pointDataRecord.WaveformPacketSizeInBytes,
                ReturnPointWaveformLocation = pointDataRecord.ReturnPointWaveformLocation,
                ParametricDx = pointDataRecord.ParametricDx,
                ParametricDy = pointDataRecord.ParametricDy,
                ParametricDz = pointDataRecord.ParametricDz,
            };

        /// <summary>
        /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
        public static GpsWaveformPointDataRecord FromExtendedPointDataRecord(IExtendedPointDataRecord pointDataRecord) =>
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
        public static GpsWaveformPointDataRecord FromExtendedGpsWaveformPointDataRecord<T>(T pointDataRecord)
            where T : IExtendedPointDataRecord, IWaveformPointDataRecord =>
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
                WavePacketDescriptorIndex = pointDataRecord.WavePacketDescriptorIndex,
                ByteOffsetToWaveformData = pointDataRecord.ByteOffsetToWaveformData,
                WaveformPacketSizeInBytes = pointDataRecord.WaveformPacketSizeInBytes,
                ReturnPointWaveformLocation = pointDataRecord.ReturnPointWaveformLocation,
                ParametricDx = pointDataRecord.ParametricDx,
                ParametricDy = pointDataRecord.ParametricDy,
                ParametricDz = pointDataRecord.ParametricDz,
            };
    }
}