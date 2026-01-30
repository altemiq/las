// -----------------------------------------------------------------------
// <copyright file="JsonLasReaderFormatter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

using Humanizer;

/// <summary>
/// the <c>JSON</c> <see cref="ILasReaderFormatter"/>.
/// </summary>
/// <param name="writer">The JSON writer.</param>
internal sealed class JsonLasReaderFormatter(System.Text.Json.Utf8JsonWriter writer) : ILasReaderFormatter
{
    /// <inheritdoc/>
    public ILasReaderFormatter AppendHeader(LasReader reader)
    {
        writer.WriteStartObject("las_header_entries");

        var header = reader.Header;
        writer.WriteString("file_signature", header.FileSignature);
        writer.WriteNumber("file_source_id", header.FileSourceId);
#if LAS1_2_OR_GREATER
        writer.WriteNumber("global_encoding", (ushort)header.GlobalEncoding);
#endif
        writer.WriteString("project_id_guid_data", header.ProjectId);
        writer.WriteString("version_major_minor", header.Version.ToString());
        writer.WriteString("system_identifier", header.SystemIdentifier);
        writer.WriteString("generating_software", header.GeneratingSoftware);
        if (header.FileCreation is { } fileCreation)
        {
            writer.WriteNumber("file_creation_day", fileCreation.DayOfYear);
            writer.WriteNumber("file_creation_year", fileCreation.Year);
        }

        writer.WriteNumber("offset_to_point_data", GetOffsetToPointData(reader));
        writer.WriteNumber("number_of_variable_length_records", reader.VariableLengthRecords.Count);
        writer.WriteNumber("point_data_format", header.PointDataFormatId);
        writer.WriteNumber("point_data_record_length", GetPointDataLength(reader));
#if LAS1_4_OR_GREATER
        writer.WriteNumber("number_of_point_records", header.LegacyNumberOfPointRecords);
        WriteArray(writer, "number_of_points_by_return", header.LegacyNumberOfPointsByReturn);
#else
        writer.WriteNumber("number_of_point_records", header.NumberOfPointRecords);
        WriteArray(writer, "number_of_points_by_return", header.NumberOfPointsByReturn);
#endif

        WritePoint(writer, "scale_factor", header.ScaleFactor);
        WritePoint(writer, "offset", header.Offset);
        WritePoint(writer, "min", header.Min, header.ScaleFactor);
        WritePoint(writer, "max", header.Max, header.ScaleFactor);

#if LAS1_4_OR_GREATER
        if (reader.Header is { Version: { Major: 1, Minor: >= 4 } })
        {
            writer.WriteNumber("start_of_first_extended_vlr", GetOffsetToEntendedVariableLengthRecords(reader));
            writer.WriteNumber("number_of_extended_vlrs", reader.ExtendedVariableLengthRecords.Count);
            writer.WriteNumber("extended_number_of_point_records", reader.Header.RawNumberOfPointRecords);
            WriteArray(writer, "extended_number_of_points_by_return", reader.Header.RawNumberOfPointsByReturn);
        }
#endif

        writer.WriteEndObject();

        return this;

        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "offsetToPointData")]
        static extern ref uint GetOffsetToPointData(LasReader reader);

        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "pointDataLength")]
        static extern ref ushort GetPointDataLength(LasReader reader);

#if LAS1_4_OR_GREATER
        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "offsetToEntendedVariableLengthRecords")]
        static extern ref long GetOffsetToEntendedVariableLengthRecords(LasReader reader);
