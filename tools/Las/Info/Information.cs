// -----------------------------------------------------------------------
// <copyright file="Information.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// Gets information.
/// </summary>
internal static class Information
{
    /// <summary>
    /// Gets the information for the <see cref="LasReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="LasReader"/>.</param>
    /// <returns>The information.</returns>
    public static IEnumerable<(object? Header, object? Value)> GetInformation(LasReader reader)
    {
        var header = reader.Header;
        yield return ("file signature:", header.FileSignature);
        yield return ("file source ID:", header.FileSourceId);
#if LAS1_2_OR_GREATER
        yield return ("global_encoding:", (ushort)header.GlobalEncoding);
#endif
        yield return ("project ID GUID data 1-4:", header.ProjectId);
        yield return ("version major.minor:", FormatVersion(header.Version));
        yield return ("system identifier:", header.SystemIdentifier);
        yield return ("generating software:", header.GeneratingSoftware);
        yield return ("file creation day/year:", FormatDate(header.FileCreation));
        yield return ("offset to point data:", GetValue(reader, "offsetToPointData"));
        yield return ("number var. length records:", reader.VariableLengthRecords.Count);
        yield return ("point data format:", header.PointDataFormatId);
        yield return ("point data record length:", GetValue(reader, "pointDataLength"));
#if LAS1_4_OR_GREATER
        yield return ("number of point records:", header.LegacyNumberOfPointRecords);
        yield return ("number of points by return:", FormatArray(header.LegacyNumberOfPointsByReturn));
#else
        yield return ("number of point records:", header.NumberOfPointRecords);
        yield return ("number of points by return:", FormatArray(header.NumberOfPointsByReturn));
#endif
        yield return ("scale factor x y z:", FormatPoint(header.ScaleFactor));
        yield return ("offset x y z:", FormatPoint(header.Offset));
        yield return ("min x y z:", FormatPoint(header.Min, header.ScaleFactor));
        yield return ("max x y z:", FormatPoint(header.Max, header.ScaleFactor));
#if LAS1_4_OR_GREATER
        if (reader.Header is { Version: { Major: 1, Minor: >= 4 } })
        {
            yield return ("number of extended_variable length records:", reader.ExtendedVariableLengthRecords.Count);
            yield return ("extended number of point records:", header.RawNumberOfPointRecords);
            yield return ("extended number of points by return:", FormatArray(header.RawNumberOfPointsByReturn));
        }
#endif

        static IFormattable FormatVersion(Version version)
        {
            return LazyFormattable.Create(version.ToString(2));
        }

#if NETFRAMEWORK || NETCOREAPP
        static IFormattable FormatArray<T>(IEnumerable<T> values)
            where T : IConvertible
        {
            return LazyFormattable.Create(values);
        }
#else
        static IFormattable FormatArray<T>(IEnumerable<T> values)
        {
            return LazyFormattable.Create(string.Join(" ", values));
        }
#endif

        static IFormattable FormatDate(DateTime? dateTime)
        {
            var (dayOfYear, year) = dateTime.HasValue ? (dateTime.Value.DayOfYear, dateTime.Value.Year) : default;

#if NET461_OR_GREATER || NETCOREAPP
            return LazyFormattable.Create((FormattableString)$"{dayOfYear}/{year}");
#else
            return LazyFormattable.Create("{0}/{1}", dayOfYear, year);
#endif
        }

        static IFormattable FormatPoint(Vector3D point, Vector3D? scaleFactor = default)
        {
            var x = point.X;
            var y = point.Y;
            var z = point.Z;

            if (scaleFactor is { } vector)
            {
                var format = string.Concat("{0", Format(vector.X), "} {1", Format(vector.Y), "} {2", Format(vector.Z), "}");
                return LazyFormattable.Create(format, x, y, z);
            }

#if NET46_OR_GREATER || NETCOREAPP
            return LazyFormattable.Create((FormattableString)$"{x} {y} {z}");
#else
            return LazyFormattable.Create("{0} {1} {2}", x, y, z);
#endif
        }
    }

