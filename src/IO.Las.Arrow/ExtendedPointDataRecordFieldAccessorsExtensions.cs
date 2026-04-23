// -----------------------------------------------------------------------
// <copyright file="ExtendedPointDataRecordFieldAccessorsExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="FieldAccessors.ExtendedPointDataRecord"/> extensions.
/// </summary>
internal static class ExtendedPointDataRecordFieldAccessorsExtensions
{
    extension(FieldAccessors.ExtendedPointDataRecord)
    {
        /// <summary>
        /// Sets the classification.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetClassification(Span<byte> bytes, ExtendedClassification value) => bytes[Constants.ExtendedPointDataRecord.ClassificationFieldOffset] = (byte)value;

        /// <summary>
        /// Sets the synthetic flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetSynthetic(Span<byte> bytes, bool value) => FieldAccessors.ExtendedPointDataRecord.SetSynthetic(ref bytes[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset], value);

        /// <summary>
        /// Sets the key point flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetKeyPoint(Span<byte> bytes, bool value) => FieldAccessors.ExtendedPointDataRecord.SetKeyPoint(ref bytes[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset], value);

        /// <summary>
        /// Sets the withheld flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetWithheld(Span<byte> bytes, bool value) => FieldAccessors.ExtendedPointDataRecord.SetWithheld(ref bytes[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset], value);

        /// <summary>
        /// Sets the overlap flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetOverlap(Span<byte> bytes, bool value) => FieldAccessors.ExtendedPointDataRecord.SetOverlap(ref bytes[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset], value);

        /// <summary>
        /// Sets the scanner channel.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetScannerChannel(Span<byte> bytes, byte value) => FieldAccessors.ExtendedPointDataRecord.SetScannerChannel(ref bytes[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset], value);

        /// <summary>
        /// Sets the user data.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetUserData(Span<byte> bytes, byte value) => bytes[Constants.ExtendedPointDataRecord.UserDataFieldOffset] = value;

        /// <summary>
        /// Sets the color.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="r">The red value.</param>
        /// <param name="g">The green value.</param>
        /// <param name="b">The blue value.</param>
        public static void SetColor(Span<byte> bytes, ushort r, ushort g, ushort b)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[Constants.ExtendedPointDataRecord.ColorFieldOffset..], r);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[(Constants.ExtendedPointDataRecord.ColorFieldOffset + sizeof(ushort))..], g);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[(Constants.ExtendedPointDataRecord.ColorFieldOffset + sizeof(ushort) + sizeof(ushort))..], b);
        }

        /// <summary>
        /// Sets the near infrared value.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value.</param>
        public static void SetNearInfrared(Span<byte> bytes, ushort value) => System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[Constants.ExtendedPointDataRecord.NirFieldOffset..ExtendedGpsColorNearInfraredPointDataRecord.Size], value);

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
        public static void SetWaveform(
            Span<byte> bytes,
            byte wavePacketDescriptorIndex,
            ulong byteOffsetToWaveformData,
            uint waveformPacketSizeInBytes,
            float returnPointWaveformLocation,
            float parametricDx,
            float parametricDy,
            float parametricDz)
        {
            const int WavePacketDescriptorIndexFieldOffset = Constants.ExtendedPointDataRecord.GpsTimeFieldOffset + Constants.Size.GpsTime;
            const int ByteOffsetToWaveformDataFieldOffset = WavePacketDescriptorIndexFieldOffset + sizeof(byte);
            const int WaveformPacketSizeInBytesFieldOffset = ByteOffsetToWaveformDataFieldOffset + sizeof(ulong);
            const int ReturnPointWaveformLocationFieldOffset = WaveformPacketSizeInBytesFieldOffset + sizeof(uint);
            const int ParametricDxFieldOffset = ReturnPointWaveformLocationFieldOffset + sizeof(float);
            const int ParametricDyFieldOffset = ParametricDxFieldOffset + sizeof(float);
            const int ParametricDzFieldOffset = ParametricDyFieldOffset + sizeof(float);

            bytes[WavePacketDescriptorIndexFieldOffset] = wavePacketDescriptorIndex;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(bytes[ByteOffsetToWaveformDataFieldOffset..WaveformPacketSizeInBytesFieldOffset], byteOffsetToWaveformData);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes[WaveformPacketSizeInBytesFieldOffset..ReturnPointWaveformLocationFieldOffset], waveformPacketSizeInBytes);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ReturnPointWaveformLocationFieldOffset..ParametricDxFieldOffset], returnPointWaveformLocation);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDxFieldOffset..ParametricDyFieldOffset], parametricDx);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDyFieldOffset..ParametricDzFieldOffset], parametricDy);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDzFieldOffset..ExtendedGpsWaveformPointDataRecord.Size], parametricDz);
        }

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
        public static void SetNearInfraredWaveform(
            Span<byte> bytes,
            byte wavePacketDescriptorIndex,
            ulong byteOffsetToWaveformData,
            uint waveformPacketSizeInBytes,
            float returnPointWaveformLocation,
            float parametricDx,
            float parametricDy,
            float parametricDz)
        {
            const int WavePacketDescriptorIndexFieldOffset = Constants.ExtendedPointDataRecord.NirFieldOffset + sizeof(ushort);
            const int ByteOffsetToWaveformDataFieldOffset = WavePacketDescriptorIndexFieldOffset + sizeof(byte);
            const int WaveformPacketSizeInBytesFieldOffset = ByteOffsetToWaveformDataFieldOffset + sizeof(ulong);
            const int ReturnPointWaveformLocationFieldOffset = WaveformPacketSizeInBytesFieldOffset + sizeof(uint);
            const int ParametricDxFieldOffset = ReturnPointWaveformLocationFieldOffset + sizeof(float);
            const int ParametricDyFieldOffset = ParametricDxFieldOffset + sizeof(float);
            const int ParametricDzFieldOffset = ParametricDyFieldOffset + sizeof(float);

            bytes[WavePacketDescriptorIndexFieldOffset] = wavePacketDescriptorIndex;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(bytes[ByteOffsetToWaveformDataFieldOffset..WaveformPacketSizeInBytesFieldOffset], byteOffsetToWaveformData);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes[WaveformPacketSizeInBytesFieldOffset..ReturnPointWaveformLocationFieldOffset], waveformPacketSizeInBytes);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ReturnPointWaveformLocationFieldOffset..ParametricDxFieldOffset], returnPointWaveformLocation);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDxFieldOffset..ParametricDyFieldOffset], parametricDx);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDyFieldOffset..ParametricDzFieldOffset], parametricDy);
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(bytes[ParametricDzFieldOffset..ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size], parametricDz);
        }
    }
}