// -----------------------------------------------------------------------
// <copyright file="FieldAccessors.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The field accessors.
/// </summary>
internal static class FieldAccessors
{
    /// <summary>
    /// The <see cref="IPointDataRecord"/> <see cref="FieldAccessors"/>.
    /// </summary>
    public static class PointDataRecord
    {
        /// <summary>
        /// Gets the x-value.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The x-value.</returns>
        public static int GetX(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source[..Constants.PointDataRecord.YFieldOffset]);

        /// <summary>
        /// Sets the x-value.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetX(Span<byte> destination, int value) => System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[..Constants.PointDataRecord.YFieldOffset], value);

        /// <summary>
        /// Gets the y-value.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The y-value.</returns>
        public static int GetY(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source[Constants.PointDataRecord.YFieldOffset..Constants.PointDataRecord.ZFieldOffset]);

        /// <summary>
        /// Sets the y-value.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetY(Span<byte> destination, int value) => System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[Constants.PointDataRecord.YFieldOffset..Constants.PointDataRecord.ZFieldOffset], value);

        /// <summary>
        /// Gets the z-value.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The z-value.</returns>
        public static int GetZ(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source[Constants.PointDataRecord.ZFieldOffset..Constants.PointDataRecord.IntensityFieldOffset]);

        /// <summary>
        /// Sets the z-value.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetZ(Span<byte> destination, int value) => System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[Constants.PointDataRecord.ZFieldOffset..Constants.PointDataRecord.IntensityFieldOffset], value);

        /// <summary>
        /// Gets the intensity.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The intensity.</returns>
        public static ushort GetIntensity(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[Constants.PointDataRecord.IntensityFieldOffset..Constants.PointDataRecord.FlagsFieldOffset]);

        /// <summary>
        /// Sets the intensity.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetIntensity(Span<byte> destination, ushort value) => System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.PointDataRecord.IntensityFieldOffset..Constants.PointDataRecord.FlagsFieldOffset], value);

        /// <summary>
        /// Gets the point source ID.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The pointSourceId.</returns>
        public static ushort GetPointSourceId(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[Constants.PointDataRecord.PointSourceIdFieldOffset..Constants.PointDataRecord.GpsTimeFieldOffset]);

        /// <summary>
        /// Sets the point source ID.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetPointSourceId(Span<byte> destination, ushort value) => System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.PointDataRecord.PointSourceIdFieldOffset..Constants.PointDataRecord.GpsTimeFieldOffset], value);

        /// <summary>
        /// Gets the return number.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The return number.</returns>
        public static byte GetReturnNumber(ReadOnlySpan<byte> bytes) => GetReturnNumber(bytes[Constants.PointDataRecord.FlagsFieldOffset]);

        /// <summary>
        /// Gets the return number.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns>The return number.</returns>
        public static byte GetReturnNumber(byte flags) => BitManipulation.Get(flags, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2);

        /// <summary>
        /// Sets the return number.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetReturnNumber(ref byte flags, byte value) => BitManipulation.Set(ref flags, value, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2);

        /// <summary>
        /// Sets the return number.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value to set.</param>
        public static void SetReturnNumber(Span<byte> bytes, byte value) => SetReturnNumber(ref bytes[Constants.PointDataRecord.FlagsFieldOffset], value);

        /// <summary>
        /// Gets the number of returns.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The number of returns.</returns>
        public static byte GetNumberOfReturns(ReadOnlySpan<byte> bytes) => GetNumberOfReturns(bytes[Constants.PointDataRecord.FlagsFieldOffset]);

        /// <summary>
        /// Gets the number of returns.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns>The number of returns.</returns>
        public static byte GetNumberOfReturns(byte flags) => BitManipulation.Get(flags, Constants.BitMasks.Mask3 | Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5, 3);

        /// <summary>
        /// Sets the number of returns.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value to set.</param>
        public static void SetNumberOfReturns(Span<byte> bytes, byte value) => SetNumberOfReturns(ref bytes[Constants.PointDataRecord.FlagsFieldOffset], value);

        /// <summary>
        /// Sets the number of returns.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetNumberOfReturns(ref byte flags, byte value) => BitManipulation.Set(ref flags, value, Constants.BitMasks.Mask3 | Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5, 3);

        /// <summary>
        /// Gets the scan direction flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The scan direction flag.</returns>
        public static bool GetScanDirectionFlag(ReadOnlySpan<byte> bytes) => GetScanDirectionFlag(bytes[Constants.PointDataRecord.FlagsFieldOffset]);

        /// <summary>
        /// Gets the scan direction flag.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns>The scan direction flag.</returns>
        public static bool GetScanDirectionFlag(byte flags) => BitManipulation.IsSet(flags, Constants.BitMasks.Mask6);

