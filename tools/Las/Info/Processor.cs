// -----------------------------------------------------------------------
// <copyright file="Processor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The information processor.
/// </summary>
internal static class Processor
{
    /// <summary>
    /// Processes the specified file.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="console">The console.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <param name="noMinMax">No min/max calculate.</param>
    /// <param name="noReturns">No returns.</param>
    /// <param name="boundingBox">The bounding box.</param>
    public static void Process(Stream stream, IAnsiConsole console, IFormatProvider formatProvider, bool noMinMax, bool noReturns, BoundingBox? boundingBox)
    {
        using var reader = LazReader.Create(stream);
        reader.Format(
            formatProvider,
            (style, provider, format, args) => console.Write(string.Format(provider, format, args), style),
            console.Write,
            (value, style) =>
            {
                if (value is null)
                {
                    console.WriteLine();
                }
                else
                {
                    console.WriteLine(value, style);
                }
            });

        if (!noMinMax)
        {
            console.WriteMajorHeader("reporting minimum and maximum for all LAS point record entries ...");
            console.WriteLine();
        }

        var values = reader.GetStatistics(boundingBox);

        if (!noMinMax)
        {
            console.WriteHeader("  X").WriteLine(string.Format(formatProvider, "{0,20}{1,11}", values.X.Minimum, values.X.Maximum));
            console.WriteHeader("  Y").WriteLine(string.Format(formatProvider, "{0,20}{1,11}", values.Y.Minimum, values.Y.Maximum));
            console.WriteHeader("  Z").WriteLine(string.Format(formatProvider, "{0,20}{1,11}", values.Z.Minimum, values.Z.Maximum));
            console.WriteHeader("  intensity").WriteLine(string.Format(formatProvider, "{0,12}{1,11}", values.Intensity.Minimum, values.Intensity.Maximum));
            console.WriteHeader("  return_number").WriteLine(string.Format(formatProvider, "{0,8}{1,11}", values.ReturnNumber.Minimum, values.ReturnNumber.Maximum));
            console.WriteHeader("  number_of_returns").WriteLine(string.Format(formatProvider, "{0,4}{1,11}", values.NumberOfReturns.Minimum, values.NumberOfReturns.Maximum));
            console.WriteHeader("  edge_of_flight_line").WriteLine(string.Format(formatProvider, "{0,2}{1,11}", 0, values.EdgeOfFlightLine ? 1 : 0));
            console.WriteHeader("  scan_direction_flag").WriteLine(string.Format(formatProvider, "{0,2}{1,11}", 0, values.ScanDirectionFlag ? 1 : 0));
            console.WriteHeader("  classification").WriteLine(string.Format(formatProvider, "{0,7}{1,11}", values.Classification.Minimum, values.Classification.Maximum));
            if (values.ScanAngleRank is { } scanAngleRank)
            {
                console.WriteHeader("  scan_angle_rank").WriteLine(string.Format(formatProvider, "{0,6}{1,11}", scanAngleRank.Minimum, scanAngleRank.Maximum));
            }

            console.WriteHeader("  user_data").WriteLine(string.Format(formatProvider, "{0,12}{1,11}", values.UserData.Minimum, values.UserData.Maximum));
            console.WriteHeader("  point_source_ID").WriteLine(string.Format(formatProvider, "{0,6}{1,11}", values.PointSourceId.Minimum, values.PointSourceId.Maximum));

            if (values.Gps is { } gps)
            {
                console.WriteHeader("  gps_time").WriteLine(string.Format(formatProvider, " {0:0.000000} {1:0.000000}", gps.Minimum, gps.Maximum));
#if LAS1_2_OR_GREATER
                if (!reader.Header.GlobalEncoding.HasFlag(GlobalEncoding.StandardGpsTime) && (gps.Minimum < 0.0 || gps.Maximum > 604800.0))
                {
                    console.WriteLine("WARNING: range violates GPS week time specified by global encoding bit 0", AnsiConsoleStyles.Warning);
                }
#endif
            }

#if LAS1_3_OR_GREATER

            if (values.WavePacketDescriptorIndex is not null)
            {
                console
                    .WriteHeader("  Wavepacket ")
                    .WriteMinorHeader("Index    ")
                    .WriteLine(string.Format(formatProvider, "{0} {1}", values.WavePacketDescriptorIndex.Minimum, values.WavePacketDescriptorIndex.Maximum));
            }

            if (values.ByteOffsetToWaveformData is not null)
            {
                console.WriteMinorHeader("             Offset   ").WriteLine(string.Format(formatProvider, "{0} {1}", values.ByteOffsetToWaveformData.Minimum, values.ByteOffsetToWaveformData.Maximum));
            }

            if (values.WaveformPacketSizeInBytes is not null)
            {
                console.WriteMinorHeader("             Size     ").WriteLine(string.Format(formatProvider, "{0} {1}", values.WaveformPacketSizeInBytes.Minimum, values.WaveformPacketSizeInBytes.Maximum));
            }

            if (values.ReturnPointWaveformLocation is not null)
            {
                console.WriteMinorHeader("             Location ").WriteLine(string.Format(formatProvider, "{0} {1}", values.ReturnPointWaveformLocation.Minimum, values.ReturnPointWaveformLocation.Maximum));
            }

            if (values.ParametricDx is not null)
            {
                console.WriteMinorHeader("             Xt       ").WriteLine(string.Format(formatProvider, "{0} {1}", values.ParametricDx.Minimum, values.ParametricDx.Maximum));
            }

            if (values.ParametricDy is not null)
            {
                console.WriteMinorHeader("             Yt       ").WriteLine(string.Format(formatProvider, "{0} {1}", values.ParametricDy.Minimum, values.ParametricDy.Maximum));
            }

            if (values.ParametricDz is not null)
            {
                console.WriteMinorHeader("             Zt       ").WriteLine(string.Format(formatProvider, "{0} {1}", values.ParametricDz.Minimum, values.ParametricDz.Maximum));
            }
#endif

#if LAS1_4_OR_GREATER
            if (values.ScanAngle is not null)
            {
                console.WriteHeader("  extended_classification").WriteLine(string.Format(formatProvider, "{0,10}{1,7}", values.Classification.Minimum, values.Classification.Maximum));
                console.WriteHeader("  extended_scan_angle").WriteLine(string.Format(formatProvider, "{0,14}{1,7}", values.ScanAngle.Minimum, values.ScanAngle.Maximum));
            }

            if (reader.VariableLengthRecords.OfType<ExtraBytes>().FirstOrDefault() is { } extraBytesRecord)
            {
                var index = default(int);
                foreach (var extraByte in values.ExtraBytes)
                {
                    console.WriteHeader(formatProvider, "  attribute{0}", index).WriteLine(string.Format(formatProvider, "{0,11:0.###}{1,11:0.###}  ('{2}')", extraByte.Minimum, extraByte.Maximum, extraBytesRecord[index].Name));
                    index++;
                }
            }
#endif
        }

        if (!noReturns)
        {
            console.WriteHeader("number of first returns:        ").WriteLine(string.Format(formatProvider, "{0}", values.FirstReturns));
            console.WriteHeader("number of intermediate returns: ").WriteLine(string.Format(formatProvider, "{0}", values.IntermediateReturns));
            console.WriteHeader("number of last returns:         ").WriteLine(string.Format(formatProvider, "{0}", values.LastReturns));
            console.WriteHeader("number of single returns:       ").WriteLine(string.Format(formatProvider, "{0}", values.SingleReturns));

            FormatOverviewReturnNumber(console, formatProvider, values.OverviewReturnNumber[0], 0);
            if (reader.Header.Version.Minor < 4)
            {
                FormatOverviewReturnNumber(console, formatProvider, values.OverviewReturnNumber[6], 6);
                FormatOverviewReturnNumber(console, formatProvider, values.OverviewReturnNumber[7], 7);
            }

            if (reader.Header.Version.Minor > 3)
            {
                var overviewNumberOfReturns = values.OverviewNumberOfReturns.Skip(1).Take(15).ToArray();
                if (Array.Exists(overviewNumberOfReturns, static v => v is not 0))
                {
                    console.WriteHeader("overview over extended number of returns of given pulse:");

                    foreach (var value in overviewNumberOfReturns)
                    {
                        console.Write(" " + value.ToString(formatProvider));
                    }

                    console.WriteLine();
                }
            }
            else
            {
                var overviewNumberOfReturns = values.OverviewNumberOfReturns.Skip(1).Take(7).ToArray();
                if (Array.Exists(overviewNumberOfReturns, static v => v is not 0))
                {
                    console.WriteHeader("overview over number of returns of given pulse:");

                    foreach (var value in overviewNumberOfReturns)
                    {
                        console.Write(" " + value.ToString(formatProvider));
                    }

                    console.WriteLine();
                }
            }

            static void FormatOverviewReturnNumber(IAnsiConsole console, IFormatProvider formatProvider, long value, int returnNumber)
            {
                if (value is 0)
                {
                    return;
                }

                console.WriteLine(string.Format(formatProvider, "WARNING: there {0} {1} point{2} with return number {3}", value > 1 ? "are" : "is", value, value > 1 ? "s" : string.Empty, returnNumber), AnsiConsoleStyles.Warning);
            }
        }

        if (values.Histogram.Take(32).Any(static v => v is not 0))
        {
            console
                .WriteHeader("histogram of classification of points:")
                .WriteLine();
            for (var i = 0; i < 32; i++)
            {
                var value = values.Histogram[i];
                if (value is 0)
                {
                    continue;
                }

                console.Write(" ");
                console.WriteCaption(value.ToString(formatProvider).PadLeft(15))
                    .Write("  ");
                console
                    .WriteValue(GetClassificationName(i))
                    .Write(" (");
                console
                    .WriteCount(i.ToString(formatProvider))
                    .WriteLine(")");

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
            console.WriteHeader("histogram of extended classification of points:");
            console.WriteLine();
            for (var i = 32; i < 256; i++)
            {
                var value = values.Histogram[i];
                if (value is 0)
                {
                    continue;
                }

                console.Write(" ");
                console.WriteCaption(value.ToString(formatProvider).PadLeft(15))
                    .Write("  ");
                console
                    .WriteValue("extended classification")
                    .Write(" (");
                console
                    .WriteCount(i.ToString(formatProvider))
                    .WriteLine(")");
            }
        }
    }
}