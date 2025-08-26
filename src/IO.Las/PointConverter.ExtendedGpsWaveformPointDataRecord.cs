// -----------------------------------------------------------------------
// <copyright file="PointConverter.ExtendedGpsWaveformPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// <see cref="ExtendedGpsWaveformPointDataRecord"/> convertion methods.
/// </content>
internal static partial class PointConverter
{
    /// <summary>
    /// <see cref="ExtendedGpsWaveformPointDataRecord"/> convertion methods.
    /// </summary>
    public static class ToExtendedGpsWaveformPointDataRecord
    {
        /// <summary>
        /// Creates a <see cref="ExtendedGpsWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsWaveformPointDataRecord FromPointDataRecord(IPointDataRecord pointDataRecord) =>
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsWaveformPointDataRecord FromGpsPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsWaveformPointDataRecord FromGpsWaveformPointDataRecord<T>(T pointDataRecord)
            where T : IPointDataRecord, IWaveformPointDataRecord =>
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
                WavePacketDescriptorIndex = pointDataRecord.WavePacketDescriptorIndex,
                ByteOffsetToWaveformData = pointDataRecord.ByteOffsetToWaveformData,
                WaveformPacketSizeInBytes = pointDataRecord.WaveformPacketSizeInBytes,
                ReturnPointWaveformLocation = pointDataRecord.ReturnPointWaveformLocation,
                ParametricDx = pointDataRecord.ParametricDx,
                ParametricDy = pointDataRecord.ParametricDy,
                ParametricDz = pointDataRecord.ParametricDz,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsWaveformPointDataRecord FromExtendedPointDataRecord(IExtendedPointDataRecord pointDataRecord) =>
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsWaveformPointDataRecord FromExtendedWaveformPointDataRecord<T>(T pointDataRecord)
            where T : IExtendedPointDataRecord, IWaveformPointDataRecord =>
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