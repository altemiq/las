// -----------------------------------------------------------------------
// <copyright file="DefaultLasReaderFormatter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The default LAS reader formatter.
/// </summary>
/// <param name="builder">The builder.</param>
internal class DefaultLasReaderFormatter(IFormatBuilder builder) : ILasReaderFormatter
{
    /// <inheritdoc />
    public ILasReaderFormatter AppendHeader(LasReader reader)
    {
        builder.AppendMajorHeader("reporting all LAS header entries:").AppendLine();
        foreach (var (header, value) in GetInformation(reader))
        {
            if (header is null)
            {
                // just data
                builder.AppendFormat("    {0}", value);
            }
            else if (value is string stringValue)
            {
                builder.AppendHeader($"{header,-27}").AppendFormat(format: " '{0}'", stringValue);
            }
            else
            {
                builder.AppendHeader($"{header,-27}").AppendFormat(" {0}", value);
            }

            builder.AppendLine();
        }

        return this;
    }

    /// <inheritdoc />
    public ILasReaderFormatter AppendVariableLengthRecords(LasReader reader)
    {
        for (var i = 0; i < reader.VariableLengthRecords.Count; i++)
        {
            builder.AppendMajorHeader("variable length header record {0} of {1}:", i + 1, reader.VariableLengthRecords.Count).AppendLine();
            foreach (var (header, value) in GetInformation(reader, reader.VariableLengthRecords[i]))
            {
                switch (header)
                {
                    case null:
                        // just data
                        builder.AppendFormat(
                            "    {0}",
                            value);
                        break;
                    case string { Length: 0 }:
                        // just information
                        builder.AppendFormat(
                            "  {0}",
                            value);
                        break;
                    default:
                        {
                            builder.AppendHeader($"  {header,-20}");
                            if (value is string stringValue)
                            {
                                builder.AppendFormat(
                                    format: " '{0}'",
                                    stringValue);
                            }
                            else
                            {
                                builder.AppendFormat(
                                    " {0}",
                                    value);
                            }

                            break;
                        }
                }

                builder.AppendLine();
            }
        }

        return this;
    }

#if LAS1_4_OR_GREATER
    /// <inheritdoc />
    public ILasReaderFormatter AppendExtendedVariableLengthRecords(LasReader reader)
    {
        for (var i = 0; i < reader.ExtendedVariableLengthRecords.Count; i++)
        {
            builder.AppendMajorHeader("extended variable length header record {0} of {1}:", i + 1, reader.ExtendedVariableLengthRecords.Count).AppendLine();
            foreach (var (header, value) in GetInformation(reader.ExtendedVariableLengthRecords[i]))
            {
                if (header is null)
                {
                    // just data
                    builder.AppendFormat("    {0}", value);
                }
                else if (value is string stringValue)
                {
                    builder
                        .Append($"  {header,-20}")
                        .AppendFormat(format: " '{0}'", stringValue);
                }
                else
                {
                    builder
                        .Append($"  {header,-20}")
                        .AppendFormat(" {0}", value);
                }

                builder.AppendLine();
            }
        }

        return this;
    }
#endif

