// -----------------------------------------------------------------------
// <copyright file="PointDataRecordFieldAccessorsExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="FieldAccessors.PointDataRecord"/> extensions.
/// </summary>
internal static class PointDataRecordFieldAccessorsExtensions
{
    extension(FieldAccessors.PointDataRecord)
    {
        /// <summary>
        /// Sets the classification.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetClassification(Span<byte> bytes, Classification value) => FieldAccessors.PointDataRecord.SetClassification(ref bytes[Constants.PointDataRecord.ClassificationFieldOffset], value);

        /// <summary>
        /// Sets the synthetic flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetSynthetic(Span<byte> bytes, bool value) => FieldAccessors.PointDataRecord.SetSynthetic(ref bytes[Constants.PointDataRecord.ClassificationFieldOffset], value);

        /// <summary>
        /// Sets the key point flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetKeyPoint(Span<byte> bytes, bool value) => FieldAccessors.PointDataRecord.SetKeyPoint(ref bytes[Constants.PointDataRecord.ClassificationFieldOffset], value);

        /// <summary>
        /// Sets the withheld flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetWithheld(Span<byte> bytes, bool value) => FieldAccessors.PointDataRecord.SetWithheld(ref bytes[Constants.PointDataRecord.ClassificationFieldOffset], value);

        /// <summary>
        /// Sets the user data.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetUserData(Span<byte> bytes, byte value) => bytes[Constants.PointDataRecord.UserDataFieldOffset] = value;

        /// <summary>
        /// Sets the scan angle rank.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetScanAngleRank(Span<byte> bytes, sbyte value) => bytes[Constants.PointDataRecord.ScanAngleRankFieldOffset] = (byte)value;

#if LAS1_2_OR_GREATER
        /// <summary>
        /// Sets the color.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="r">The red value.</param>
        /// <param name="g">The green value.</param>
        /// <param name="b">The blue value.</param>
        public static void SetColor(Span<byte> bytes, ushort r, ushort g, ushort b)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[Constants.PointDataRecord.ColorFieldOffset..], r);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[(Constants.PointDataRecord.ColorFieldOffset + sizeof(ushort))..], g);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[(Constants.PointDataRecord.ColorFieldOffset + sizeof(ushort) + sizeof(ushort))..], b);
        }

        /// <summary>
        /// Sets the color.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="r">The red value.</param>
        /// <param name="g">The green value.</param>
        /// <param name="b">The blue value.</param>
        public static void SetGpsColor(Span<byte> bytes, ushort r, ushort g, ushort b)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[Constants.PointDataRecord.GpsColorFieldOffset..], r);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[(Constants.PointDataRecord.GpsColorFieldOffset + sizeof(ushort))..], g);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[(Constants.PointDataRecord.GpsColorFieldOffset + sizeof(ushort) + sizeof(ushort))..], b);
        }
#endif

