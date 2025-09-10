// -----------------------------------------------------------------------
// <copyright file="Processor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.To.Las;

/// <summary>
/// The las2las processor.
/// </summary>
internal static class Processor
{
    /// <summary>
    /// Processes the input to output.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="input">The input URI.</param>
    /// <param name="output">The output file.</param>
    public static void Process(IServiceProvider? serviceProvider, Uri input, FileInfo output)
    {
        using var reader = LazReader.Create(File.OpenRead(input, serviceProvider));
        using var writer = new LasWriter(output.OpenWrite());
        Las(reader, writer);
    }

    /// <summary>
    /// Writes the input reader to the output writer.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="writer">The writer.</param>
    internal static void Las(LasReader reader, LasWriter writer)
    {
#if LAS1_4_OR_GREATER
        writer.Write(reader.Header, reader.VariableLengthRecords.Where(static vlr => !vlr.IsForCompression() && !vlr.IsForCloudOptimization()));
#else
        writer.Write(reader.Header, reader.VariableLengthRecords.Where(static vlr => !vlr.IsForCompression()));
#endif

        while (reader.ReadPointDataRecord() is { PointDataRecord: not null } pointDataRecord)
        {
            writer.Write(pointDataRecord.PointDataRecord, pointDataRecord.ExtraBytes);
        }

        writer.Flush();

#if LAS1_4_OR_GREATER
        foreach (var extendedVariableLengthRecord in reader
                     .ExtendedVariableLengthRecords
                     .Where(static extendedVariableLengthRecord => !extendedVariableLengthRecord.IsForCompression() && !extendedVariableLengthRecord.IsForCloudOptimization()))
        {
            writer.Write(extendedVariableLengthRecord);
        }
#endif
    }
}