    /// <inheritdoc/>
    public ILasReaderFormatter AppendStatistics(LasReader reader, Func<LasReader, Statistics> statisticsFunc)
    {
        _ = builder.AppendMajorHeader("reporting minimum and maximum for all LAS point record entries ...").AppendLine();

        var statistics = statisticsFunc(reader);

        _ = builder.AppendHeader("  X").AppendFormat("{0,20}{1,11}", statistics.X.Minimum, statistics.X.Maximum).AppendLine()
            .AppendHeader("  Y").AppendFormat("{0,20}{1,11}", statistics.Y.Minimum, statistics.Y.Maximum).AppendLine()
            .AppendHeader("  Z").AppendFormat("{0,20}{1,11}", statistics.Z.Minimum, statistics.Z.Maximum).AppendLine()
            .AppendHeader("  intensity").AppendFormat("{0,12}{1,11}", statistics.Intensity.Minimum, statistics.Intensity.Maximum).AppendLine()
            .AppendHeader("  return_number").AppendFormat("{0,8}{1,11}", statistics.ReturnNumber.Minimum, statistics.ReturnNumber.Maximum).AppendLine()
            .AppendHeader("  number_of_returns").AppendFormat("{0,4}{1,11}", statistics.NumberOfReturns.Minimum, statistics.NumberOfReturns.Maximum).AppendLine()
            .AppendHeader("  edge_of_flight_line").AppendFormat("{0,2}{1,11}", 0, statistics.EdgeOfFlightLine ? 1 : 0).AppendLine()
            .AppendHeader("  scan_direction_flag").AppendFormat("{0,2}{1,11}", 0, statistics.ScanDirectionFlag ? 1 : 0).AppendLine()
            .AppendHeader("  classification").AppendFormat("{0,7}{1,11}", statistics.Classification.Minimum, statistics.Classification.Maximum).AppendLine();
        if (statistics.ScanAngleRank is { } scanAngleRank)
        {
            _ = builder.AppendHeader("  scan_angle_rank").AppendFormat("{0,6}{1,11}", scanAngleRank.Minimum, scanAngleRank.Maximum).AppendLine();
        }

        _ = builder
            .AppendHeader("  user_data").AppendFormat("{0,12}{1,11}", statistics.UserData.Minimum, statistics.UserData.Maximum).AppendLine()
            .AppendHeader("  point_source_ID").AppendFormat("{0,6}{1,11}", statistics.PointSourceId.Minimum, statistics.PointSourceId.Maximum).AppendLine();

        if (statistics.Gps is { } gps)
        {
            builder.AppendHeader("  gps_time").AppendFormat(" {0:0.000000} {1:0.000000}", gps.Minimum, gps.Maximum).AppendLine();
#if LAS1_2_OR_GREATER
            if (!reader.Header.GlobalEncoding.HasFlag(GlobalEncoding.StandardGpsTime) && (gps.Minimum < 0.0 || gps.Maximum > 604800.0))
            {
                builder.Append("WARNING: range violates GPS week time specified by global encoding bit 0", AnsiConsoleStyles.Warning).AppendLine();
            }
#endif
        }

#if LAS1_3_OR_GREATER
        if (statistics.WavePacketDescriptorIndex is not null)
        {
            builder
                .AppendHeader("  Wavepacket ")
                .AppendMinorHeader("Index    ")
                .AppendFormat("{0} {1}", statistics.WavePacketDescriptorIndex.Minimum, statistics.WavePacketDescriptorIndex.Maximum).AppendLine();
        }

        if (statistics.ByteOffsetToWaveformData is not null)
        {
            builder.AppendMinorHeader("             Offset   ").AppendFormat("{0} {1}", statistics.ByteOffsetToWaveformData.Minimum, statistics.ByteOffsetToWaveformData.Maximum).AppendLine();
        }

        if (statistics.WaveformPacketSizeInBytes is not null)
        {
            builder.AppendMinorHeader("             Size     ").AppendFormat("{0} {1}", statistics.WaveformPacketSizeInBytes.Minimum, statistics.WaveformPacketSizeInBytes.Maximum).AppendLine();
        }

        if (statistics.ReturnPointWaveformLocation is not null)
        {
            builder.AppendMinorHeader("             Location ").AppendFormat("{0} {1}", statistics.ReturnPointWaveformLocation.Minimum, statistics.ReturnPointWaveformLocation.Maximum).AppendLine();
        }

        if (statistics.ParametricDx is not null)
        {
            builder.AppendMinorHeader("             Xt       ").AppendFormat("{0} {1}", statistics.ParametricDx.Minimum, statistics.ParametricDx.Maximum).AppendLine();
        }

        if (statistics.ParametricDy is not null)
        {
            builder.AppendMinorHeader("             Yt       ").AppendFormat("{0} {1}", statistics.ParametricDy.Minimum, statistics.ParametricDy.Maximum).AppendLine();
        }

        if (statistics.ParametricDz is not null)
        {
            builder.AppendMinorHeader("             Zt       ").AppendFormat("{0} {1}", statistics.ParametricDz.Minimum, statistics.ParametricDz.Maximum).AppendLine();
        }
#endif

#if LAS1_4_OR_GREATER
        if (statistics.ScanAngle is not null)
        {
            builder.AppendHeader("  extended_classification").AppendFormat("{0,10}{1,7}", statistics.Classification.Minimum, statistics.Classification.Maximum).AppendLine();
            builder.AppendHeader("  extended_scan_angle").AppendFormat("{0,14}{1,7}", statistics.ScanAngle.Minimum, statistics.ScanAngle.Maximum).AppendLine();
        }

        if (reader.VariableLengthRecords.OfType<ExtraBytes>().FirstOrDefault() is { } extraBytesRecord)
        {
            var index = default(int);
            foreach (var extraByte in statistics.ExtraBytes)
            {
                builder.AppendHeader("  attribute{0}", index).AppendFormat("{0,11:0.###}{1,11:0.###}  ('{2}')", extraByte.Minimum, extraByte.Maximum, extraBytesRecord[index].Name).AppendLine();
                index++;
            }
        }
#endif

        return this;
    }