#if LAS1_3_OR_GREATER
        /// <summary>
        /// Sets the waveform values.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="wavePacketDescriptorIndex">The wave packet descriptor index.</param>
        /// <param name="byteOffsetToWaveformData">The byte offset to waveform data.</param>
        /// <param name="waveformPacketSizeInBytes">The waveform package size in bytes.</param>
        /// <param name="returnPointWaveformLocation">The temporal offset.</param>
        /// <param name="parametricDx">The parametric dx value.</param>
        /// <param name="parametricDy">The parametric dy value.</param>
        /// <param name="parametricDz">The parametric dz value.</param>
        public static void SetGpsWaveform(
            Span<byte> bytes,
            byte wavePacketDescriptorIndex,
            ulong byteOffsetToWaveformData,
            uint waveformPacketSizeInBytes,
            float returnPointWaveformLocation,
            float parametricDx,
            float parametricDy,
            float parametricDz)
        {
            const int ByteOffsetToWaveformDataFieldOffset = Constants.PointDataRecord.GpsWaveformFieldOffset + sizeof(byte);
            const int WaveformPacketSizeInBytesFieldOffset = ByteOffsetToWaveformDataFieldOffset + sizeof(ulong);
            const int ReturnPointWaveformLocationFieldOffset = WaveformPacketSizeInBytesFieldOffset + sizeof(uint);
            const int ParametricDxFieldOffset = ReturnPointWaveformLocationFieldOffset + sizeof(float);
            const int ParametricDyFieldOffset = ParametricDxFieldOffset + sizeof(float);
            const int ParametricDzFieldOffset = ParametricDyFieldOffset + sizeof(float);

            bytes[Constants.PointDataRecord.GpsWaveformFieldOffset] = wavePacketDescriptorIndex;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(bytes[ByteOffsetToWaveformDataFieldOffset..WaveformPacketSizeInBytesFieldOffset], byteOffsetToWaveformData);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes[WaveformPacketSizeInBytesFieldOffset..ReturnPointWaveformLocationFieldOffset], waveformPacketSizeInBytes);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ReturnPointWaveformLocationFieldOffset..ParametricDxFieldOffset], returnPointWaveformLocation);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDxFieldOffset..ParametricDyFieldOffset], parametricDx);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDyFieldOffset..ParametricDzFieldOffset], parametricDy);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDzFieldOffset..GpsWaveformPointDataRecord.Size], parametricDz);
        }

        /// <summary>
        /// Sets the color and waveform values.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="r">The red value.</param>
        /// <param name="g">The green value.</param>
        /// <param name="b">The blue value.</param>
        /// <param name="wavePacketDescriptorIndex">The wave packet descriptor index.</param>
        /// <param name="byteOffsetToWaveformData">The byte offset to waveform data.</param>
        /// <param name="waveformPacketSizeInBytes">The waveform package size in bytes.</param>
        /// <param name="returnPointWaveformLocation">The temporal offset.</param>
        /// <param name="parametricDx">The parametric dx value.</param>
        /// <param name="parametricDy">The parametric dy value.</param>
        /// <param name="parametricDz">The parametric dz value.</param>
        public static void SetGpsColorWaveform(
            Span<byte> bytes,
            ushort r,
            ushort g,
            ushort b,
            byte wavePacketDescriptorIndex,
            ulong byteOffsetToWaveformData,
            uint waveformPacketSizeInBytes,
            float returnPointWaveformLocation,
            float parametricDx,
            float parametricDy,
            float parametricDz)
        {
            const int ByteOffsetToWaveformDataFieldOffset = Constants.PointDataRecord.GpsColorWaveformFieldOffset + sizeof(byte);
            const int WaveformPacketSizeInBytesFieldOffset = ByteOffsetToWaveformDataFieldOffset + sizeof(ulong);
            const int ReturnPointWaveformLocationFieldOffset = WaveformPacketSizeInBytesFieldOffset + sizeof(uint);
            const int ParametricDxFieldOffset = ReturnPointWaveformLocationFieldOffset + sizeof(float);
            const int ParametricDyFieldOffset = ParametricDxFieldOffset + sizeof(float);
            const int ParametricDzFieldOffset = ParametricDyFieldOffset + sizeof(float);

            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[Constants.PointDataRecord.GpsColorWaveformFieldOffset..], r);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[(Constants.PointDataRecord.GpsColorWaveformFieldOffset + sizeof(ushort))..], g);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[(Constants.PointDataRecord.GpsColorWaveformFieldOffset + sizeof(ushort) + sizeof(ushort))..], b);
            bytes[Constants.PointDataRecord.GpsColorWaveformFieldOffset] = wavePacketDescriptorIndex;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(bytes[ByteOffsetToWaveformDataFieldOffset..WaveformPacketSizeInBytesFieldOffset], byteOffsetToWaveformData);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes[WaveformPacketSizeInBytesFieldOffset..ReturnPointWaveformLocationFieldOffset], waveformPacketSizeInBytes);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ReturnPointWaveformLocationFieldOffset..ParametricDxFieldOffset], returnPointWaveformLocation);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDxFieldOffset..ParametricDyFieldOffset], parametricDx);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDyFieldOffset..ParametricDzFieldOffset], parametricDy);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDzFieldOffset..GpsColorWaveformPointDataRecord.Size], parametricDz);
        }
#endif
    }
}