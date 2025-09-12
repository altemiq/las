// -----------------------------------------------------------------------
// <copyright file="HeaderBlockValidator.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="HeaderBlock"/> validator.
/// </summary>
public class HeaderBlockValidator
{
    /// <summary>
    /// The common instance.
    /// </summary>
    public static readonly HeaderBlockValidator Instance = new();

    /// <inheritdoc cref="Properties.Resources.Culture"/>
    public System.Globalization.CultureInfo Culture { get; set; } = Properties.Resources.Culture;

    /// <summary>
    /// Validates the header.
    /// </summary>
    /// <param name="header">The header to validate.</param>
    /// <param name="variableLengthRecords">The variable length records.</param>
    /// <exception cref="InvalidOperationException">The header is not valid.</exception>
    public void Validate(
        in HeaderBlock header,
        IReadOnlyCollection<VariableLengthRecord> variableLengthRecords)
    {
        switch (header)
        {
            // invalid version
            case { Version: { Major: not 1, Minor: < 1 or > HeaderBlock.MaxMinorVersion } }:
                throw new InvalidOperationException(string.Format(this.Culture, Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.InvalidVersion), this.Culture)!, header.Version, new Version(1, 1), new Version(1, HeaderBlock.MaxMinorVersion)));

            // invalid point format ID
            case { PointDataFormatId: > GpsPointDataRecord.Id, Version: { Major: 1, Minor: <= 1 } }:
                throw new InvalidOperationException(string.Format(this.Culture, Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.InvalidPointFormatId), this.Culture)!, header.PointDataFormatId, header.Version));

#if LAS1_2_OR_GREATER
            // invalid point format ID
            case { PointDataFormatId: > GpsColorPointDataRecord.Id, Version: { Major: 1, Minor: <= 2 } }:
                throw new InvalidOperationException(string.Format(this.Culture, Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.InvalidPointFormatId), this.Culture)!, header.PointDataFormatId, header.Version));
#endif

#if LAS1_3_OR_GREATER
            // invalid point format ID
            case { PointDataFormatId: > GpsColorWaveformPointDataRecord.Id, Version: { Major: 1, Minor: <= 3 } }:
                throw new InvalidOperationException(string.Format(this.Culture, Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.InvalidPointFormatId), this.Culture)!, header.PointDataFormatId, header.Version));
#endif

#if LAS1_4_OR_GREATER
            // if the point data format is >= 6, then WKT must be set
            case { PointDataFormatId: >= ExtendedGpsPointDataRecord.Id, Version: { Major: 1, Minor: >= 4 } } when !header.GlobalEncoding.HasFlag(GlobalEncoding.Wkt):
                throw new InvalidOperationException(Properties.v1_4.Resources.ResourceManager.GetString(nameof(Properties.v1_4.Resources.WktMustBeSet), this.Culture));
#endif

#if LAS1_5_OR_GREATER
            // invalid point format ID
            case { PointDataFormatId: > ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id, Version: { Major: 1, Minor: <= 5 } }:
                throw new InvalidOperationException(string.Format(this.Culture, Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.InvalidPointFormatId), this.Culture)!, header.PointDataFormatId, header.Version));
            case { PointDataFormatId: < ExtendedGpsPointDataRecord.Id, Version: { Major: 1, Minor: >= 5 } }:
                throw new InvalidOperationException(Properties.v1_5.Resources.ResourceManager.GetString(nameof(Properties.v1_5.Resources.PointFormatIdHasBeenDeprecated), this.Culture));
#endif
        }

#if LAS1_5_OR_GREATER
        if (header.GlobalEncoding.HasFlag(GlobalEncoding.TimeOffsetFlag) && !header.GlobalEncoding.HasFlag(GlobalEncoding.StandardGpsTime))
        {
            throw new InvalidOperationException(Properties.v1_5.Resources.ResourceManager.GetString(nameof(Properties.v1_5.Resources.GpsTimeFlagMustBeSet), this.Culture));
        }

        if (header is { NumberOfPointRecords: 0, TimeOffset: not 0 })
        {
            throw new InvalidOperationException(Properties.v1_5.Resources.ResourceManager.GetString(nameof(Properties.v1_5.Resources.TimeOffsetMustBeZeroWithNoPoints), this.Culture));
        }

        if (header.TimeOffset is not 0 && !header.GlobalEncoding.HasFlag(GlobalEncoding.TimeOffsetFlag))
        {
            throw new InvalidOperationException(Properties.v1_5.Resources.ResourceManager.GetString(nameof(Properties.v1_5.Resources.TimeOffsetMustBeZeroWithNoFlag), this.Culture));
        }

        // check the VLRs
        if (header is { Version: { Major: 1, Minor: >= 5 } })
        {
            CheckAny<GeoKeyDirectoryTag>(this.Culture, variableLengthRecords);
            CheckAny<GeoAsciiParamsTag>(this.Culture, variableLengthRecords);
            CheckAny<GeoDoubleParamsTag>(this.Culture, variableLengthRecords);
        }
        else
        {
            CheckMultiple<GeoKeyDirectoryTag>(this.Culture, variableLengthRecords);
            CheckMultiple<GeoAsciiParamsTag>(this.Culture, variableLengthRecords);
            CheckMultiple<GeoDoubleParamsTag>(this.Culture, variableLengthRecords);
        }
#else
        // check the VLRs
        CheckMultiple<GeoKeyDirectoryTag>(this.Culture, variableLengthRecords);
        CheckMultiple<GeoAsciiParamsTag>(this.Culture, variableLengthRecords);
        CheckMultiple<GeoDoubleParamsTag>(this.Culture, variableLengthRecords);
#endif
#if LAS1_4_OR_GREATER
        // check the VLRs
        CheckMultiple<OgcCoordinateSystemWkt>(this.Culture, variableLengthRecords);
        CheckMultiple<OgcMathTransformWkt>(this.Culture, variableLengthRecords);
#endif

#if LAS1_3_OR_GREATER
        if (IsWavePointDataFormatId(header.PointDataFormatId)
            && !variableLengthRecords.OfType<WaveformPacketDescriptor>().Any())
        {
            throw new InvalidOperationException();
        }

        static bool IsWavePointDataFormatId(byte pointDataFormatId)
        {
#if LAS1_4_OR_GREATER
            return pointDataFormatId is 4 or 5 or 9 or 10;
#else
            return pointDataFormatId is 4 or 5;
#endif
        }
#endif

#if LAS1_5_OR_GREATER
        static void CheckAny<T>(System.Globalization.CultureInfo culture, IEnumerable<VariableLengthRecord> variableLengthRecords)
            where T : VariableLengthRecord
        {
            if (variableLengthRecords.OfType<T>().Any())
            {
                throw new InvalidOperationException(string.Format(culture, Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.InvalidVlrFound), culture)!, typeof(T)));
            }
        }
#endif

        static void CheckMultiple<T>(System.Globalization.CultureInfo culture, IEnumerable<VariableLengthRecord> variableLengthRecords)
            where T : VariableLengthRecord
        {
            if (variableLengthRecords.OfType<T>().Skip(1).Any())
            {
                throw new InvalidOperationException(string.Format(culture, Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.MultipleVlrsFound), culture)!, typeof(T)));
            }
        }
    }
}