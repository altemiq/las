// -----------------------------------------------------------------------
// <copyright file="LasToArrowStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="ILasReader"/> to <see cref="Apache.Arrow"/> Stream.
/// </summary>
internal static class LasToArrowStream
{
    /// <summary>
    /// Converts the <see cref="ILasReader"/> into <see cref="RecordBatch"/> instances.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="batchSize">The batch size.</param>
    /// <returns>The <see cref="RecordBatch"/> instances.</returns>
    internal static IEnumerable<RecordBatch> ToArrowBatches(ILasReader reader, Schema schema, int batchSize = 50_000)
    {
        IReadOnlyList<ColumnBuffer> buffers = [.. schema.FieldsList.Select(f => ColumnBuffer.Create(f.DataType.TypeId, capacity: batchSize))];

        var rowCount = 0;
        var hasYielded = false;

        while (reader.ReadPointDataRecord() is { PointDataRecord: { } basePointDataRecord })
        {
            AddPointDataRecordToBuffers(basePointDataRecord, buffers);

            rowCount++;

            if (rowCount < batchSize)
            {
                continue;
            }

            yield return FlushToRecordBatch(buffers, schema, rowCount);

            rowCount = 0;
            hasYielded = true;
        }

        if (rowCount > 0)
        {
            yield return FlushToRecordBatch(buffers, schema, rowCount);
            hasYielded = true;
        }

        if (!hasYielded)
        {
            yield return FlushToRecordBatch(buffers, schema, 0);
        }
    }

    /// <summary>
    /// Adds the <see cref="IBasePointDataRecord"/> to the buffers.
    /// </summary>
    /// <param name="basePointDataRecord">The point data record.</param>
    /// <param name="buffers">The buffers.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "This _could_ be used.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "RedundantAssignment", Justification = "This _could_ be used.")]
    internal static void AddPointDataRecordToBuffers(IBasePointDataRecord basePointDataRecord, IReadOnlyList<ColumnBuffer> buffers)
    {
        // do the base values
        var index = 0;
        buffers[index++].Add(basePointDataRecord.X);
        buffers[index++].Add(basePointDataRecord.Y);
        buffers[index++].Add(basePointDataRecord.Z);
        buffers[index++].Add(basePointDataRecord.Intensity);
        buffers[index++].Add(basePointDataRecord.ReturnNumber);
        buffers[index++].Add(basePointDataRecord.NumberOfReturns);
        buffers[index++].Add(basePointDataRecord.ScanDirectionFlag);
        buffers[index++].Add(basePointDataRecord.EdgeOfFlightLine);
        buffers[index++].Add(basePointDataRecord.Synthetic);
        buffers[index++].Add(basePointDataRecord.KeyPoint);
        buffers[index++].Add(basePointDataRecord.Withheld);
        buffers[index++].Add(basePointDataRecord.UserData);
        buffers[index++].Add(basePointDataRecord.PointSourceId);

        switch (basePointDataRecord)
        {
            case IPointDataRecord pointDataRecord:
                buffers[index++].Add((byte)pointDataRecord.Classification);
                buffers[index++].Add(pointDataRecord.ScanAngleRank);
                break;
#if LAS1_4_OR_GREATER
            case IExtendedPointDataRecord extendedPointDataRecord:
                buffers[index++].Add(extendedPointDataRecord.Overlap);
                buffers[index++].Add(extendedPointDataRecord.ScannerChannel);
                buffers[index++].Add((byte)extendedPointDataRecord.Classification);
                buffers[index++].Add(extendedPointDataRecord.ScanAngle);
                break;
#endif
        }

        if (basePointDataRecord is IGpsPointDataRecord gpsPointDataRecord)
        {
            buffers[index++].Add(gpsPointDataRecord.GpsTime);
        }

#if LAS1_2_OR_GREATER
        if (basePointDataRecord is IColorPointDataRecord colorPointDataRecord)
        {
            buffers[index++].Add(colorPointDataRecord.Color.R);
            buffers[index++].Add(colorPointDataRecord.Color.G);
            buffers[index++].Add(colorPointDataRecord.Color.B);
        }
#endif

#if LAS1_3_OR_GREATER
        if (basePointDataRecord is IWaveformPointDataRecord waveformPointDataRecord)
        {
            buffers[index++].Add(waveformPointDataRecord.WavePacketDescriptorIndex);
            buffers[index++].Add(waveformPointDataRecord.ByteOffsetToWaveformData);
            buffers[index++].Add(waveformPointDataRecord.WaveformPacketSizeInBytes);
            buffers[index++].Add(waveformPointDataRecord.ReturnPointWaveformLocation);
            buffers[index++].Add(waveformPointDataRecord.ParametricDx);
            buffers[index++].Add(waveformPointDataRecord.ParametricDy);
            buffers[index++].Add(waveformPointDataRecord.ParametricDz);
        }
#endif

#if LAS1_4_OR_GREATER
        if (basePointDataRecord is INearInfraredPointDataRecord nearInfraredPointDataRecord)
        {
            buffers[index++].Add(nearInfraredPointDataRecord.NearInfrared);
        }
#endif
    }