    /// <summary>
    /// Gets the information about the <see cref="VariableLengthRecord"/>.
    /// </summary>
    /// <param name="header">The <see cref="HeaderBlock"/>.</param>
    /// <param name="vlr">The <see cref="VariableLengthRecord"/>.</param>
    /// <returns>The information.</returns>
    public static IEnumerable<(object? Header, object? Value)> GetInformation(HeaderBlock header, VariableLengthRecord vlr)
    {
        yield return ("reserved", 0);
        yield return ("user ID", vlr.Header.UserId);
        yield return ("record ID", vlr.Header.RecordId);
        yield return ("length after header", vlr.Header.RecordLengthAfterHeader);
        yield return ("description", vlr.Header.Description);

        var extra = vlr switch
        {
            GeoAsciiParamsTag geoAsciiParamsTag => GetGeoAsciiParamsTag(geoAsciiParamsTag),
            GeoDoubleParamsTag geoDoubleParamsTag => GetGeoDoubleParamsTag(geoDoubleParamsTag),
            GeoKeyDirectoryTag geoKeyEntries => GetGeoKeyDirectoryTag(geoKeyEntries),
#if LAS1_3_OR_GREATER
            WaveformPacketDescriptor waveformPacketDescriptor => GetWaveformPacketDescriptor(waveformPacketDescriptor),
#endif
#if LAS1_4_OR_GREATER
            ExtraBytes extraBytes => GetExtraBytes(extraBytes),
            OgcCoordinateSystemWkt ogcCoordinateSystemWkt => GetOgcCoordinateSystemWkt(ogcCoordinateSystemWkt),
            OgcMathTransformWkt ogcMathTransformWkt => GetOgcMathTransformWkt(ogcMathTransformWkt),
#endif
            _ => [],
        };

        foreach (var item in extra)
        {
            yield return item;
        }

#if LAS1_4_OR_GREATER
        static IEnumerable<(object? Header, object? Value)> GetExtraBytes(ExtraBytes record)
        {
            yield return (default, "Extra Byte Descriptions");
            foreach (var item in record)
            {
                yield return (default, LazyFormattable.Create(
                    formatProvider =>
                    {
                        var stringBuilder = new System.Text.StringBuilder();
                        _ = stringBuilder
                            .Append("  ")
                            .AppendFormat(formatProvider, "data type: {0} ({1})", (uint)item.DataType, GetName(item.DataType))
                            .AppendFormat(formatProvider, ", name \"{0}\"", item.Name)
                            .AppendFormat(formatProvider, ", description: \"{0}\"", item.Description);

                        if (item.Options.HasFlag(ExtraBytesOptions.Min))
                        {
                            _ = stringBuilder.AppendFormat(formatProvider, ", min: {0}", item.Min);
                        }

                        if (item.Options.HasFlag(ExtraBytesOptions.Max))
                        {
                            _ = stringBuilder.AppendFormat(formatProvider, ", max: {0}", item.Max);
                        }

                        _ = stringBuilder.Append(", scale: ");
                        _ = item.HasScale
                            ? stringBuilder.AppendFormat(formatProvider, "{0}", item.Scale)
                            : stringBuilder.AppendFormat(formatProvider, "{0} (not set)", 1);

                        _ = stringBuilder.Append(", offset: ");
                        _ = item.HasOffset
                            ? stringBuilder.AppendFormat(formatProvider, "{0}", item.Offset)
                            : stringBuilder.AppendFormat(formatProvider, "{0} (not set)", 0);

                        return stringBuilder.ToString();

                        static string GetName(ExtraBytesDataType dataType)
                        {
                            return dataType switch
                            {
                                ExtraBytesDataType.UnsignedChar => "unsigned char",
                                ExtraBytesDataType.Char => "char",
                                ExtraBytesDataType.UnsignedShort => "unsigned short",
                                ExtraBytesDataType.Short => "short",
                                ExtraBytesDataType.UnsignedLong => "unsigned long",
                                ExtraBytesDataType.Long => "long",
                                ExtraBytesDataType.UnsignedLongLong => "unsigned long long",
                                ExtraBytesDataType.LongLong => "long long",
                                ExtraBytesDataType.Float => "float",
                                ExtraBytesDataType.Double => "double",
                                ExtraBytesDataType.Undocumented => "undocumented",
                                _ => "invalid",
                            };
                        }
                    }));
            }
        }
#endif

        static IEnumerable<(object? Header, object? Value)> GetGeoAsciiParamsTag(GeoAsciiParamsTag record)
        {
            yield return (default, LazyFormattable.Create("GeoAsciiParamsTag (number of characters {0})", record.Header.RecordLengthAfterHeader));
            var stringBuilder = new System.Text.StringBuilder();
            foreach (var item in record)
            {
                stringBuilder.Append(item);
                stringBuilder.Append('|');
            }

            yield return (default, LazyFormattable.Create("  " + stringBuilder));
        }

        static IEnumerable<(object? Header, object? Value)> GetGeoDoubleParamsTag(GeoDoubleParamsTag record)
        {
            yield return (default, LazyFormattable.Create("GeoDoubleParamsTag  (number of doubles {0})", record.Count));
#if NETFRAMEWORK || NETCOREAPP
            yield return (default, LazyFormattable.Create(GetEnumerable()));
#else
            yield return (default, LazyFormattable.Create(GetEnumerable(), (formatProvider, value) => value.ToString(formatProvider)));
#endif

            IEnumerable<double> GetEnumerable()
            {
                var count = record.Count;
                for (var i = 0; i < count; i++)
                {
                    yield return record[i];
                }
            }
        }

        static IEnumerable<(object? Header, object? Value)> GetGeoKeyDirectoryTag(GeoKeyDirectoryTag record)
        {
            var version = record.Version;
            yield return (default, LazyFormattable.Create("GeoKeyDirectoryTag version {0} number of keys {1}", version, record.Count));
            foreach (var key in record)
            {
                yield return (default, LazyFormattable.Create("  key {0} tiff_tag_location {1} count {2} value_offset {3} - {4}: {5}", (ushort)key.KeyId, key.TiffTagLocation, key.Count, key.ValueOffset, key.KeyId, GetValueCore(key)));
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "Checked")]
            static object GetValueCore(GeoKeyEntry geoKeyEntry)
            {
                return geoKeyEntry switch
                {
                    { KeyId: GeoKey.GTModelTypeGeoKey, ValueOffset: 0 } => "ModelTypeUndefined",
                    { KeyId: GeoKey.GTModelTypeGeoKey, ValueOffset: 1 } => "ModelTypeProjected",
                    { KeyId: GeoKey.GTModelTypeGeoKey, ValueOffset: 2 } => "ModelTypeGeographic",
                    { KeyId: GeoKey.GTModelTypeGeoKey, ValueOffset: 3 } => "ModelTypeGeocentric",
                    { KeyId: GeoKey.GTRasterTypeGeoKey, ValueOffset: 1 } => "RasterPixelIsArea",
                    { KeyId: GeoKey.GTRasterTypeGeoKey, ValueOffset: 2 } => "RasterPixelIsPoint",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 32767 } => "user-defined",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4001 } => "GCSE_Airy1830",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4002 } => "GCSE_AiryModified1849 ",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4003 } => "GCSE_AustralianNationalSpheroid",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4004 } => "GCSE_Bessel1841",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4005 } => "GCSE_Bessel1841Modified",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4006 } => "GCSE_BesselNamibia",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4008 } => "GCSE_Clarke1866",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4009 } => "GCSE_Clarke1866Michigan",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4010 } => "GCSE_Clarke1880_Benoit",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4011 } => "GCSE_Clarke1880_IGN",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4012 } => "GCSE_Clarke1880_RGS",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4013 } => "GCSE_Clarke1880_Arc",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4014 } => "GCSE_Clarke1880_SGA1922",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4015 } => "GCSE_Everest1830_1937Adjustment",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4016 } => "GCSE_Everest1830_1967Definition",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4017 } => "GCSE_Everest1830_1975Definition",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4018 } => "GCSE_Everest1830Modified",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4019 } => "GCSE_GRS1980",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4020 } => "GCSE_Helmert1906",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4022 } => "GCSE_International1924",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4023 } => "GCSE_International1967",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4024 } => "GCSE_Krassowsky1940",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4030 } => "GCSE_WGS84",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4034 } => "GCSE_Clarke1880",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4140 } => "GCSE_NAD83_CSRS",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4167 } => "GCSE_New_Zealand_Geodetic_Datum_2000",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4267 } => "GCS_NAD27",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4269 } => "GCS_NAD83",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4283 } => "GCS_GDA94",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4312 } => "GCS_MGI",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4322 } => "GCS_WGS_72",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4326 } => "GCS_WGS_84",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4289 } => "GCS_Amersfoort",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4617 } => "GCS_NAD83_CSRS",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 4619 } => "GCS_SWEREF99",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 6318 } => "GCS_NAD83_2011",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 6322 } => "GCS_NAD83_PA11",
                    { KeyId: GeoKey.GeodeticCRSGeoKey, ValueOffset: 7844 } => "GCS_GDA2020",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 32767 } => "user-defined",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6202 } => "Datum_Australian_Geodetic_Datum_1966",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6203 } => "Datum_Australian_Geodetic_Datum_1984",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6267 } => "Datum_North_American_Datum_1927",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6269 } => "Datum_North_American_Datum_1983",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6283 } => "Datum_Geocentric_Datum_of_Australia_1994",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6322 } => "Datum_WGS72",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6326 } => "Datum_WGS84",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6140 } => "Datum_WGS84",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6619 } => "Datum_SWEREF99",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6289 } => "Datum_Amersfoort",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6167 } => "Datum_NZGD2000",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6001 } => "DatumE_Airy1830",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6002 } => "DatumE_AiryModified1849",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6003 } => "DatumE_AustralianNationalSpheroid",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6004 } => "DatumE_Bessel1841",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6005 } => "DatumE_BesselModified",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6006 } => "DatumE_BesselNamibia",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6008 } => "DatumE_Clarke1866",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6009 } => "DatumE_Clarke1866Michigan",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6010 } => "DatumE_Clarke1880_Benoit",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6011 } => "DatumE_Clarke1880_IGN",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6012 } => "DatumE_Clarke1880_RGS",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6013 } => "DatumE_Clarke1880_Arc",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6014 } => "DatumE_Clarke1880_SGA1922",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6015 } => "DatumE_Everest1830_1937Adjustment",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6016 } => "DatumE_Everest1830_1967Definition",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6017 } => "DatumE_Everest1830_1975Definition",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6018 } => "DatumE_Everest1830Modified",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6019 } => "DatumE_GRS1980",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6020 } => "DatumE_Helmert1906",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6022 } => "DatumE_International1924",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6023 } => "DatumE_International1967",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6024 } => "DatumE_Krassowsky1940",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6030 } => "DatumE_WGS84",
                    { KeyId: GeoKey.GeodeticDatumGeoKey, ValueOffset: 6034 } => "DatumE_Clarke1880",
                    { KeyId: GeoKey.PrimeMeridianGeoKey, ValueOffset: 32767 } => "user-defined",
                    { KeyId: GeoKey.PrimeMeridianGeoKey, ValueOffset: 8901 } => "PM_Greenwich",
                    { KeyId: GeoKey.PrimeMeridianGeoKey, ValueOffset: 8902 } => "PM_Lisbon",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9001 } => "Linear_Meter",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9002 } => "Linear_Foot",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9003 } => "Linear_Foot_US_Survey",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9004 } => "Linear_Foot_Modified_American",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9005 } => "Linear_Foot_Clarke",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9006 } => "Linear_Foot_Indian",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9007 } => "Linear_Link",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9008 } => "Linear_Link_Benoit",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9009 } => "Linear_Link_Sears",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9010 } => "Linear_Chain_Benoit",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9011 } => "Linear_Chain_Sears",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9012 } => "Linear_Yard_Sears",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9013 } => "Linear_Yard_Indian",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9014 } => "Linear_Fathom",
                    { KeyId: GeoKey.GeogLinearUnitsGeoKey or GeoKey.ProjLinearUnitsGeoKey or GeoKey.VerticalUnitsGeoKey, ValueOffset: 9015 } => "Linear_Mile_International_Nautical",
                    { KeyId: GeoKey.GeogAngularUnitsGeoKey or GeoKey.GeogAzimuthUnitsGeoKey, ValueOffset: 9101 } => "Angular_Radian",
                    { KeyId: GeoKey.GeogAngularUnitsGeoKey or GeoKey.GeogAzimuthUnitsGeoKey, ValueOffset: 9102 } => "Angular_Degree",
                    { KeyId: GeoKey.GeogAngularUnitsGeoKey or GeoKey.GeogAzimuthUnitsGeoKey, ValueOffset: 9103 } => "Angular_Arc_Minute",
                    { KeyId: GeoKey.GeogAngularUnitsGeoKey or GeoKey.GeogAzimuthUnitsGeoKey, ValueOffset: 9104 } => "Angular_Arc_Second",
                    { KeyId: GeoKey.GeogAngularUnitsGeoKey or GeoKey.GeogAzimuthUnitsGeoKey, ValueOffset: 9105 } => "Angular_Grad",
                    { KeyId: GeoKey.GeogAngularUnitsGeoKey or GeoKey.GeogAzimuthUnitsGeoKey, ValueOffset: 9106 } => "Angular_Gon",
                    { KeyId: GeoKey.GeogAngularUnitsGeoKey or GeoKey.GeogAzimuthUnitsGeoKey, ValueOffset: 9107 } => "Angular_DMS",
                    { KeyId: GeoKey.GeogAngularUnitsGeoKey or GeoKey.GeogAzimuthUnitsGeoKey, ValueOffset: 9108 } => "Angular_DMS_Hemisphere",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 32767 } => "user-defined",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7001 } => "Ellipse_Airy_1830",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7002 } => "Ellipse_Airy_Modified_1849",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7003 } => "Ellipse_Australian_National_Spheroid",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7004 } => "Ellipse_Bessel_1841",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7005 } => "Ellipse_Bessel_Modified",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7006 } => "Ellipse_Bessel_Namibia",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7008 } => "Ellipse_Clarke_1866",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7009 } => "Ellipse_Clarke_1866_Michigan",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7010 } => "Ellipse_Clarke1880_Benoit",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7011 } => "Ellipse_Clarke1880_IGN",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7012 } => "Ellipse_Clarke1880_RGS",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7013 } => "Ellipse_Clarke1880_Arc",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7014 } => "Ellipse_Clarke1880_SGA1922",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7015 } => "Ellipse_Everest1830_1937Adjustment",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7016 } => "Ellipse_Everest1830_1967Definition",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7017 } => "Ellipse_Everest1830_1975Definition",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7018 } => "Ellipse_Everest1830Modified",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7019 } => "Ellipse_GRS_1980",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7020 } => "Ellipse_Helmert1906",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7022 } => "Ellipse_International1924",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7023 } => "Ellipse_International1967",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7024 } => "Ellipse_Krassowsky1940",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7030 } => "Ellipse_WGS_84",
                    { KeyId: GeoKey.EllipsoidGeoKey, ValueOffset: 7034 } => "Ellipse_Clarke_1880",
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: 32767 } => "user-defined",
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 32601 and <= 32660 } v => FormatPcs("WGS 84", v.ValueOffset - 32600),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 32701 and <= 32760 } v => FormatPcs("WGS 84", v.ValueOffset - 32700, south: true),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 25828 and <= 25838 } v => FormatPcs("ETRS89", v.ValueOffset - 25800),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 26903 and <= 26923 } v => FormatPcs("NAD83", v.ValueOffset - 26900),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 3154 and <= 3160 } v => FormatPcs("NAD83(CSRS)", v.ValueOffset < 3158 ? v.ValueOffset - 3154 + 7 : v.ValueOffset - 3158 + 14),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 7846 and <= 7859 } v => FormatPcs("GDA2020", v.ValueOffset - 7800, south: true, "MGA"),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 28348 and <= 28358 } v => FormatPcs("GDA94", v.ValueOffset - 28300, south: true, "MGA"),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 29118 and <= 29118 } v => FormatPcs("SAD69", v.ValueOffset - 29100),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 29177 and <= 29185 } v => FormatPcs("SAD69", v.ValueOffset - 29160, south: true),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 32201 and <= 32260 } v => FormatPcs("WGS 72", v.ValueOffset - 32200),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 32301 and <= 32360 } v => FormatPcs("WGS 72", v.ValueOffset - 32300, south: true),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 32401 and <= 32460 } v => FormatPcs("WGS 72BE", v.ValueOffset - 32400),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 32501 and <= 32560 } v => FormatPcs("WGS 72BE", v.ValueOffset - 32500, south: true),
                    { KeyId: GeoKey.ProjectedCRSGeoKey, ValueOffset: >= 26703 and <= 26723 } v => FormatPcs("NAD27", v.ValueOffset - 26700),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: >= 16001 and <= 16060 } v => LazyFormattable.Create("Proj_UTM_zone_{0}N", v.ValueOffset - 16000),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: >= 16101 and <= 16160 } v => LazyFormattable.Create("Proj_UTM_zone_{0}S", v.ValueOffset - 16100),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 32767 } => "user-defined",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10101 } => "Proj_Alabama_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10102 } => "Proj_Alabama_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10131 } => "Proj_Alabama_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10132 } => "Proj_Alabama_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10201 } => "Proj_Arizona_Coordinate_System_east",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10202 } => "Proj_Arizona_Coordinate_System_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10203 } => "Proj_Arizona_Coordinate_System_west",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10231 } => "Proj_Arizona_CS83_east",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10232 } => "Proj_Arizona_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10233 } => "Proj_Arizona_CS83_west",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10301 } => "Proj_Arkansas_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10302 } => "Proj_Arkansas_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10331 } => "Proj_Arkansas_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10332 } => "Proj_Arkansas_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10401 } => "Proj_California_CS27_I",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10402 } => "Proj_California_CS27_II",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10403 } => "Proj_California_CS27_III",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10404 } => "Proj_California_CS27_IV",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10405 } => "Proj_California_CS27_V",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10406 } => "Proj_California_CS27_VI",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10407 } => "Proj_California_CS27_VII",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: >= 10431 and <= 10436 } v => FormatProjection("California_CS83", v.ValueOffset - 10430),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10501 } => "Proj_Colorado_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10502 } => "Proj_Colorado_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10503 } => "Proj_Colorado_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10531 } => "Proj_Colorado_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10532 } => "Proj_Colorado_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10533 } => "Proj_Colorado_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10600 } => "Proj_Connecticut_CS27",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10630 } => "Proj_Connecticut_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10700 } => "Proj_Delaware_CS27",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10730 } => "Proj_Delaware_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10901 } => "Proj_Florida_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10902 } => "Proj_Florida_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10903 } => "Proj_Florida_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10931 } => "Proj_Florida_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10932 } => "Proj_Florida_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 10933 } => "Proj_Florida_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11001 } => "Proj_Georgia_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11002 } => "Proj_Georgia_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11031 } => "Proj_Georgia_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11032 } => "Proj_Georgia_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11101 } => "Proj_Idaho_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11102 } => "Proj_Idaho_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11103 } => "Proj_Idaho_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11131 } => "Proj_Idaho_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11132 } => "Proj_Idaho_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11133 } => "Proj_Idaho_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11201 } => "Proj_Illinois_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11202 } => "Proj_Illinois_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11231 } => "Proj_Illinois_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11232 } => "Proj_Illinois_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11301 } => "Proj_Indiana_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11302 } => "Proj_Indiana_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11331 } => "Proj_Indiana_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11332 } => "Proj_Indiana_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11401 } => "Proj_Iowa_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11402 } => "Proj_Iowa_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11431 } => "Proj_Iowa_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11432 } => "Proj_Iowa_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11501 } => "Proj_Kansas_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11502 } => "Proj_Kansas_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11531 } => "Proj_Kansas_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11532 } => "Proj_Kansas_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11601 } => "Proj_Kentucky_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11602 } => "Proj_Kentucky_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11631 } => "Proj_Kentucky_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11632 } => "Proj_Kentucky_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11701 } => "Proj_Louisiana_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11702 } => "Proj_Louisiana_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11731 } => "Proj_Louisiana_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11732 } => "Proj_Louisiana_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11801 } => "Proj_Maine_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11802 } => "Proj_Maine_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11831 } => "Proj_Maine_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11832 } => "Proj_Maine_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11900 } => "Proj_Maryland_CS27",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 11930 } => "Proj_Maryland_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12001 } => "Proj_Massachusetts_CS27_Mainland",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12002 } => "Proj_Massachusetts_CS27_Island",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12031 } => "Proj_Massachusetts_CS83_Mainland",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12032 } => "Proj_Massachusetts_CS83_Island",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12101 } => "Proj_Michigan_State_Plane_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12102 } => "Proj_Michigan_State_Plane_Old_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12103 } => "Proj_Michigan_State_Plane_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12111 } => "Proj_Michigan_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12112 } => "Proj_Michigan_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12113 } => "Proj_Michigan_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12141 } => "Proj_Michigan_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12142 } => "Proj_Michigan_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12143 } => "Proj_Michigan_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12201 } => "Proj_Minnesota_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12202 } => "Proj_Minnesota_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12203 } => "Proj_Minnesota_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12231 } => "Proj_Minnesota_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12232 } => "Proj_Minnesota_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12233 } => "Proj_Minnesota_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12301 } => "Proj_Mississippi_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12302 } => "Proj_Mississippi_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12331 } => "Proj_Mississippi_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12332 } => "Proj_Mississippi_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12401 } => "Proj_Missouri_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12402 } => "Proj_Missouri_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12403 } => "Proj_Missouri_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12431 } => "Proj_Missouri_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12432 } => "Proj_Missouri_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12433 } => "Proj_Missouri_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12501 } => "Proj_Montana_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12502 } => "Proj_Montana_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12503 } => "Proj_Montana_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12530 } => "Proj_Montana_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12601 } => "Proj_Nebraska_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12602 } => "Proj_Nebraska_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12630 } => "Proj_Nebraska_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12701 } => "Proj_Nevada_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12702 } => "Proj_Nevada_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12703 } => "Proj_Nevada_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12731 } => "Proj_Nevada_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12732 } => "Proj_Nevada_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12733 } => "Proj_Nevada_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12800 } => "Proj_New_Hampshire_CS27",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12830 } => "Proj_New_Hampshire_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12900 } => "Proj_New_Jersey_CS27",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 12930 } => "Proj_New_Jersey_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13001 } => "Proj_New_Mexico_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13002 } => "Proj_New_Mexico_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13003 } => "Proj_New_Mexico_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13031 } => "Proj_New_Mexico_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13032 } => "Proj_New_Mexico_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13033 } => "Proj_New_Mexico_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13101 } => "Proj_New_York_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13102 } => "Proj_New_York_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13103 } => "Proj_New_York_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13104 } => "Proj_New_York_CS27_Long_Island",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13131 } => "Proj_New_York_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13132 } => "Proj_New_York_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13133 } => "Proj_New_York_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13134 } => "Proj_New_York_CS83_Long_Island",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13200 } => "Proj_North_Carolina_CS27",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13230 } => "Proj_North_Carolina_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13301 } => "Proj_North_Dakota_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13302 } => "Proj_North_Dakota_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13331 } => "Proj_North_Dakota_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13332 } => "Proj_North_Dakota_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13401 } => "Proj_Ohio_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13402 } => "Proj_Ohio_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13431 } => "Proj_Ohio_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13432 } => "Proj_Ohio_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13501 } => "Proj_Oklahoma_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13502 } => "Proj_Oklahoma_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13531 } => "Proj_Oklahoma_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13532 } => "Proj_Oklahoma_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13601 } => "Proj_Oregon_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13602 } => "Proj_Oregon_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13631 } => "Proj_Oregon_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13632 } => "Proj_Oregon_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13701 } => "Proj_Pennsylvania_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13702 } => "Proj_Pennsylvania_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13731 } => "Proj_Pennsylvania_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13732 } => "Proj_Pennsylvania_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13800 } => "Proj_Rhode_Island_CS27",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13830 } => "Proj_Rhode_Island_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13901 } => "Proj_South_Carolina_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13902 } => "Proj_South_Carolina_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 13930 } => "Proj_South_Carolina_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14001 } => "Proj_South_Dakota_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14002 } => "Proj_South_Dakota_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14031 } => "Proj_South_Dakota_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14032 } => "Proj_South_Dakota_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14100 } => "Proj_Tennessee_CS27",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14130 } => "Proj_Tennessee_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14201 } => "Proj_Texas_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14202 } => "Proj_Texas_CS27_North_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14203 } => "Proj_Texas_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14204 } => "Proj_Texas_CS27_South_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14205 } => "Proj_Texas_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14231 } => "Proj_Texas_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14232 } => "Proj_Texas_CS83_North_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14233 } => "Proj_Texas_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14234 } => "Proj_Texas_CS83_South_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14235 } => "Proj_Texas_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14301 } => "Proj_Utah_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14302 } => "Proj_Utah_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14303 } => "Proj_Utah_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14331 } => "Proj_Utah_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14332 } => "Proj_Utah_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14333 } => "Proj_Utah_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14400 } => "Proj_Vermont_CS27",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14430 } => "Proj_Vermont_CS83",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14501 } => "Proj_Virginia_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14502 } => "Proj_Virginia_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14531 } => "Proj_Virginia_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14532 } => "Proj_Virginia_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14601 } => "Proj_Washington_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14602 } => "Proj_Washington_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14631 } => "Proj_Washington_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14632 } => "Proj_Washington_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14701 } => "Proj_West_Virginia_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14702 } => "Proj_West_Virginia_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14731 } => "Proj_West_Virginia_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14732 } => "Proj_West_Virginia_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14801 } => "Proj_Wisconsin_CS27_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14802 } => "Proj_Wisconsin_CS27_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14803 } => "Proj_Wisconsin_CS27_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14831 } => "Proj_Wisconsin_CS83_North",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14832 } => "Proj_Wisconsin_CS83_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14833 } => "Proj_Wisconsin_CS83_South",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14901 } => "Proj_Wyoming_CS27_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14902 } => "Proj_Wyoming_CS27_East_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14903 } => "Proj_Wyoming_CS27_West_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14904 } => "Proj_Wyoming_CS27_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14931 } => "Proj_Wyoming_CS83_East",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14932 } => "Proj_Wyoming_CS83_East_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14933 } => "Proj_Wyoming_CS83_West_Central",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 14934 } => "Proj_Wyoming_CS83_West",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: >= 15001 and <= 15010 } v => FormatProjection("Alaska_CS27", v.ValueOffset - 15000),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: >= 15031 and <= 15040 } v => FormatProjection("Alaska_CS83", v.ValueOffset - 15030),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: >= 15101 and <= 15105 } v => FormatProjection("Hawaii_CS27", v.ValueOffset - 15100),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: >= 15131 and <= 15135 } v => FormatProjection("Hawaii_CS83", v.ValueOffset - 15130),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 15201 } => "Proj_Puerto_Rico_CS27",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 15202 } => "Proj_St_Croix",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 15230 } => "Proj_Puerto_Rico_Virgin_Is",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 15914 } => "Proj_BLM_14N_feet",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 15915 } => "Proj_BLM_15N_feet",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 15916 } => "Proj_BLM_16N_feet",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 15917 } => "Proj_BLM_17N_feet",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 17333 } => "Proj_SWEREF99_TM",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: >= 17348 and <= 17358 } v => FormatProjection("Map_Grid_of_Australia", v.ValueOffset - 17300),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: >= 17448 and <= 17458 } v => FormatProjection("Australian_Map_Grid", v.ValueOffset - 17400),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: >= 18031 and <= 18037 } v => FormatProjection("Argentina", v.ValueOffset - 18030),
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 18051 } => "Proj_Colombia_3W",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 18052 } => "Proj_Colombia_Bogota",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 18053 } => "Proj_Colombia_3E",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 18054 } => "Proj_Colombia_6E",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 18072 } => "Proj_Egypt_Red_Belt",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 18073 } => "Proj_Egypt_Purple_Belt",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 18074 } => "Proj_Extended_Purple_Belt",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 18141 } => "Proj_New_Zealand_North_Island_Nat_Grid",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 18142 } => "Proj_New_Zealand_South_Island_Nat_Grid",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 19900 } => "Proj_Bahrain_Grid",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 19905 } => "Proj_Netherlands_E_Indies_Equatorial",
                    { KeyId: GeoKey.ProjectionGeoKey, ValueOffset: 19912 } => "Proj_RSO_Borneo",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 1 } => "CT_TransverseMercator",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 2 } => "CT_TransvMercator_Modified_Alaska",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 3 } => "CT_ObliqueMercator",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 4 } => "CT_ObliqueMercator_Laborde",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 5 } => "CT_ObliqueMercator_Rosenmund",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 6 } => "CT_ObliqueMercator_Spherical",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 7 } => "CT_Mercator",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 8 } => "CT_LambertConfConic_2SP",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 9 } => "CT_LambertConfConic_Helmert",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 10 } => "CT_LambertAzimEqualArea",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 11 } => "CT_AlbersEqualArea",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 12 } => "CT_AzimuthalEquidistant",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 13 } => "CT_EquidistantConic",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 14 } => "CT_Stereographic",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 15 } => "CT_PolarStereographic",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 16 } => "CT_ObliqueStereographic",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 17 } => "CT_Equirectangular",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 18 } => "CT_CassiniSoldner",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 19 } => "CT_Gnomonic",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 20 } => "CT_MillerCylindrical",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 21 } => "CT_Orthographic",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 22 } => "CT_Polyconic",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 23 } => "CT_Robinson",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 24 } => "CT_Sinusoidal",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 25 } => "CT_VanDerGrinten",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 26 } => "CT_NewZealandMapGrid",
                    { KeyId: GeoKey.ProjMethodGeoKey, ValueOffset: 27 } => "CT_TransvMercator_SouthOriented",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 1127 } => "VertCS_Canadian_Geodetic_Vertical_Datum_2013",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5001 } => "VertCS_Airy_1830_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5002 } => "VertCS_Airy_Modified_1849_ellipsoid ",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5003 } => "VertCS_ANS_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5004 } => "VertCS_Bessel_1841_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5005 } => "VertCS_Bessel_Modified_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5006 } => "VertCS_Bessel_Namibia_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5007 } => "VertCS_Clarke_1858_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5008 } => "VertCS_Clarke_1866_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5010 } => "VertCS_Clarke_1880_Benoit_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5011 } => "VertCS_Clarke_1880_IGN_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5012 } => "VertCS_Clarke_1880_RGS_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5013 } => "VertCS_Clarke_1880_Arc_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5014 } => "VertCS_Clarke_1880_SGA_1922_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5015 } => "VertCS_Everest_1830_1937_Adjustment_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5016 } => "VertCS_Everest_1830_1967_Definition_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5017 } => "VertCS_Everest_1830_1975_Definition_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5018 } => "VertCS_Everest_1830_Modified_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5019 } => "VertCS_GRS_1980_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5020 } => "VertCS_Helmert_1906_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5021 } => "VertCS_INS_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5022 } => "VertCS_International_1924_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5023 } => "VertCS_International_1967_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5024 } => "VertCS_Krassowsky_1940_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5025 } => "VertCS_NWL_9D_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5026 } => "VertCS_NWL_10D_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5027 } => "VertCS_Plessis_1817_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5028 } => "VertCS_Struve_1860_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5029 } => "VertCS_War_Office_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5030 } => "VertCS_WGS_84_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5031 } => "VertCS_GEM_10C_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5032 } => "VertCS_OSU86F_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5033 } => "VertCS_OSU91A_ellipsoid",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5101 } => "VertCS_Newlyn",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5102 } => "VertCS_North_American_Vertical_Datum_1929",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5103 } => "VertCS_North_American_Vertical_Datum_1988",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5104 } => "VertCS_Yellow_Sea_1956",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5105 } => "VertCS_Baltic_Sea",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5106 } => "VertCS_Caspian_Sea",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5114 } => "VertCS_Canadian_Geodetic_Vertical_Datum_1928",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5206 } => "VertCS_Dansk_Vertikal_Reference_1990",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5215 } => "VertCS_European_Vertical_Reference_Frame_2007",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5701 } => "ODN height (Reserved EPSG)",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5702 } => "NGVD29 height (Reserved EPSG)",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5703 } => "NAVD88 height (Reserved EPSG)",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5704 } => "Yellow Sea (Reserved EPSG)",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5705 } => "Baltic height (Reserved EPSG)",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5706 } => "Caspian depth (Reserved EPSG)",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5707 } => "NAP height (Reserved EPSG)",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5710 } => "Oostende height (Reserved EPSG)",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5711 } => "AHD height (Reserved EPSG)",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5712 } => "AHD (Tasmania) height (Reserved EPSG)",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5776 } => "Norway Normal Null 1954",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5783 } => "Deutches Haupthohennetz 1992",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 5941 } => "Norway Normal Null 2000",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 6647 } => "Canadian Geodetic Vertical Datum of 2013",
                    { KeyId: GeoKey.VerticalGeoKey, ValueOffset: 7837 } => "Deutches Haupthohennetz 2016",
                    { KeyId: GeoKey.VerticalDatumGeoKey, ValueOffset: var value } => LazyFormattable.Create("Vertical Datum Codes {0}", value),
                    { TiffTagLocation: GeoDoubleParamsTag.TagRecordId or GeoAsciiParamsTag.TagRecordId } => geoKeyEntry,
                    { ValueOffset: var value } => LazyFormattable.Create("{0}: look-up for {1} not implemented", geoKeyEntry.KeyId, value),
                };

                static IFormattable FormatPcs(string name, int zone, bool south = false, string utm = "UTM")
                {
                    return LazyFormattable.Create("{0} / {1} {2}{3}", name, utm, zone, south ? 'S' : 'N');
                }

                static IFormattable FormatProjection(string name, int zone)
                {
                    return LazyFormattable.Create("Proj_{0}_{1}", name, zone);
                }
            }
        }