    /// <inheritdoc/>
    public ILasReaderFormatter AppendReturns(LasReader reader, Func<LasReader, Statistics> statisticsFunc)
    {
        var values = statisticsFunc(reader);
        builder.AppendHeader("number of first returns:        ").AppendFormat("{0}", values.FirstReturns).AppendLine();
        builder.AppendHeader("number of intermediate returns: ").AppendFormat("{0}", values.IntermediateReturns).AppendLine();
        builder.AppendHeader("number of last returns:         ").AppendFormat("{0}", values.LastReturns).AppendLine();
        builder.AppendHeader("number of single returns:       ").AppendFormat("{0}", values.SingleReturns).AppendLine();

        FormatOverviewReturnNumber(builder, values.OverviewReturnNumber[0], 0);
        if (reader.Header.Version.Minor < 4)
        {
            FormatOverviewReturnNumber(builder, values.OverviewReturnNumber[6], 6);
            FormatOverviewReturnNumber(builder, values.OverviewReturnNumber[7], 7);
        }

        if (reader.Header.Version.Minor > 3)
        {
            var overviewNumberOfReturns = values.OverviewNumberOfReturns.Skip(1).Take(15).ToArray();
            if (!Array.Exists(overviewNumberOfReturns, static v => v is not 0))
            {
                return this;
            }

            builder.AppendHeader("overview over extended number of returns of given pulse:");
            _ = overviewNumberOfReturns.Aggregate(builder, static (builder, v) => builder.AppendFormat(" {0}", v));
        }
        else
        {
            var overviewNumberOfReturns = values.OverviewNumberOfReturns.Skip(1).Take(7).ToArray();
            if (!Array.Exists(overviewNumberOfReturns, static v => v is not 0))
            {
                return this;
            }

            builder.AppendHeader("overview over number of returns of given pulse:");
            _ = overviewNumberOfReturns.Aggregate(builder, static (builder, v) => builder.AppendFormat(" {0}", v));
        }

        builder.AppendLine();

        return this;

        static void FormatOverviewReturnNumber(IFormatBuilder builder, long value, int returnNumber)
        {
            if (value is 0)
            {
                return;
            }

            builder
                .AppendFormat(AnsiConsoleStyles.Warning, "WARNING: there {0} {1} point{2} with return number {3}", value > 1 ? "are" : "is", value, value > 1 ? "s" : string.Empty, returnNumber)
                .AppendLine();
        }
    }