        /// <summary>
        /// Sets the scan direction flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value to set.</param>
        public static void SetScanDirectionFlag(Span<byte> bytes, bool value) => SetScanDirectionFlag(ref bytes[Constants.PointDataRecord.FlagsFieldOffset], value);

        /// <summary>
        /// Sets the scan direction flag.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetScanDirectionFlag(ref byte flags, bool value) => BitManipulation.Apply(ref flags, Constants.BitMasks.Mask6, value);

        /// <summary>
        /// Gets the edge of flight line.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The edge of flight line.</returns>
        public static bool GetEdgeOfFlightLine(ReadOnlySpan<byte> bytes) => GetEdgeOfFlightLine(bytes[Constants.PointDataRecord.FlagsFieldOffset]);

        /// <summary>
        /// Gets the edge of flight line.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns>The edge of flight line.</returns>
        public static bool GetEdgeOfFlightLine(byte flags) => BitManipulation.IsSet(flags, Constants.BitMasks.Mask7);

        /// <summary>
        /// Sets the edge of flight line.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value to set.</param>
        public static void SetEdgeOfFlightLine(Span<byte> bytes, bool value) => SetEdgeOfFlightLine(ref bytes[Constants.PointDataRecord.FlagsFieldOffset], value);

        /// <summary>
        /// Sets the edge of flight line.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetEdgeOfFlightLine(ref byte flags, bool value) => BitManipulation.Apply(ref flags, Constants.BitMasks.Mask7, value);

        /// <summary>
        /// Gets the classification.
        /// </summary>
        /// <param name="classification">The classification.</param>
        /// <returns>The classification enum.</returns>
        public static Classification GetClassification(byte classification) => (Classification)BitManipulation.Get(classification, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3 | Constants.BitMasks.Mask4);

        /// <summary>
        /// Sets the classification.
        /// </summary>
        /// <param name="classification">The classification.</param>
        /// <param name="value">The value to set.</param>
        public static void SetClassification(ref byte classification, Classification value) =>
            BitManipulation.Set(ref classification, (byte)value, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3 | Constants.BitMasks.Mask4);

        /// <summary>
        /// Gets the synthetic.
        /// </summary>
        /// <param name="classification">The classification.</param>
        /// <returns>The synthetic.</returns>
        public static bool GetSynthetic(byte classification) => BitManipulation.IsSet(classification, Constants.BitMasks.Mask5);

        /// <summary>
        /// Sets the synthetic flag.
        /// </summary>
        /// <param name="classification">The classification.</param>
        /// <param name="value">The value to set.</param>
        public static void SetSynthetic(ref byte classification, bool value) => BitManipulation.Apply(ref classification, Constants.BitMasks.Mask5, value);

        /// <summary>
        /// Gets the key point.
        /// </summary>
        /// <param name="classification">The classification.</param>
        /// <returns>The key point.</returns>
        public static bool GetKeyPoint(byte classification) => BitManipulation.IsSet(classification, Constants.BitMasks.Mask6);

        /// <summary>
        /// Sets the key point flag.
        /// </summary>
        /// <param name="classification">The classification.</param>
        /// <param name="value">The value to set.</param>
        public static void SetKeyPoint(ref byte classification, bool value) => BitManipulation.Apply(ref classification, Constants.BitMasks.Mask6, value);

        /// <summary>
        /// Gets the withheld flag.
        /// </summary>
        /// <param name="classification">The classification.</param>
        /// <returns>The withheld flag.</returns>
        public static bool GetWithheld(byte classification) => BitManipulation.IsSet(classification, Constants.BitMasks.Mask7);

        /// <summary>
        /// Sets the withheld flag.
        /// </summary>
        /// <param name="classification">The classification.</param>
        /// <param name="value">The value to set.</param>
        public static void SetWithheld(ref byte classification, bool value) => BitManipulation.Apply(ref classification, Constants.BitMasks.Mask7, value);
    }

    /// <summary>
    /// The <see cref="IExtendedPointDataRecord"/> <see cref="FieldAccessors"/>.
    /// </summary>
    public static class ExtendedPointDataRecord
    {
        /// <summary>
        /// Gets the x-value.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The x-value.</returns>
        public static int GetX(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source[..Constants.ExtendedPointDataRecord.YFieldOffset]);

        /// <summary>
        /// Sets the x-value.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetX(Span<byte> destination, int value) => System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[..Constants.ExtendedPointDataRecord.YFieldOffset], value);

        /// <summary>
        /// Gets the y-value.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The y-value.</returns>
        public static int GetY(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source[Constants.ExtendedPointDataRecord.YFieldOffset..Constants.ExtendedPointDataRecord.ZFieldOffset]);