#endif

        static void WritePoint(System.Text.Json.Utf8JsonWriter writer, string name, Vector3D point, Vector3D? scaleFactor = default)
        {
            writer.WriteStartObject(name);

            if (scaleFactor is { } vector)
            {
                writer.WriteNumber("x", Math.Round(point.X, Decimals(vector.X), MidpointRounding.ToEven));
                writer.WriteNumber("y", Math.Round(point.Y, Decimals(vector.Y), MidpointRounding.ToEven));
                writer.WriteNumber("z", Math.Round(point.Z, Decimals(vector.Z), MidpointRounding.ToEven));
            }
            else
            {
                writer.WriteNumber("x", point.X);
                writer.WriteNumber("y", point.Y);
                writer.WriteNumber("z", point.Z);
            }

            writer.WriteEndObject();
        }
    }

    /// <inheritdoc/>
    public ILasReaderFormatter AppendVariableLengthRecords(LasReader reader)
    {
        if (reader.VariableLengthRecords.Count is 0)
        {
            return this;
        }

        writer.WriteStartArray("las_variable_length_records");

        foreach (var (vlr, i) in reader.VariableLengthRecords.Select((vlr, i) => (vlr, i)))
        {
            writer.WriteStartObject();
            writer.WriteNumber("record_number", i + 1);
            writer.WriteNumber("total_records", reader.VariableLengthRecords.Count);
            writer.WriteNumber("reserved", 0);
            writer.WriteString("user_id", vlr.Header.UserId);
            writer.WriteNumber("record_id", vlr.Header.RecordId);
            writer.WriteNumber("record_length_after_header", vlr.Header.RecordLengthAfterHeader);
            writer.WriteString("description", vlr.Header.Description);

            switch (vlr)
            {
                case GeoKeyDirectoryTag geoKeyEntries:
                    writer.WriteStartObject("geo_key_directory_tag");
                    writer.WriteString("geo_key_version", string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}.{2}", 1, geoKeyEntries.Version.Major, geoKeyEntries.Version.Minor));
                    writer.WriteNumber("number_of_keys", geoKeyEntries.Count);

                    writer.WriteStartArray("geo_keys");

                    var geoDoubleValue = reader.VariableLengthRecords.OfType<GeoDoubleParamsTag>().SingleOrDefault();
                    var geoAsciiValue = reader.VariableLengthRecords.OfType<GeoAsciiParamsTag>().SingleOrDefault();
                    foreach (var entry in geoKeyEntries)
                    {
                        writer.WriteStartObject();

                        writer.WriteNumber("key", (int)entry.KeyId);
                        writer.WriteNumber("tiff_tag_location", entry.TiffTagLocation);
                        writer.WriteNumber("count", entry.Count);
                        writer.WriteNumber("value_offset", entry.ValueOffset);
                        _ = GeoProjectionConverter.TryGetGeoTiffInfo(entry, geoDoubleValue, geoAsciiValue, out var key, out var value);
                        writer.WriteString(key.Underscore(), value);

                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();

                    writer.WriteEndObject();
                    break;
                case GeoDoubleParamsTag doubleParamsTag:
                    writer.WriteStartObject("geo_double_params_tag");
                    writer.WriteNumber("number_of_doubles", doubleParamsTag.Count);
                    WriteArray(writer, "geo_params", doubleParamsTag);
                    writer.WriteEndObject();
                    break;
                case GeoAsciiParamsTag asciiParamsTag:
                    writer.WriteStartObject("geo_ascii_params_tag");
                    writer.WriteNumber("number_of_characters", asciiParamsTag.Header.RecordLengthAfterHeader);
                    writer.WriteStartArray("geo_params");
                    foreach (var value in asciiParamsTag)
                    {
                        writer.WriteStringValue(value);
                    }

                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    break;
                case ClassificationLookup classificationLookup:
                    writer.WriteStartArray("classification");
                    foreach (var item in classificationLookup)
                    {
                        writer.WriteStartObject();
                        writer.WriteNumber("class_number", item.ClassNumber);
                        writer.WriteString("class_description", item.Description);
                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();
                    break;
                case TextAreaDescription textAreaDescription:
                    writer.WriteString("text_area_description", textAreaDescription.Value);
                    break;
#if LAS1_4_OR_GREATER
                case OgcMathTransformWkt mathTransformWkt:
                    WriteWkt(writer, "wkt_ogc_math_transform", mathTransformWkt.Wkt.ToString());
                    break;
                case OgcCoordinateSystemWkt coordinateSystemWkt:
                    WriteWkt(writer, "wkt_ogc_coordinate_system", coordinateSystemWkt.Wkt.ToString());
                    break;
                case ExtraBytes extraBytes:
                    writer.WriteStartArray("extra_byte_descriptions");

                    foreach (var item in extraBytes)
                    {
                        writer.WriteStartObject();

                        writer.WriteNumber("data_type", (ushort)item.DataType);
                        writer.WriteString("type", GetName(item.DataType));
                        writer.WriteString("name", item.Name);
                        writer.WriteString("description", item.Description);

                        WriteValue(writer, item, ExtraBytesOptions.Min, "min", static item => item.Min);
                        WriteValue(writer, item, ExtraBytesOptions.Min, "max", static item => item.Max);
                        WriteValue(writer, item, ExtraBytesOptions.Min, "scale", static item => item.Scale);
                        WriteValue(writer, item, ExtraBytesOptions.Min, "offset", static item => item.Offset);

                        writer.WriteEndObject();

                        static void WriteValue(System.Text.Json.Utf8JsonWriter writer, ExtraBytesItem item, ExtraBytesOptions option, string name, Func<ExtraBytesItem, object> value)
                        {
                            if (!item.Options.HasFlag(option))
                            {
                                writer.WriteNull(name);
                            }

                            writer.WriteStartObject(name);

                            switch (value(item))
                            {
                                case ulong v:
                                    writer.WriteNumberValue(v);
                                    break;
                                case long v:
                                    writer.WriteNumberValue(v);
                                    break;
                                case double v:
                                    writer.WriteNumberValue(v);
                                    break;
                            }

                            writer.WriteEndObject();
                        }

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
                    }

                    writer.WriteEndArray();

                    break;
#endif
                case CompressedTag compressedTag:
                    writer.WriteStartObject("laszip_compression");

                    writer.WriteString("version", string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{compressedTag.Version.Major}.{compressedTag.Version.Minor}r{compressedTag.Version.Revision} c{(int)compressedTag.Compressor}"));
                    writer.WriteNumber("chunk_size", (uint)compressedTag.ChunkSize);

                    writer.WriteStartArray("data_structures");
                    foreach (var item in compressedTag)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("name", item.Name);
                        writer.WriteNumber("version", item.Version);
                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();

                    writer.WriteEndObject();

                    break;
                case Tiling tiling:
                    writer.WriteStartObject("tiling");
                    writer.WriteNumber("index", tiling.LevelIndex);
                    writer.WriteNumber("level", tiling.Level);
                    writer.WriteNumber("implcit_levels", tiling.ImplicitLevels);

                    writer.WriteStartObject("bbox");
                    writer.WriteNumber("min_x", tiling.MinX);
                    writer.WriteNumber("min_y", tiling.MinY);
                    writer.WriteNumber("max_x", tiling.MaxX);
                    writer.WriteNumber("max_y", tiling.MaxY);
                    writer.WriteEndObject();

                    writer.WriteBoolean("buffer", tiling.Buffer);
                    writer.WriteBoolean("reversible", tiling.Reversible);

                    var quadTree = new Indexing.LasQuadTree(tiling.MinX, tiling.MaxX, tiling.MinY, tiling.MaxY, (int)tiling.Level, (int)tiling.LevelIndex, default);
                    var (minimum, maximum) = quadTree.GetBounds(0, (int)tiling.LevelIndex);

                    writer.WriteStartObject("size");
                    writer.WriteNumber("width", maximum.X - minimum.X);
                    writer.WriteNumber("height", maximum.Y - minimum.Y);
                    writer.WriteEndObject();

                    var min = reader.Header.Min;
                    var max = reader.Header.Min;
                    var tilingBuffer = tiling.Buffer
                        ? Math.Max(
                            Math.Max(
                                Math.Max(
                                    (float)(minimum.X - min.X),
                                    (float)(minimum.Y - min.Y)),
                                (float)(max.X - maximum.X)),
                            (float)(max.Y - maximum.Y))
                        : default;

                    writer.WriteNumber("buffer_size", tilingBuffer);
                    writer.WriteEndObject();
                    break;
#if LAS1_4_OR_GREATER
                case Cloud.CopcInfo copcInfo:
                    writer.WriteStartObject("copc");

                    writer.WriteStartObject("center");
                    writer.WriteNumber("x", copcInfo.CentreX);
                    writer.WriteNumber("y", copcInfo.CentreY);
                    writer.WriteNumber("z", copcInfo.CentreZ);
                    writer.WriteEndObject();

                    writer.WriteNumber("root_node_halfsize", copcInfo.HalfSize);
                    writer.WriteNumber("root_node_point_spacing", copcInfo.Spacing);

                    writer.WriteStartObject("gpstime");
                    writer.WriteNumber("min", copcInfo.GpsTimeMinimum);
                    writer.WriteNumber("max", copcInfo.GpsTimeMaximum);
                    writer.WriteEndObject();

                    writer.WriteStartObject("root_hierarchy");
                    writer.WriteNumber("offset", copcInfo.RootHierOffset);
                    writer.WriteNumber("size", copcInfo.RootHierSize);
                    writer.WriteEndObject();

                    writer.WriteEndObject();
                    break;
#endif
            }

            writer.WriteEndObject();

#if LAS1_4_OR_GREATER
            static void WriteWkt(System.Text.Json.Utf8JsonWriter writer, string name, string wkt)
            {
                writer.WritePropertyName(name);

                // ensure we escape the WKT
                writer.WriteRawValue($"\"{wkt.Replace("\"", "\\\"", StringComparison.Ordinal)}\"");
            }
#endif
        }

        writer.WriteEndArray();
        return this;
    }

#if LAS1_4_OR_GREATER
    /// <inheritdoc/>
    public ILasReaderFormatter AppendExtendedVariableLengthRecords(LasReader reader)
    {
        if (reader.ExtendedVariableLengthRecords.Count is 0)
        {
            return this;
        }

        writer.WriteStartArray("las_extended_variable_length_records");

        foreach (var (evlr, i) in reader.ExtendedVariableLengthRecords.Select((vlr, i) => (vlr, i)))
        {
            writer.WriteStartObject();
            writer.WriteNumber("record_number", i + 1);
            writer.WriteNumber("total_records", reader.ExtendedVariableLengthRecords.Count);
            writer.WriteNumber("reserved", 0);
            writer.WriteString("user_id", evlr.Header.UserId);
            writer.WriteNumber("record_id", evlr.Header.RecordId);
            writer.WriteNumber("record_length_after_header", evlr.Header.RecordLengthAfterHeader);
            writer.WriteString("description", evlr.Header.Description);

            switch (evlr)
            {
                case Cloud.CopcHierarchy { Root: { } root }:
                    writer.WriteStartObject("copc");
                    var maxOctreeLevel = root.Max(entry => entry.Key.Level) + 1;
                    writer.WriteNumber("octree_level_number", maxOctreeLevel);

                    var pointCount = new uint[maxOctreeLevel];
                    var voxelCount = new uint[maxOctreeLevel];

                    foreach (var entry in root)
                    {
                        var entryPointCount = (uint)entry.PointCount;
                        pointCount[entry.Key.Level] += entryPointCount;
                        voxelCount[entry.Key.Level]++;
                    }

                    writer.WriteStartArray("octree_levels");
                    for (var j = 0; j < maxOctreeLevel; j++)
                    {
                        if (pointCount[j] is 0)
                        {
                            continue;
                        }

                        writer.WriteStartObject();
                        writer.WriteNumber("level", j);
                        writer.WriteNumber("point_count", pointCount[j]);
                        writer.WriteNumber("voxels", voxelCount[j]);
                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();

                    writer.WriteEndObject();
                    break;
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();

        return this;
    }
#endif

    /// <inheritdoc/>
    public ILasReaderFormatter AppendStatistics(LasReader reader, Func<LasReader, Statistics> statisticsFunc)
    {
        writer.WriteStartObject("min_max_las_point_report");

        var statistics = statisticsFunc(reader);
        var values = statistics.IntValues;
        WriteVectorMinMax(writer, "x", values, 0);
        WriteVectorMinMax(writer, "y", values, 1);
        WriteVectorMinMax(writer, "z", values, 2);
        WriteVectorMinMax(writer, "intensity", values, 3);
        WriteVectorMinMax(writer, "return_number", values, 4);
        WriteVectorMinMax(writer, "number_of_returns", values, 5);
        WriteBoolean(writer, "edge_of_flight_line", statistics.EdgeOfFlightLine);
        WriteBoolean(writer, "scan_direction_flag", statistics.ScanDirectionFlag);
        WriteMinMax(writer, "classification", statistics.Classification);
        WriteMinMax(writer, "scan_angle", statistics.ScanAngleRank);
        WriteMinMax(writer, "user_data", statistics.UserData);
        WriteMinMax(writer, "point_source_id", statistics.PointSourceId);
        WriteMinMax(writer, "gps_time", statistics.Gps);

#if LAS1_4_OR_GREATER
        if (statistics.ScanAngle is not null)
        {
            WriteVectorMinMax(writer, "extended_return_number", values, 4);
            WriteVectorMinMax(writer, "extended_number_of_returns", values, 5);
            WriteMinMax(writer, "extended_classification", statistics.Classification);
            WriteMinMax(writer, "extended_scan_angle", statistics.ScanAngle);
        }
#endif

        writer.WriteEndObject();
        return this;

        static void WriteVectorMinMax<T>(System.Text.Json.Utf8JsonWriter writer, string name, IMinMax<System.Runtime.Intrinsics.Vector256<T>>? minMax, int index, Func<T, T>? transformer = default)
            where T : System.Numerics.INumber<T>
        {
            if (minMax is null)
            {
                return;
            }

            var min = transformer is null ? minMax.Minimum[index] : transformer(minMax.Minimum[index]);
            var max = transformer is null ? minMax.Maximum[index] : transformer(minMax.Maximum[index]);

            WriteMinMaxValues(writer, name, min, max);
        }

        static void WriteMinMax<T>(System.Text.Json.Utf8JsonWriter writer, string name, IMinMax<T>? minMax, Func<T, T>? transformer = default)
            where T : System.Numerics.INumber<T>
        {
            if (minMax is null)
            {
                return;
            }

            var min = transformer is null ? minMax.Minimum : transformer(minMax.Minimum);
            var max = transformer is null ? minMax.Maximum : transformer(minMax.Maximum);

            WriteMinMaxValues(writer, name, min, max);
        }

        static void WriteBoolean(System.Text.Json.Utf8JsonWriter writer, string name, bool value)
        {
            WriteMinMaxValues(writer, name, 0, value ? 1 : 0);
        }

        static void WriteMinMaxValues<T>(System.Text.Json.Utf8JsonWriter writer, string name, T min, T max)
            where T : System.Numerics.INumber<T>
        {
            writer.WriteStartObject(name);
            WriteNumber(writer, "min", min);
            WriteNumber(writer, "max", max);
            writer.WriteEndObject();
        }
    }

    /// <inheritdoc/>
    public ILasReaderFormatter AppendReturns(LasReader reader, Func<LasReader, Statistics> statisticsFunc)
    {
        var statistics = statisticsFunc(reader);

        writer.WriteNumber("number_of_first_returns", statistics.FirstReturns);
        writer.WriteNumber("number_of_intermediate_returns", statistics.IntermediateReturns);
        writer.WriteNumber("number_of_last_returns", statistics.LastReturns);
        writer.WriteNumber("number_of_single_returns", statistics.SingleReturns);

        // check the header
        var warnings = new List<string>();
#if LAS1_4_OR_GREATER
        if (reader.Header is { PointDataFormatId: >= 6, LegacyNumberOfPointRecords: not 0U })
        {
            warnings.Add($"WARNING: point type is {reader.Header.PointDataFormatId} but (legacy) number of point records in header is {reader.Header.LegacyNumberOfPointRecords} instead zero.");
        }

        if (warnings.Count > 0)
        {
            writer.WriteStartObject("number_of_point_records");
            WriteWarnings(writer, warnings);
            writer.WriteEndObject();
            warnings.Clear();
        }
#endif

#if LAS1_4_OR_GREATER
        if (reader.Header is { PointDataFormatId: >= 6 })
        {
            foreach (var (i, numberOfPointsByReturn) in reader.Header.LegacyNumberOfPointsByReturn.Index().Where(i => i.Item is not 0U))
            {
                warnings.Add(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"WARNING: point type is {reader.Header.PointDataFormatId} but (legacy) number of points by return [{i}] in header is {numberOfPointsByReturn} instead zero."));
            }
        }
#endif

        FormatOverviewReturnNumber(warnings, statistics.OverviewReturnNumber[0], 0);
        if (reader.Header.Version.Minor < 4)
        {
            FormatOverviewReturnNumber(warnings, statistics.OverviewReturnNumber[6], 6);
            FormatOverviewReturnNumber(warnings, statistics.OverviewReturnNumber[7], 7);
        }

        writer.WriteStartObject("points_by_return");
        WriteWarnings(writer, warnings);
#if LAS1_4_OR_GREATER
        if (reader.Header.Version.Minor >= 4)
        {
            WriteArray(writer, "extended_number_of_returns_of_given_pulse", statistics.OverviewNumberOfReturns.Skip(1).Take(15));
        }
        else
        {
            WriteArray(writer, "number_of_returns_of_given_pulse", statistics.OverviewNumberOfReturns.Skip(1).Take(7));
        }
#else
        WriteArray(writer, "number_of_returns_of_given_pulse", statistics.OverviewNumberOfReturns.Skip(1).Take(7));
#endif

        writer.WriteEndObject();

        return this;

        static void WriteWarnings(System.Text.Json.Utf8JsonWriter writer, ICollection<string> warnings)
        {
            if (warnings.Count is 0)
            {
                return;
            }

            writer.WriteStartArray("warnings");
            foreach (var warning in warnings)
            {
                writer.WriteStringValue(warning);
            }

            writer.WriteEndArray();
        }

        static void FormatOverviewReturnNumber(IList<string> warnings, long value, int returnNumber)
        {
            if (value is 0)
            {
                return;
            }

            warnings.Add(value is 1
                ? string.Create(System.Globalization.CultureInfo.InvariantCulture, $"WARNING: there is {value} point with return number {returnNumber}")
                : string.Create(System.Globalization.CultureInfo.InvariantCulture, $"WARNING: there are {value} points with return number {returnNumber}"));
        }
    }

    /// <inheritdoc/>
    public ILasReaderFormatter AppendHistograms(LasReader reader, Func<LasReader, Statistics> statisticsFunc)
    {
        var statistics = statisticsFunc(reader);
        if (statistics.Histogram.Take(32).Any(static v => v is not 0))
        {
            writer.WriteStartObject("histogram_classification_of_points");

            writer.WriteStartArray("classification");
            for (var i = 0; i < 32; i++)
            {
                var value = statistics.Histogram[i];
                if (value is 0)
                {
                    continue;
                }

                writer.WriteStartObject();
                writer.WriteNumber("id", value);
                writer.WriteString("type", GetClassificationName(i));
                writer.WriteNumber("index", i);
                writer.WriteEndObject();
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

            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        if (reader.Header.PointDataFormatId >= 6 && statistics.Histogram.Skip(32).Any(static v => v is not 0))
        {
            writer.WriteStartObject("extended_histogram_classification_of_points");

            writer.WriteStartArray("extended_classification");
            for (var i = 32; i < 256; i++)
            {
                var value = statistics.Histogram[i];
                if (value is 0)
                {
                    continue;
                }

                writer.WriteStartObject();
                writer.WriteNumber("id", value);
                writer.WriteString("type", "extended classification");
                writer.WriteNumber("index", i);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        return this;
    }

    private static void WriteArray<T>(System.Text.Json.Utf8JsonWriter writer, string name, IEnumerable<T> values)
        where T : System.Numerics.INumber<T>
    {
        writer.WriteStartArray(name);
        foreach (var value in values)
        {
            WriteNumber(writer, value);
        }

        writer.WriteEndArray();
    }

    private static void WriteNumber<T>(System.Text.Json.Utf8JsonWriter writer, string name, T value)
        where T : System.Numerics.INumber<T>
    {
        writer.WritePropertyName(name);
        WriteNumber(writer, value);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1871:Two branches in a conditional structure should not have exactly the same implementation", Justification = "Checked")]
    private static void WriteNumber<T>(System.Text.Json.Utf8JsonWriter writer, T value)
        where T : System.Numerics.INumber<T>
    {
        switch (value)
        {
            case byte number:
                writer.WriteNumberValue(number);
                break;
            case sbyte number:
                writer.WriteNumberValue(number);
                break;
            case short number:
                writer.WriteNumberValue(number);
                break;
            case ushort number:
                writer.WriteNumberValue(number);
                break;
            case int number:
                writer.WriteNumberValue(number);
                break;
            case uint number:
                writer.WriteNumberValue(number);
                break;
            case long number:
                writer.WriteNumberValue(number);
                break;
            case ulong number:
                writer.WriteNumberValue(number);
                break;
            case double number:
                writer.WriteNumberValue(number);
                break;
            case float number:
                writer.WriteNumberValue(number);
                break;
        }
    }

    private static int Decimals(double precision) => precision switch
    {
        0.1 or 0.5 => 1,
        0.01 or 0.25 => 2,
        0.001 or 0.002 or 0.005 or 0.025 or 0.125 => 3,
        0.0001 or 0.0002 or 0.0005 or 0.0025 => 4,
        0.00001 or 0.00002 or 0.00005 or 0.00025 => 5,
        0.000001 => 6,
        0.0000001 => 7,
        0.00000001 => 8,
        _ => int.MaxValue,
    };
}