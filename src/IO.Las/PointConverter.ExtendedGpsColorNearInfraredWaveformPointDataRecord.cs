// -----------------------------------------------------------------------
// <copyright file="PointConverter.ExtendedGpsColorNearInfraredWaveformPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> convertion methods.
/// </content>
internal static partial class PointConverter
{
    /// <summary>
    /// <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> convertion methods.
    /// </summary>
    public static class ToExtendedGpsColorNearInfraredWaveformPointDataRecord
    {
        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromPointDataRecord(IPointDataRecord pointDataRecord) =>
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromGpsPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromColorPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromGpsColorPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromGpsWaveformPointDataRecord<T>(T pointDataRecord)
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
                Color = default,
                NearInfrared = default,
                WavePacketDescriptorIndex = pointDataRecord.WavePacketDescriptorIndex,
                ByteOffsetToWaveformData = pointDataRecord.ByteOffsetToWaveformData,
                WaveformPacketSizeInBytes = pointDataRecord.WaveformPacketSizeInBytes,
                ReturnPointWaveformLocation = pointDataRecord.ReturnPointWaveformLocation,
                ParametricDx = pointDataRecord.ParametricDx,
                ParametricDy = pointDataRecord.ParametricDy,
                ParametricDz = pointDataRecord.ParametricDz,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromGpsColorWaveformPointDataRecord<T>(T pointDataRecord)
            where T : IPointDataRecord, IColorPointDataRecord, IWaveformPointDataRecord =>
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
                WavePacketDescriptorIndex = pointDataRecord.WavePacketDescriptorIndex,
                ByteOffsetToWaveformData = pointDataRecord.ByteOffsetToWaveformData,
                WaveformPacketSizeInBytes = pointDataRecord.WaveformPacketSizeInBytes,
                ReturnPointWaveformLocation = pointDataRecord.ReturnPointWaveformLocation,
                ParametricDx = pointDataRecord.ParametricDx,
                ParametricDy = pointDataRecord.ParametricDy,
                ParametricDz = pointDataRecord.ParametricDz,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromExtendedPointDataRecord(IExtendedPointDataRecord pointDataRecord) =>
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromExtendedWaveformPointDataRecord<T>(T pointDataRecord)
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
                Color = default,
                NearInfrared = default,
                WavePacketDescriptorIndex = pointDataRecord.WavePacketDescriptorIndex,
                ByteOffsetToWaveformData = pointDataRecord.ByteOffsetToWaveformData,
                WaveformPacketSizeInBytes = pointDataRecord.WaveformPacketSizeInBytes,
                ReturnPointWaveformLocation = pointDataRecord.ReturnPointWaveformLocation,
                ParametricDx = pointDataRecord.ParametricDx,
                ParametricDy = pointDataRecord.ParametricDy,
                ParametricDz = pointDataRecord.ParametricDz,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromExtendedColorPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromExtendedNearInfraredPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
        public static ExtendedGpsColorNearInfraredWaveformPointDataRecord FromExtendedNearInfraredWaveformPointDataRecord<T>(T pointDataRecord)
            where T : IExtendedPointDataRecord, INearInfraredPointDataRecord, IWaveformPointDataRecord =>
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