    /// <summary>
    /// Flushes the buffers to the <see cref="RecordBatch"/>.
    /// </summary>
    /// <param name="buffers">The buffers.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="length">The length.</param>
    /// <returns>The record batch.</returns>
    /// <exception cref="InvalidOperationException">Incorrect length.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1863:Use \'CompositeFormat\'", Justification = "This is for exceptions")]
    internal static RecordBatch FlushToRecordBatch(IReadOnlyList<ColumnBuffer> buffers, Schema schema, int length)
    {
        return new(schema, GetArrays(schema, buffers, length), length);

        static IEnumerable<IArrowArray> GetArrays(Schema schema, IReadOnlyList<ColumnBuffer> buffers, int length)
        {
            for (var i = 0; i < buffers.Count; i++)
            {
                var buffer = buffers[i];

                if (buffer.Count != length)
                {
                    throw new InvalidOperationException(string.Format(Arrow.Properties.Resources.Culture, Arrow.Properties.Resources.IncorrectBufferLength, i, schema.GetFieldByIndex(i).Name, buffer.Count, length));
                }

                var array = buffer.BuildArray();

                if (array.Length != length)
                {
                    throw new InvalidOperationException(string.Format(Arrow.Properties.Resources.Culture, Arrow.Properties.Resources.IncorrectBullderBuildLength, i, array.Length, length));
                }

                buffer.Clear();

                yield return array;
            }
        }
    }

    /// <summary>
    /// Gets the arrow schema.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The arrow schema.</returns>
    internal static Schema GetArrowSchema(ILasReader reader) => GetArrowSchema(reader.Header);