    /// <inheritdoc/>
    public ILasReaderFormatter AppendHistograms(LasReader reader, Func<LasReader, Statistics> statisticsFunc)
    {
        var values = statisticsFunc(reader);
        if (values.Histogram.Take(32).Any(static v => v is not 0))
        {
            builder
                .AppendHeader("histogram of classification of points:")
                .AppendLine();
            for (var i = 0; i < 32; i++)
            {
                var value = values.Histogram[i];
                if (value is 0)
                {
                    continue;
                }

                builder.Append(" ");
                builder.AppendCaption("{0,15}", value)
                    .Append("  ");
                builder
                    .AppendValue(GetClassificationName(i))
                    .Append(" (");
                builder
                    .AppendCount("{0}", i)
                    .AppendLine(")");

                static string GetClassificationName(int value)
                {
                    return value switch
                    {
                        0 => "never classified",
                        1 => "unclassified",
                        2 => "ground",
                        3 => "low vegetation",
                        4 => "medium vegetation",
                        5 => "high vegetation",
                        6 => "building",
                        7 => "noise",
                        8 => "keypoint",
                        9 => "water",
                        10 => "rail",
                        11 => "road surface",
                        12 => "overlap",
                        13 => "wire guard",
                        14 => "wire conductor",
                        15 => "tower",
                        16 => "wire connector",
                        17 => "bridge deck",
                        18 => "high noise",
                        19 => "overhead structure",
                        20 => "ignored ground",
                        21 => "snow",
                        22 => "temporal exclusion",
                        _ => "Reserved for ASPRS Definition",
                    };
                }
            }
        }

        if (reader.Header.PointDataFormatId >= 6 && values.Histogram.Skip(32).Any(static p => p is not 0))
        {
            builder
                .AppendHeader("histogram of extended classification of points:")
                .AppendLine();
            for (var i = 32; i < 256; i++)
            {
                var value = values.Histogram[i];
                if (value is 0)
                {
                    continue;
                }

                builder.Append(" ");
                builder.AppendCaption("{0,15}", value)
                    .Append("  ");
                builder
                    .AppendValue("extended classification")
                    .Append(" (");
                builder
                    .AppendCount("{0}", i)
                    .AppendLine(")");
            }
        }

        return this;
    }

    private static IEnumerable<(object? Header, object? Value)> GetInformation(LasReader reader)
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
        yield return ("offset to point data:", GetOffsetToPointData(reader));
        yield return ("number var. length records:", reader.VariableLengthRecords.Count);
        yield return ("point data format:", header.PointDataFormatId);
        yield return ("point data record length:", GetPointDataLength(reader));
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

        static IFormattable FormatArray<T>(IEnumerable<T> statistics)
            where T : IConvertible
        {
            return LazyFormattable.Create(statistics);
        }

        static IFormattable FormatDate(DateTime? dateTime)
        {
            var (dayOfYear, year) = dateTime.HasValue ? (dateTime.Value.DayOfYear, dateTime.Value.Year) : default;
            return LazyFormattable.Create((FormattableString)$"{dayOfYear}/{year}");
        }

        static IFormattable FormatPoint(Vector3D point, Vector3D? scaleFactor = default)
        {
            var x = point.X;
            var y = point.Y;
            var z = point.Z;

            if (scaleFactor is not { } vector)
            {
                return LazyFormattable.Create((FormattableString)$"{x} {y} {z}");
            }

            var format = string.Concat("{0", Format(vector.X), "} {1", Format(vector.Y), "} {2", Format(vector.Z), "}");
            return LazyFormattable.Create(format, x, y, z);
        }

        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "offsetToPointData")]
        static extern ref uint GetOffsetToPointData(LasReader reader);

        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "pointDataLength")]
        static extern ref ushort GetPointDataLength(LasReader reader);
    }

    private static IEnumerable<(object? Header, object? Value)> GetInformation(LasReader reader, VariableLengthRecord vlr)
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
            GeoKeyDirectoryTag geoKeyEntries => GetGeoKeyDirectoryTag(geoKeyEntries, reader.VariableLengthRecords.OfType<GeoDoubleParamsTag>().SingleOrDefault(), reader.VariableLengthRecords.OfType<GeoAsciiParamsTag>().SingleOrDefault()),
#if LAS1_3_OR_GREATER
            WaveformPacketDescriptor waveformPacketDescriptor => GetWaveformPacketDescriptor(waveformPacketDescriptor),
