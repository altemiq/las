// -----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The constants.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The column constants.
    /// </summary>
    public static class Columns
    {
        /// <summary>
        /// The <see cref="IBasePointDataRecord.X"/> column name.
        /// </summary>
        public const string X = "x";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Y"/> column name.
        /// </summary>
        public const string Y = "y";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Z"/> column name.
        /// </summary>
        public const string Z = "z";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Intensity"/> column name.
        /// </summary>
        public const string Intensity = "intensity";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.ReturnNumber"/> column name.
        /// </summary>
        public const string ReturnNumber = "return_number";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.NumberOfReturns"/> column name.
        /// </summary>
        public const string NumberOfReturns = "number_of_returns";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.ScanDirectionFlag"/> column name.
        /// </summary>
        public const string ScanDirectionFlag = "scan_direction_flag";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.EdgeOfFlightLine"/> column name.
        /// </summary>
        public const string EdgeOfFlightLine = "edge_of_flight_line";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Synthetic"/> column name.
        /// </summary>
        public const string Synthetic = "synthetic";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.KeyPoint"/> column name.
        /// </summary>
        public const string KeyPoint = "key_point";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.Withheld"/> column name.
        /// </summary>
        public const string Withheld = "withheld";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.UserData"/> column name.
        /// </summary>
        public const string UserData = "user_data";

        /// <summary>
        /// The <see cref="IBasePointDataRecord.PointSourceId"/> column name.
        /// </summary>
        public const string PointSourceId = "point_source_id";

        /// <summary>
        /// The <see cref="IPointDataRecord"/> column names.
        /// </summary>
        public static class Legacy
        {
            /// <summary>
            /// The <see cref="IPointDataRecord.Classification"/> column name.
            /// </summary>
            public const string Classification = "classification";

            /// <summary>
            /// The <see cref="IPointDataRecord.ScanAngleRank"/> column name.
            /// </summary>
            public const string ScanAngleRank = "scan_angle_rank";
        }

        /// <summary>
        /// The <see cref="IGpsPointDataRecord"/> column names.
        /// </summary>
        public static class Gps
        {
            /// <summary>
            /// The <see cref="IGpsPointDataRecord.GpsTime"/> column name.
            /// </summary>
            public const string GpsTime = "gps_time";
        }

#if LAS1_2_OR_GREATER
        /// <summary>
        /// The <see cref="IColorPointDataRecord"/> column names.
        /// </summary>
        public static class Color
        {
            /// <summary>
            /// The <see cref="Altemiq.IO.Las.Color.Red"/> column name.
            /// </summary>
            public const string Red = "red";

            /// <summary>
            /// The <see cref="Altemiq.IO.Las.Color.Green"/> column name.
            /// </summary>
            public const string Green = "green";

            /// <summary>
            /// The <see cref="Altemiq.IO.Las.Color.Blue"/> column name.
            /// </summary>
            public const string Blue = "blue";
        }
#endif

#if LAS1_3_OR_GREATER
        /// <summary>
        /// The <see cref="IWaveformPointDataRecord"/> column names.
        /// </summary>
        public static class Waveform
        {
            /// <summary>
            /// The <see cref="IWaveformPointDataRecord.WavePacketDescriptorIndex"/> column name.
            /// </summary>
            public const string WavePacketDescriptorIndex = "wave_packet_descriptor_index";

            /// <summary>
            /// The <see cref="IWaveformPointDataRecord.ByteOffsetToWaveformData"/> column name.
            /// </summary>
            public const string ByteOffsetToWaveformData = "byte_offset_to_waveform_data";

            /// <summary>
            /// The <see cref="IWaveformPointDataRecord.WaveformPacketSizeInBytes"/> column name.
            /// </summary>
            public const string WaveformPacketSizeInBytes = "waveform_packet_size_in_bytes";

            /// <summary>
            /// The <see cref="IWaveformPointDataRecord.ReturnPointWaveformLocation"/> column name.
            /// </summary>
            public const string ReturnPointWaveformLocation = "return_point_waveform_location";

            /// <summary>
            /// The <see cref="IWaveformPointDataRecord.ParametricDx"/> column name.
            /// </summary>
            public const string ParametricDx = "parametric_d_x";

            /// <summary>
            /// The <see cref="IWaveformPointDataRecord.ParametricDy"/> column name.
            /// </summary>
            public const string ParametricDy = "parametric_d_y";

            /// <summary>
            /// The <see cref="IWaveformPointDataRecord.ParametricDz"/> column name.
            /// </summary>
            public const string ParametricDz = "parametric_d_z";
        }
#endif

#if LAS1_4_OR_GREATER
        /// <summary>
        /// The <see cref="IExtendedPointDataRecord"/> column names.
        /// </summary>
        public static class Extended
        {
            /// <summary>
            /// The <see cref="IExtendedPointDataRecord.Overlap"/> column name.
            /// </summary>
            public const string Overlap = "overlap";

            /// <summary>
            /// The <see cref="IExtendedPointDataRecord.ScannerChannel"/> column name.
            /// </summary>
            public const string ScannerChannel = "scanner_channel";

            /// <summary>
            /// The <see cref="IExtendedPointDataRecord.Classification"/> column name.
            /// </summary>
            public const string Classification = "classification";

            /// <summary>
            /// The <see cref="IExtendedPointDataRecord.ScanAngle"/> column name.
            /// </summary>
            public const string ScanAngle = "scan_angle";
        }

        /// <summary>
        /// The <see cref="INearInfraredPointDataRecord"/> column names.
        /// </summary>
        public static class Nir
        {
            /// <summary>
            /// The <see cref="INearInfraredPointDataRecord.NearInfrared"/> column name.
            /// </summary>
            public const string NearInfrared = "nir";
        }
#endif
    }

    /// <summary>
    /// The metadata constants.
    /// </summary>
    internal static class Metadata
    {
        /// <summary>
        /// The <see cref="HeaderBlock.PointDataFormatId"/> metadata name.
        /// </summary>
        public const string PointDataFormatId = "point_data_format_id";

#if LAS1_2_OR_GREATER
        /// <summary>
        /// The <see cref="HeaderBlock.GlobalEncoding"/> metadata name.
        /// </summary>
        public const string GlobalEncoding = "global_encoding";
#endif

        /// <summary>
        /// The <see cref="HeaderBlock.Version"/> metadata name.
        /// </summary>
        public const string Version = "version";

        /// <summary>
        /// The <see cref="HeaderBlock.Offset"/> metadata name.
        /// </summary>
        public const string Offset = "offset";

        /// <summary>
        /// The <see cref="HeaderBlock.ScaleFactor"/> metadata name.
        /// </summary>
        public const string ScaleFactor = "scale_factor";

#if LAS1_5_OR_GREATER
        /// <summary>
        /// The <see cref="HeaderBlock.TimeOffset"/> metadata name.
        /// </summary>
        public const string TimeOffset = "time_offset";
#endif
    }
}