    /// <summary>
    /// Gets the arrow schema.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <returns>The arrow schema.</returns>
    internal static Schema GetArrowSchema(in HeaderBlock header)
    {
        var builder = new Schema.Builder();

        builder
            .Field(new Field(Constants.Columns.X, Int32Type.Default, nullable: false))
            .Field(new Field(Constants.Columns.Y, Int32Type.Default, nullable: false))
            .Field(new Field(Constants.Columns.Z, Int32Type.Default, nullable: false))
            .Field(new Field(Constants.Columns.Intensity, UInt16Type.Default, nullable: false))
            .Field(new Field(Constants.Columns.ReturnNumber, UInt8Type.Default, nullable: false))
            .Field(new Field(Constants.Columns.NumberOfReturns, UInt8Type.Default, nullable: false))
            .Field(new Field(Constants.Columns.ScanDirectionFlag, BooleanType.Default, nullable: false))
            .Field(new Field(Constants.Columns.EdgeOfFlightLine, BooleanType.Default, nullable: false))
            .Field(new Field(Constants.Columns.Synthetic, BooleanType.Default, nullable: false))
            .Field(new Field(Constants.Columns.KeyPoint, BooleanType.Default, nullable: false))
            .Field(new Field(Constants.Columns.Withheld, BooleanType.Default, nullable: false))
            .Field(new Field(Constants.Columns.UserData, UInt8Type.Default, nullable: false))
            .Field(new Field(Constants.Columns.PointSourceId, UInt16Type.Default, nullable: false));

        var pointDataFormatId = header.PointDataFormatId;
#if LAS1_3_OR_GREATER
        switch (pointDataFormatId)
        {
            case <= GpsColorWaveformPointDataRecord.Id:
                builder
                    .Field(new Field(Constants.Columns.Legacy.Classification, UInt8Type.Default, nullable: false))
                    .Field(new Field(Constants.Columns.Legacy.ScanAngleRank, Int8Type.Default, nullable: false));
                break;
#if LAS1_4_OR_GREATER
            case > GpsColorWaveformPointDataRecord.Id:
                builder
                    .Field(new Field(Constants.Columns.Extended.Overlap, BooleanType.Default, nullable: false))
                    .Field(new Field(Constants.Columns.Extended.ScannerChannel, UInt8Type.Default, nullable: false))
                    .Field(new Field(Constants.Columns.Extended.Classification, UInt8Type.Default, nullable: false))
                    .Field(new Field(Constants.Columns.Extended.ScanAngle, Int16Type.Default, nullable: false));
                break;
#endif
        }
#else
        builder
            .Field(new Field(Constants.Columns.Legacy.Classification, UInt8Type.Default, nullable: false))
            .Field(new Field(Constants.Columns.Legacy.ScanAngleRank, Int8Type.Default, nullable: false));
#endif

        // GPS point data formats
        if (pointDataFormatId
#if !LAS1_2_OR_GREATER
            is GpsPointDataRecord.Id)
#endif
#if LAS1_2_OR_GREATER
            is GpsColorPointDataRecord.Id
#endif
#if LAS1_3_OR_GREATER
            or GpsWaveformPointDataRecord.Id
            or GpsColorWaveformPointDataRecord.Id
#endif
#if LAS1_4_OR_GREATER
            or ExtendedGpsPointDataRecord.Id
            or ExtendedGpsColorPointDataRecord.Id
            or ExtendedGpsColorNearInfraredPointDataRecord.Id
            or ExtendedGpsWaveformPointDataRecord.Id
            or ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id
#endif
#if LAS1_2_OR_GREATER
            or GpsPointDataRecord.Id)
#endif
        {
            builder.Field(new Field(Constants.Columns.Gps.GpsTime, DoubleType.Default, nullable: false));
        }

#if LAS1_2_OR_GREATER
        // Color point data formats
        if (pointDataFormatId
            is GpsColorPointDataRecord.Id
#if LAS1_3_OR_GREATER
            or GpsColorWaveformPointDataRecord.Id
#endif
#if LAS1_4_OR_GREATER
            or ExtendedGpsColorPointDataRecord.Id
            or ExtendedGpsColorNearInfraredPointDataRecord.Id
            or ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id
#endif
            or ColorPointDataRecord.Id)
        {
            builder
                .Field(new Field(Constants.Columns.Color.Red, UInt16Type.Default, nullable: false))
                .Field(new Field(Constants.Columns.Color.Green, UInt16Type.Default, nullable: false))
                .Field(new Field(Constants.Columns.Color.Blue, UInt16Type.Default, nullable: false));
        }
#endif

#if LAS1_4_OR_GREATER
        // NIR point data formats
        if (pointDataFormatId
            is ExtendedGpsColorNearInfraredPointDataRecord.Id
            or ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id)
        {
            builder.Field(new Field(Constants.Columns.Nir.NearInfrared, UInt16Type.Default, nullable: false));
        }
#endif

#if LAS1_3_OR_GREATER
        if (pointDataFormatId
            is GpsWaveformPointDataRecord.Id
#if LAS1_4_OR_GREATER
            or ExtendedGpsWaveformPointDataRecord.Id
            or ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id
#endif
            or GpsColorWaveformPointDataRecord.Id)
        {
            builder
                .Field(new Field(Constants.Columns.Waveform.WavePacketDescriptorIndex, UInt8Type.Default, nullable: false))
                .Field(new Field(Constants.Columns.Waveform.ByteOffsetToWaveformData, UInt64Type.Default, nullable: false))
                .Field(new Field(Constants.Columns.Waveform.WaveformPacketSizeInBytes, UInt32Type.Default, nullable: false))
                .Field(new Field(Constants.Columns.Waveform.ReturnPointWaveformLocation, FloatType.Default, nullable: false))
                .Field(new Field(Constants.Columns.Waveform.ParametricDx, FloatType.Default, nullable: false))
                .Field(new Field(Constants.Columns.Waveform.ParametricDy, FloatType.Default, nullable: false))
                .Field(new Field(Constants.Columns.Waveform.ParametricDz, FloatType.Default, nullable: false));
        }
#endif

        builder.Metadata(Constants.Metadata.PointDataFormatId, pointDataFormatId.ToString(System.Globalization.CultureInfo.InvariantCulture));
#if LAS1_2_OR_GREATER
        builder.Metadata(Constants.Metadata.GlobalEncoding, header.GlobalEncoding.ToString());
#endif
        builder.Metadata(Constants.Metadata.Version, header.Version.ToString());
        builder.Metadata(Constants.Metadata.Offset, header.Offset.ToString(format: null, System.Globalization.CultureInfo.InvariantCulture));
        builder.Metadata(Constants.Metadata.ScaleFactor, header.ScaleFactor.ToString(format: null, System.Globalization.CultureInfo.InvariantCulture));
#if LAS1_5_OR_GREATER
        builder.Metadata(Constants.Metadata.TimeOffset, header.TimeOffset.ToString(System.Globalization.CultureInfo.InvariantCulture));
#endif

        return builder.Build();
    }
}