#endif
#if LAS1_4_OR_GREATER
            ExtraBytes extraBytes => GetExtraBytes(extraBytes),
            OgcCoordinateSystemWkt ogcCoordinateSystemWkt => GetOgcCoordinateSystemWkt(ogcCoordinateSystemWkt),
            OgcMathTransformWkt ogcMathTransformWkt => GetOgcMathTransformWkt(ogcMathTransformWkt),
            Cloud.CopcInfo copcInfo => GetCopcInfo(reader.Header, copcInfo),
#endif
            Tiling tiling => GetTiling(reader.Header.Min, reader.Header.Max, tiling),
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
                yield return (default, LazyFormattable.Create(formatProvider =>
                {
                    var stringBuilder = new System.Text.StringBuilder();
                    _ = stringBuilder
                        .Append("  ")
                        .Append(formatProvider, $"data type: {(uint)item.DataType} ({GetName(item.DataType)})")
                        .Append(formatProvider, $", name \"{item.Name}\"")
                        .Append(formatProvider, $", description: \"{item.Description}\"");

                    if (item.Options.HasFlag(ExtraBytesOptions.Min))
                    {
                        _ = stringBuilder.Append(formatProvider, $", min: {item.Min}");
                    }

                    if (item.Options.HasFlag(ExtraBytesOptions.Max))
                    {
                        _ = stringBuilder.Append(formatProvider, $", max: {item.Max}");
                    }

                    _ = stringBuilder.Append(", scale: ");
                    _ = item.HasScale
                        ? stringBuilder.Append(formatProvider, $"{item.Scale}")
                        : stringBuilder.Append(formatProvider, $"{1} (not set)");

                    _ = stringBuilder.Append(", offset: ");
                    _ = item.HasOffset
                        ? stringBuilder.Append(formatProvider, $"{item.Offset}")
                        : stringBuilder.Append(formatProvider, $"{0} (not set)");

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
                stringBuilder.Append(item).Append('|');
            }

            yield return (default, LazyFormattable.Create("  " + stringBuilder));
        }

        static IEnumerable<(object? Header, object? Value)> GetGeoDoubleParamsTag(GeoDoubleParamsTag record)
        {
            yield return (default, LazyFormattable.Create("GeoDoubleParamsTag  (number of doubles {0})", record.Count));
            yield return (default, LazyFormattable.Create(GetEnumerable()));

            IEnumerable<double> GetEnumerable()
            {
                var count = record.Count;
                for (var i = 0; i < count; i++)
                {
                    yield return record[i];
                }
            }
        }

        static IEnumerable<(object? Header, object? Value)> GetGeoKeyDirectoryTag(GeoKeyDirectoryTag record, GeoDoubleParamsTag? geoDoubleValue, GeoAsciiParamsTag? geoAsciiValue)
        {
            var version = record.Version;
            yield return (default, LazyFormattable.Create("GeoKeyDirectoryTag version {0} number of keys {1}", version, record.Count));

            foreach (var key in record)
            {
                yield return (default, LazyFormattable.Create("  key {0} tiff_tag_location {1} count {2} value_offset {3} - {4}: {5}", (ushort)key.KeyId, key.TiffTagLocation, key.Count, key.ValueOffset, key.KeyId, GetValueCore(key, geoDoubleValue, geoAsciiValue)));

                static string GetValueCore(GeoKeyEntry key, GeoDoubleParamsTag? geoDoubleValue, GeoAsciiParamsTag? geoAsciiValue)
                {
                    _ = GeoProjectionConverter.TryGetGeoTiffInfo(key, geoDoubleValue, geoAsciiValue, out _, out var value);
                    return value;
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
        static IEnumerable<(object? Header, object? Value)> GetWaveformPacketDescriptor(WaveformPacketDescriptor record)
        {
            yield return (string.Empty,
                string.Create(
                    System.Globalization.CultureInfo.InvariantCulture,
                    $"index {record.Header.RecordId - WaveformPacketDescriptor.MinTagRecordId + 1} bits/sample {record.BitsPerSample} compression {record.WaveformCompressionType} samples {record.NumberOfSamples} temporal {record.TemporalSampleSpacing} gain {record.DigitizerGain}, offset {record.DigitizerOffset}"));
        }
#endif

#if LAS1_4_OR_GREATER
        static IEnumerable<(object? Header, object? Value)> GetCopcInfo(HeaderBlock header, Cloud.CopcInfo record)
        {
            var centerFormat = string.Concat("center x y z: {0", Format(header.ScaleFactor.X), "} {1", Format(header.ScaleFactor.Y), "} {2", Format(header.ScaleFactor.Z), "}");
            yield return (default, string.Format(System.Globalization.CultureInfo.InvariantCulture, centerFormat, record.CentreX, record.CentreY, record.CentreZ));
            yield return (default, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"root node halfsize: {record.HalfSize:0.000}"));
            yield return (default, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"root node point spacing: {record.Spacing:0.000}"));
            yield return (default, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"gpstime min/max: {record.GpsTimeMinimum:0.00}/{record.GpsTimeMaximum:0.00}"));
            yield return (default, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"root hierarchy offset/size: {record.RootHierOffset}/{record.RootHierSize}"));
        }
