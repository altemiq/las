// -----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The internal constants.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// The sizes.
    /// </summary>
    public static class Size
    {
        /// <summary>
        /// The <see cref="IGpsPointDataRecord.GpsTime"/> size.
        /// </summary>
        public const ushort GpsTime = sizeof(double);

#if LAS1_2_OR_GREATER
        /// <summary>
        /// The <see cref="IColorPointDataRecord.Color"/> size.
        /// </summary>
        public const ushort Color = 3 * sizeof(ushort);
#endif

#if LAS1_3_OR_GREATER
        /// <summary>
        /// The <see cref="IWaveformPointDataRecord"/> size.
        /// </summary>
        public const ushort Waveform = sizeof(byte) + sizeof(ulong) + sizeof(uint) + sizeof(float) + sizeof(float) + sizeof(float) + sizeof(float);
#endif

#if LAS1_4_OR_GREATER
        /// <summary>
        /// The <see cref="INearInfraredPointDataRecord.NearInfrared"/> size.
        /// </summary>
        public const ushort NearInfrared = sizeof(ushort);
#endif
    }

    /// <summary>
    /// The bit masks.
    /// </summary>
    public static class BitMasks
    {
        /// <summary>
        /// The bitwise mask for bit 0.
        /// </summary>
        public const byte Mask0 = 0x01;

        /// <summary>
        /// The bitwise mask for bit 1.
        /// </summary>
        public const byte Mask1 = 0x02;

        /// <summary>
        /// The bitwise mask for bit 2.
        /// </summary>
        public const byte Mask2 = 0x04;

        /// <summary>
        /// The bitwise mask for bit 3.
        /// </summary>
        public const byte Mask3 = 0x08;

        /// <summary>
        /// The bitwise mask for bit 4.
        /// </summary>
        public const byte Mask4 = 0x10;

        /// <summary>
        /// The bitwise mask for bit 5.
        /// </summary>
        public const byte Mask5 = 0x20;

        /// <summary>
        /// The bitwise mask for bit 6.
        /// </summary>
        public const byte Mask6 = 0x40;

        /// <summary>
        /// The bitwise mask for bit 7.
        /// </summary>
        public const int Mask7 = 0x80;
    }

    /// <summary>
    /// The <see cref="IPointDataRecord"/> constants.
    /// </summary>
    public static class PointDataRecord
    {
        /// <summary>
        /// The <see cref="IBasePointDataRecord.X"/> field offset.
        /// </summary>
        public const int XFieldOffset = 0;

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Z"/> field offset.
        /// </summary>
        public const int YFieldOffset = XFieldOffset + sizeof(int);

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Z"/> field offset.
        /// </summary>
        public const int ZFieldOffset = YFieldOffset + sizeof(int);

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Intensity"/> field offset.
        /// </summary>
        public const int IntensityFieldOffset = ZFieldOffset + sizeof(int);

        /// <summary>
        /// The flags field offset.
        /// </summary>
        public const int FlagsFieldOffset = IntensityFieldOffset + sizeof(ushort);

        /// <summary>
        /// The classification field offset.
        /// </summary>
        public const int ClassificationFieldOffset = FlagsFieldOffset + sizeof(byte);

        /// <summary>
        /// The <see cref="IPointDataRecord.ScanAngleRank"/> field offset.
        /// </summary>
        public const int ScanAngleRankFieldOffset = ClassificationFieldOffset + sizeof(byte);

        /// <summary>
        /// The <see cref="IBasePointDataRecord.UserData"/> field offset.
        /// </summary>
        public const int UserDataFieldOffset = ScanAngleRankFieldOffset + sizeof(sbyte);

        /// <summary>
        /// The <see cref="IBasePointDataRecord.PointSourceId"/> field offset.
        /// </summary>
        public const int PointSourceIdFieldOffset = UserDataFieldOffset + sizeof(byte);

        /// <summary>
        /// The <see cref="IGpsPointDataRecord.GpsTime"/> field offset.
        /// </summary>
        public const int GpsTimeFieldOffset = PointSourceIdFieldOffset + sizeof(ushort);

#if LAS1_2_OR_GREATER
        /// <summary>
        /// The <see cref="IColorPointDataRecord.Color"/> field offset.
        /// </summary>
        public const int ColorFieldOffset = PointSourceIdFieldOffset + sizeof(ushort);

        /// <summary>
        /// The <see cref="IColorPointDataRecord.Color"/> field offset.
        /// </summary>
        public const int GpsColorFieldOffset = GpsTimeFieldOffset + sizeof(double);
#endif

#if LAS1_3_OR_GREATER
        /// <summary>
        /// The <see cref="IWaveformPointDataRecord.WavePacketDescriptorIndex"/> field offset.
        /// </summary>
        public const int GpsWaveformFieldOffset = GpsTimeFieldOffset + sizeof(double);

        /// <summary>
        /// The <see cref="IWaveformPointDataRecord.WavePacketDescriptorIndex"/> field offset.
        /// </summary>
        public const int GpsColorWaveformFieldOffset = GpsColorFieldOffset + (3 * sizeof(ushort));
#endif
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// The <see cref="IExtendedPointDataRecord"/> constants.
    /// </summary>
    public static class ExtendedPointDataRecord
    {
        /// <summary>
        /// The <see cref="IBasePointDataRecord.X"/> field offset.
        /// </summary>
        public const int XFieldOffset = PointDataRecord.XFieldOffset;

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Z"/> field offset.
        /// </summary>
        public const int YFieldOffset = PointDataRecord.YFieldOffset;

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Z"/> field offset.
        /// </summary>
        public const int ZFieldOffset = PointDataRecord.ZFieldOffset;

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Intensity"/> field offset.
        /// </summary>
        public const int IntensityFieldOffset = PointDataRecord.IntensityFieldOffset;

        /// <summary>
        /// The flags field offset.
        /// </summary>
        public const int FlagsFieldOffset = IntensityFieldOffset + sizeof(ushort);

        /// <summary>
        /// The classification flags field offset.
        /// </summary>
        public const int ClassificationFlagsFieldOffset = FlagsFieldOffset + sizeof(byte);

        /// <summary>
        /// The <see cref="IExtendedPointDataRecord.Classification"/> offset.
        /// </summary>
        public const int ClassificationFieldOffset = ClassificationFlagsFieldOffset + sizeof(byte);

        /// <summary>
        /// The <see cref="IBasePointDataRecord.UserData"/> field offset.
        /// </summary>
        public const int UserDataFieldOffset = ClassificationFieldOffset + sizeof(byte);

        /// <summary>
        /// The <see cref="IExtendedPointDataRecord.ScanAngle"/> field offset.
        /// </summary>
        public const int ScanAngleFieldOffset = UserDataFieldOffset + sizeof(byte);

        /// <summary>
        /// The <see cref="IBasePointDataRecord.PointSourceId"/> field offset.
        /// </summary>
        public const int PointSourceIdFieldOffset = ScanAngleFieldOffset + sizeof(short);

        /// <summary>
        /// The <see cref="IGpsPointDataRecord.GpsTime"/> field offset.
        /// </summary>
        public const int GpsTimeFieldOffset = PointSourceIdFieldOffset + sizeof(ushort);

        /// <summary>
        /// The <see cref="IColorPointDataRecord.Color"/> field offset.
        /// </summary>
        public const int ColorFieldOffset = GpsTimeFieldOffset + sizeof(double);

        /// <summary>
        /// The <see cref="INearInfraredPointDataRecord.NearInfrared"/> field offset.
        /// </summary>
        public const int NirFieldOffset = ColorFieldOffset + (3 * sizeof(ushort));
    }
#endif

    /// <summary>
    /// The <see cref="VariableLengthRecordHeader"/> constants.
    /// </summary>
    public static class VariableLengthRecord
    {
        /// <summary>
        /// The reserved field offset.
        /// </summary>
        public const int ReservedFieldOffset = 0;

        /// <summary>
        /// The <see cref="VariableLengthRecordHeader.UserId"/> field offset.
        /// </summary>
        public const int UserIdFieldOffset = ReservedFieldOffset + sizeof(ushort);

        /// <summary>
        /// The <see cref="VariableLengthRecordHeader.RecordId"/> field offset.
        /// </summary>
        public const int RecordIdFieldOffset = UserIdFieldOffset + 16;

        /// <summary>
        /// The <see cref="VariableLengthRecordHeader.RecordLengthAfterHeader"/> field offset.
        /// </summary>
        public const int RecordLengthAfterHeaderFieldOffset = RecordIdFieldOffset + sizeof(ushort);

        /// <summary>
        /// The <see cref="VariableLengthRecordHeader.Description"/> field offset.
        /// </summary>
        public const int DescriptionFieldOffset = RecordLengthAfterHeaderFieldOffset + sizeof(ushort);
    }

#if LAS1_3_OR_GREATER
    /// <summary>
    /// The <see cref="ExtendedVariableLengthRecordHeader"/> constants.
    /// </summary>
    public static class ExtendedVariableLengthRecord
    {
        /// <summary>
        /// The reserved field offset.
        /// </summary>
        public const int ReservedFieldOffset = VariableLengthRecord.ReservedFieldOffset;

        /// <summary>
        /// The <see cref="ExtendedVariableLengthRecordHeader.UserId"/> field offset.
        /// </summary>
        public const int UserIdFieldOffset = VariableLengthRecord.UserIdFieldOffset;

        /// <summary>
        /// The <see cref="ExtendedVariableLengthRecordHeader.RecordId"/> field offset.
        /// </summary>
        public const int RecordIdFieldOffset = VariableLengthRecord.RecordIdFieldOffset;

        /// <summary>
        /// The <see cref="ExtendedVariableLengthRecordHeader.RecordLengthAfterHeader"/> field offset.
        /// </summary>
        public const int RecordLengthAfterHeaderFieldOffset = VariableLengthRecord.RecordLengthAfterHeaderFieldOffset;

        /// <summary>
        /// The <see cref="ExtendedVariableLengthRecordHeader.Description"/> field offset.
        /// </summary>
        public const int DescriptionFieldOffset = RecordLengthAfterHeaderFieldOffset + sizeof(ulong);
    }
#endif
}