        /// <summary>
        /// Sets the y-value.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetY(Span<byte> destination, int value) => System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[Constants.ExtendedPointDataRecord.YFieldOffset..Constants.ExtendedPointDataRecord.ZFieldOffset], value);

        /// <summary>
        /// Gets the z-value.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The z-value.</returns>
        public static int GetZ(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source[Constants.ExtendedPointDataRecord.ZFieldOffset..Constants.ExtendedPointDataRecord.IntensityFieldOffset]);

        /// <summary>
        /// Sets the z-value.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetZ(Span<byte> destination, int value) => System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[Constants.ExtendedPointDataRecord.ZFieldOffset..Constants.ExtendedPointDataRecord.IntensityFieldOffset], value);

        /// <summary>
        /// Gets the intensity.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The intensity.</returns>
        public static ushort GetIntensity(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[Constants.ExtendedPointDataRecord.IntensityFieldOffset..Constants.ExtendedPointDataRecord.FlagsFieldOffset]);

        /// <summary>
        /// Sets the intensity.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetIntensity(Span<byte> destination, ushort value) => System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.ExtendedPointDataRecord.IntensityFieldOffset..Constants.ExtendedPointDataRecord.FlagsFieldOffset], value);

        /// <summary>
        /// Gets the scan angle.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The scan angle.</returns>
        public static short GetScanAngle(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(source[Constants.ExtendedPointDataRecord.ScanAngleFieldOffset..Constants.ExtendedPointDataRecord.PointSourceIdFieldOffset]);

        /// <summary>
        /// Sets the scan angle.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetScanAngle(Span<byte> destination, short value) => System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(destination[Constants.ExtendedPointDataRecord.ScanAngleFieldOffset..Constants.ExtendedPointDataRecord.PointSourceIdFieldOffset], value);

        /// <summary>
        /// Gets the point source ID.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The pointSourceId.</returns>
        public static ushort GetPointSourceId(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[Constants.ExtendedPointDataRecord.PointSourceIdFieldOffset..Constants.ExtendedPointDataRecord.GpsTimeFieldOffset]);

        /// <summary>
        /// Sets the point source ID.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetPointSourceId(Span<byte> destination, ushort value) => System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.ExtendedPointDataRecord.PointSourceIdFieldOffset..Constants.ExtendedPointDataRecord.GpsTimeFieldOffset], value);

        /// <summary>
        /// Gets the return number.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The return number.</returns>
        public static byte GetReturnNumber(ReadOnlySpan<byte> bytes) => GetReturnNumber(bytes[Constants.ExtendedPointDataRecord.FlagsFieldOffset]);

        /// <summary>
        /// Gets the return number.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns>The return number.</returns>
        public static byte GetReturnNumber(byte flags) => BitManipulation.Get(flags, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3);

        /// <summary>
        /// Sets the return number.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value to set.</param>
        public static void SetReturnNumber(Span<byte> bytes, byte value) => SetReturnNumber(ref bytes[Constants.ExtendedPointDataRecord.FlagsFieldOffset], value);

        /// <summary>
        /// Sets the return number.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetReturnNumber(ref byte flags, byte value) => BitManipulation.Set(ref flags, value, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3);

        /// <summary>
        /// Gets the number of returns.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The number of returns.</returns>
        public static byte GetNumberOfReturns(ReadOnlySpan<byte> bytes) => GetNumberOfReturns(bytes[Constants.ExtendedPointDataRecord.FlagsFieldOffset]);

        /// <summary>
        /// Gets the number of returns.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns>The number of returns.</returns>
        public static byte GetNumberOfReturns(byte flags) => BitManipulation.Get(flags, Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5 | Constants.BitMasks.Mask6 | Constants.BitMasks.Mask7, 4);

        /// <summary>
        /// Sets the number of returns.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value to set.</param>
        public static void SetNumberOfReturns(Span<byte> bytes, byte value) => SetNumberOfReturns(ref bytes[Constants.ExtendedPointDataRecord.FlagsFieldOffset], value);

        /// <summary>
        /// Sets the number of returns.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetNumberOfReturns(ref byte flags, byte value) => BitManipulation.Set(ref flags, value, Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5 | Constants.BitMasks.Mask6 | Constants.BitMasks.Mask7, 4);

        /// <summary>
        /// Gets the synthetic.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <returns>The synthetic.</returns>
        public static bool GetSynthetic(byte classificationFlags) => BitManipulation.IsSet(classificationFlags, Constants.BitMasks.Mask0);

        /// <summary>
        /// Sets the synthetic flag.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetSynthetic(ref byte classificationFlags, bool value) => BitManipulation.Apply(ref classificationFlags, Constants.BitMasks.Mask0, value);

        /// <summary>
        /// Gets the key point.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <returns>The key point.</returns>
        public static bool GetKeyPoint(byte classificationFlags) => BitManipulation.IsSet(classificationFlags, Constants.BitMasks.Mask1);

        /// <summary>
        /// Sets the key point flag.
        /// </summary>
        /// <param name="classificationFlags">The classification.</param>
        /// <param name="value">The value to set.</param>
        public static void SetKeyPoint(ref byte classificationFlags, bool value) => BitManipulation.Apply(ref classificationFlags, Constants.BitMasks.Mask1, value);

        /// <summary>
        /// Gets the withheld flag.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <returns>The withheld flag.</returns>
        public static bool GetWithheld(byte classificationFlags) => BitManipulation.IsSet(classificationFlags, Constants.BitMasks.Mask2);

        /// <summary>
        /// Sets the withheld flag.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetWithheld(ref byte classificationFlags, bool value) => BitManipulation.Apply(ref classificationFlags, Constants.BitMasks.Mask2, value);

        /// <summary>
        /// Gets the overlap flag.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <returns>The overlap flag.</returns>
        public static bool GetOverlap(byte classificationFlags) => BitManipulation.IsSet(classificationFlags, Constants.BitMasks.Mask3);

        /// <summary>
        /// Sets the overlap flag.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetOverlap(ref byte classificationFlags, bool value) => BitManipulation.Apply(ref classificationFlags, Constants.BitMasks.Mask3, value);

        /// <summary>
        /// Gets the scanner channel.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <returns>The scanner channel.</returns>
        public static byte GetScannerChannel(byte classificationFlags) => BitManipulation.Get(classificationFlags, Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5, 4);

        /// <summary>
        /// Sets the scanner channel flag.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetScannerChannel(ref byte classificationFlags, byte value) => BitManipulation.Set(ref classificationFlags, value, Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5, 4);

        /// <summary>
        /// Gets the scan direction flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The scan direction flag.</returns>
        public static bool GetScanDirectionFlag(ReadOnlySpan<byte> bytes) => GetScanDirectionFlag(bytes[Constants.ExtendedPointDataRecord.ClassificationFieldOffset]);

        /// <summary>
        /// Gets the scan direction flag.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <returns>The scan direction flag.</returns>
        public static bool GetScanDirectionFlag(byte classificationFlags) => BitManipulation.IsSet(classificationFlags, Constants.BitMasks.Mask6);

        /// <summary>
        /// Sets the scan direction flag.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value to set.</param>
        public static void SetScanDirectionFlag(Span<byte> bytes, bool value) => SetScanDirectionFlag(ref bytes[Constants.ExtendedPointDataRecord.ClassificationFieldOffset], value);

        /// <summary>
        /// Sets the scan direction flag.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetScanDirectionFlag(ref byte classificationFlags, bool value) => BitManipulation.Apply(ref classificationFlags, Constants.BitMasks.Mask6, value);

        /// <summary>
        /// Gets the edge of flight line.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The edge of flight line.</returns>
        public static bool GetEdgeOfFlightLine(ReadOnlySpan<byte> bytes) => GetEdgeOfFlightLine(bytes[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset]);

        /// <summary>
        /// Gets the edge of flight line.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <returns>The edge of flight line.</returns>
        public static bool GetEdgeOfFlightLine(byte classificationFlags) => BitManipulation.IsSet(classificationFlags, Constants.BitMasks.Mask7);

        /// <summary>
        /// Sets the edge of flight line.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="value">The value to set.</param>
        public static void SetEdgeOfFlightLine(Span<byte> bytes, bool value) => SetEdgeOfFlightLine(ref bytes[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset], value);

        /// <summary>
        /// Sets the edge of flight line.
        /// </summary>
        /// <param name="classificationFlags">The classification flags.</param>
        /// <param name="value">The value to set.</param>
        public static void SetEdgeOfFlightLine(ref byte classificationFlags, bool value) => BitManipulation.Apply(ref classificationFlags, Constants.BitMasks.Mask7, value);

        /// <summary>
        /// Gets the GPS time.
        /// </summary>
        /// <param name="source">The data.</param>
        /// <returns>The GPS time.</returns>
        public static double GetGpsTime(ReadOnlySpan<byte> source) => System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[Constants.ExtendedPointDataRecord.GpsTimeFieldOffset..Constants.ExtendedPointDataRecord.ColorFieldOffset]);

        /// <summary>
        /// Sets the GPS time.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        public static void SetGpsTime(Span<byte> destination, double value) => System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[Constants.ExtendedPointDataRecord.GpsTimeFieldOffset..Constants.ExtendedPointDataRecord.ColorFieldOffset], value);
    }
}