#if LAS1_4_OR_GREATER
        static IEnumerable<(object? Header, object? Value)> GetOgcCoordinateSystemWkt(OgcCoordinateSystemWkt record)
        {
            yield return (default, "WKT OGC COORDINATE SYSTEM:");
            yield return (default, record.Wkt);
        }

        static IEnumerable<(object? Header, object? Value)> GetOgcMathTransformWkt(OgcMathTransformWkt record)
        {
            yield return (default, "WKT OGC MATH TRANSFORM:");
            yield return (default, record.Wkt);
        }
#endif

#if LAS1_3_OR_GREATER
        IEnumerable<(object? Header, object? Value)> GetWaveformPacketDescriptor(WaveformPacketDescriptor record)
        {
            yield return (string.Empty, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"index {record.Header.RecordId - WaveformPacketDescriptor.MinTagRecordId + 1} bits/sample {record.BitsPerSample} compression {record.WaveformCompressionType} samples {record.NumberOfSamples} temporal {record.TemporalSampleSpacing} gain {record.DigitizerGain}, offset {record.DigitizerOffset}"));
        }
#endif
    }

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Gets the information about the <see cref="ExtendedVariableLengthRecord"/>.
    /// </summary>
    /// <param name="evlr">The <see cref="ExtendedVariableLengthRecord"/>.</param>
    /// <returns>The information.</returns>
    public static IEnumerable<(object? Header, object? Value)> GetInformation(ExtendedVariableLengthRecord evlr)
    {
        yield return ("reserved", 0);
        yield return ("user ID", evlr.Header.UserId);
        yield return ("record ID", evlr.Header.RecordId);
        yield return ("length after header", evlr.Header.RecordLengthAfterHeader);
        yield return ("description", evlr.Header.Description);
    }
#endif

    /// <summary>
    /// Gets the value from the reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="name">The name.</param>
    /// <returns>The value.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "This is by design.")]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static object? GetValue(LasReader reader, string name)
    {
        return GetField(reader.GetType(), name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(reader);

        static System.Reflection.FieldInfo? GetField(Type type, string name, System.Reflection.BindingFlags flags)
        {
            var currentType = type;
            while (currentType is not null)
            {
                if (currentType.GetField(name, flags) is { } fieldInfo)
                {
                    return fieldInfo;
                }

                currentType = currentType.BaseType;
            }

            return default;
        }
    }

    private static string Format(double precision) => precision switch
    {
        0.1 or 0.5 => ":0.0",
        0.01 or 0.25 => ":0.00",
        0.001 or 0.002 or 0.005 or 0.025 or 0.125 => ":0.000",
        0.0001 or 0.0002 or 0.0005 or 0.0025 => ":0.0000",
        0.00001 or 0.00002 or 0.00005 or 0.00025 => ":0.00000",
        0.000001 => ":0.000000",
        0.0000001 => ":0.0000000",
        0.00000001 => ":0.00000000",
        _ => string.Empty,
    };
}