#endif

        static IEnumerable<(object? Header, object? Value)> GetTiling(Vector3D min, Vector3D max, Tiling record)
        {
            var quadTree = new Indexing.LasQuadTree(record.MinX, record.MaxX, record.MinY, record.MaxY, (int)record.Level, (int)record.LevelIndex, default);
            var (minimumX, minimumY, maximumX, maximumY) = quadTree.GetBounds(0, (int)record.LevelIndex);
            var buffer = record.Buffer
                ? Math.Max(
                    Math.Max(
                        Math.Max(
                            (float)(minimumX - min.X),
                            (float)(minimumY - min.Y)),
                        (float)(max.X - maximumX)),
                    (float)(max.Y - maximumY))
                : default;

            yield return (string.Empty,
                string.Create(
                    System.Globalization.CultureInfo.InvariantCulture,
                    $"LAStiling (idx {record.LevelIndex}, lvl {record.Level}, sub {record.ImplicitLevels}, bbox {record.MinX} {record.MinY} {record.MaxX} {record.MaxY}{(record.Buffer ? ", buffer" : string.Empty)}{(record.Reversible ? ", reversible" : string.Empty)}) (size {maximumX - minimumX} x {maximumY - minimumY}, buffer {buffer})"));
        }
    }

#if LAS1_4_OR_GREATER
    private static IEnumerable<(object? Header, object? Value)> GetInformation(ExtendedVariableLengthRecord record)
    {
        yield return ("reserved", 0);
        yield return ("user ID", record.Header.UserId);
        yield return ("record ID", record.Header.RecordId);
        yield return ("length after header", record.Header.RecordLengthAfterHeader);
        yield return ("description", record.Header.Description);

        var extra = record switch
        {
            Cloud.CopcHierarchy copcHierarchy => GetCopcHierarchy(copcHierarchy),
            _ => [],
        };

        foreach (var item in extra)
        {
            yield return item;
        }

        static IEnumerable<(object? Header, object? Value)> GetCopcHierarchy(Cloud.CopcHierarchy record)
        {
            if (record.Root is not { } root)
            {
                yield return (default, "ERROR: invalid COPC file, EPT hierarchy not parsed.");
                yield break;
            }

            var maxOctreeLevel = root.Max(static e => e.Key.Level) + 1;
            yield return (default, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Octree with {maxOctreeLevel} levels"));

            var pointCount = new uint[maxOctreeLevel];
            var voxelCount = new uint[maxOctreeLevel];

            foreach (var entry in root)
            {
                var entryPointCount = (uint)entry.PointCount;
                pointCount[entry.Key.Level] += entryPointCount;
                voxelCount[entry.Key.Level]++;
            }

            for (var i = 0; i < maxOctreeLevel; i++)
            {
                if (pointCount[i] is not 0)
                {
                    yield return (default, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Level {i}: {pointCount[i]} points in {voxelCount[i]} voxels"));
                }
            }
        }
    }
#endif

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