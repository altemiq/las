// -----------------------------------------------------------------------
// <copyright file="PointConverter.GpsColorWaveformPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// <see cref="GpsColorWaveformPointDataRecord"/> convertion methods.
/// </content>
internal static partial class PointConverter
{
    /// <summary>
    /// <see cref="GpsColorWaveformPointDataRecord"/> convertion methods.
    /// </summary>
    public static class ToGpsColorWaveformPointDataRecord
    {
        /// <summary>
        /// Creates a <see cref="GpsColorWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
        public static GpsColorWaveformPointDataRecord FromPointDataRecord(IPointDataRecord pointDataRecord) =>
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
        public static GpsColorWaveformPointDataRecord FromGpsPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
        public static GpsColorWaveformPointDataRecord FromColorPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
        public static GpsColorWaveformPointDataRecord FromGpsColorPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
        public static GpsColorWaveformPointDataRecord FromGpsWaveformPointDataRecord<T>(T pointDataRecord)
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
                Color = default,
                WavePacketDescriptorIndex = pointDataRecord.WavePacketDescriptorIndex,
                ByteOffsetToWaveformData = pointDataRecord.ByteOffsetToWaveformData,
                WaveformPacketSizeInBytes = pointDataRecord.WaveformPacketSizeInBytes,
                ReturnPointWaveformLocation = pointDataRecord.ReturnPointWaveformLocation,
                ParametricDx = pointDataRecord.ParametricDx,
                ParametricDy = pointDataRecord.ParametricDy,
                ParametricDz = pointDataRecord.ParametricDz,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorWaveformPointDataRecord"/> from a <see cref="IPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
        public static GpsColorWaveformPointDataRecord FromGpsColorWaveformPointDataRecord<T>(T pointDataRecord)
            where T : IPointDataRecord, IColorPointDataRecord, IWaveformPointDataRecord =>
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
                WavePacketDescriptorIndex = pointDataRecord.WavePacketDescriptorIndex,
                ByteOffsetToWaveformData = pointDataRecord.ByteOffsetToWaveformData,
                WaveformPacketSizeInBytes = pointDataRecord.WaveformPacketSizeInBytes,
                ReturnPointWaveformLocation = pointDataRecord.ReturnPointWaveformLocation,
                ParametricDx = pointDataRecord.ParametricDx,
                ParametricDy = pointDataRecord.ParametricDy,
                ParametricDz = pointDataRecord.ParametricDz,
            };

#if LAS1_4_OR_GREATER
        /// <summary>
        /// Creates a <see cref="GpsColorWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
        public static GpsColorWaveformPointDataRecord FromExtendedPointDataRecord(IExtendedPointDataRecord pointDataRecord) =>
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
        public static GpsColorWaveformPointDataRecord FromExtendedColorPointDataRecord<T>(T pointDataRecord)
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
                WavePacketDescriptorIndex = default,
                ByteOffsetToWaveformData = default,
                WaveformPacketSizeInBytes = default,
                ReturnPointWaveformLocation = default,
                ParametricDx = default,
                ParametricDy = default,
                ParametricDz = default,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
        public static GpsColorWaveformPointDataRecord FromExtendedGpsWaveformPointDataRecord<T>(T pointDataRecord)
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
                Color = default,
                WavePacketDescriptorIndex = pointDataRecord.WavePacketDescriptorIndex,
                ByteOffsetToWaveformData = pointDataRecord.ByteOffsetToWaveformData,
                WaveformPacketSizeInBytes = pointDataRecord.WaveformPacketSizeInBytes,
                ReturnPointWaveformLocation = pointDataRecord.ReturnPointWaveformLocation,
                ParametricDx = pointDataRecord.ParametricDx,
                ParametricDy = pointDataRecord.ParametricDy,
                ParametricDz = pointDataRecord.ParametricDz,
            };

        /// <summary>
        /// Creates a <see cref="GpsColorWaveformPointDataRecord"/> from a <see cref="IExtendedPointDataRecord"/>.
        /// </summary>
        /// <typeparam name="T">The type of point.</typeparam>
        /// <param name="pointDataRecord">The point data record.</param>
        /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
        public static GpsColorWaveformPointDataRecord FromExtendedGpsColorWaveformPointDataRecord<T>(T pointDataRecord)
            where T : IExtendedPointDataRecord, IColorPointDataRecord, IWaveformPointDataRecord =>
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
                WavePacketDescriptorIndex = pointDataRecord.WavePacketDescriptorIndex,
                ByteOffsetToWaveformData = pointDataRecord.ByteOffsetToWaveformData,
                WaveformPacketSizeInBytes = pointDataRecord.WaveformPacketSizeInBytes,
                ReturnPointWaveformLocation = pointDataRecord.ReturnPointWaveformLocation,
                ParametricDx = pointDataRecord.ParametricDx,
                ParametricDy = pointDataRecord.ParametricDy,
                ParametricDz = pointDataRecord.ParametricDz,
            };
#endif
    }
}