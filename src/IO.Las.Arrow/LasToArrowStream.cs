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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "This _could_ be used.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "RedundantAssignment", Justification = "This _could_ be used.")]
    internal static IEnumerable<RecordBatch> ToArrowBatches(ILasReader reader, Schema schema, int batchSize = 50_000)
    {
        IReadOnlyList<ColumnBuffer> buffers = [.. schema.FieldsList.Select(f => ColumnBuffer.Create(f.DataType.TypeId, capacity: batchSize))];

        var rowCount = 0;
        var hasYielded = false;

        while (reader.ReadPointDataRecord() is { PointDataRecord: { } basePointDataRecord })
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

            if (basePointDataRecord is IPointDataRecord pointDataRecord)
            {
                buffers[index++].Add((byte)pointDataRecord.Classification);
                buffers[index++].Add(pointDataRecord.ScanAngleRank);
            }

#if LAS1_4_OR_GREATER
            if (basePointDataRecord is IExtendedPointDataRecord extendedPointDataRecord)
            {
                buffers[index++].Add(extendedPointDataRecord.Overlap);
                buffers[index++].Add(extendedPointDataRecord.ScannerChannel);
                buffers[index++].Add((byte)extendedPointDataRecord.Classification);
                buffers[index++].Add(extendedPointDataRecord.ScanAngle);
            }
#endif

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

#if LAS1_4_OR_GREATER
            if (basePointDataRecord is INearInfraredPointDataRecord nearInfraredPointDataRecord)
            {
                buffers[index++].Add(nearInfraredPointDataRecord.NearInfrared);
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
                buffers[index].Add(waveformPointDataRecord.ParametricDz);
            }
#endif

            rowCount++;

            if (rowCount < batchSize)
            {
                continue;
            }

            yield return Flush(schema, buffers, rowCount);

            rowCount = 0;
            hasYielded = true;
        }

        if (rowCount > 0)
        {
            yield return Flush(schema, buffers, rowCount);
            hasYielded = true;
        }

        if (!hasYielded)
        {
            yield return Flush(schema, buffers, 0);
        }

        static RecordBatch Flush(Schema schema, IReadOnlyList<ColumnBuffer> buffers, int length)
        {
            return new(schema, GetArrays(schema, buffers, length), length);

            static IEnumerable<IArrowArray> GetArrays(Schema schema, IReadOnlyList<ColumnBuffer> buffers, int length)
            {
                for (var i = 0; i < buffers.Count; i++)
                {
                    var buffer = buffers[i];

                    if (buffer.Count != length)
                    {
                        throw new InvalidOperationException($"Column {i} ('{schema.GetFieldByIndex(i).Name}') has {buffer.Count} elements, but we are creating a batch of length {length}!");
                    }

                    var array = buffer.BuildArray();

                    if (array.Length != length)
                    {
                        throw new InvalidOperationException($"Column {i} BuildArray() returned length {array.Length}, expected {length}. ArrowConverter might be adding extras?");
                    }

                    buffer.Clear();

                    yield return array;
                }
            }
        }
    }

    /// <summary>
    /// Gets the arrow schema.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The arrow schema.</returns>
    internal static Schema GetArrowSchema(ILasReader reader)
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

        var pointDataFormatId = reader.Header.PointDataFormatId;
#if LAS1_3_OR_GREATER
        if (pointDataFormatId <= GpsColorWaveformPointDataRecord.Id)
        {
            builder
                .Field(new Field(Constants.Columns.Legacy.Classification, UInt8Type.Default, nullable: false))
                .Field(new Field(Constants.Columns.Legacy.ScanAngleRank, Int8Type.Default, nullable: false));
        }
#else
        builder
            .Field(new Field(Constants.Columns.Legacy.Classification, UInt8Type.Default, nullable: false))
            .Field(new Field(Constants.Columns.Legacy.ScanAngleRank, Int8Type.Default, nullable: false));
#endif

#if LAS1_4_OR_GREATER
        if (pointDataFormatId > GpsColorWaveformPointDataRecord.Id)
        {
            builder
                .Field(new Field(Constants.Columns.Extended.Overlap, BooleanType.Default, nullable: false))
                .Field(new Field(Constants.Columns.Extended.ScannerChannel, UInt8Type.Default, nullable: false))
                .Field(new Field(Constants.Columns.Extended.Classification, UInt8Type.Default, nullable: false))
                .Field(new Field(Constants.Columns.Extended.ScanAngle, Int16Type.Default, nullable: false));
        }
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
        builder.Metadata(Constants.Metadata.GlobalEncoding, reader.Header.GlobalEncoding.ToString());
#endif
        builder.Metadata(Constants.Metadata.Version, reader.Header.Version.ToString());
        builder.Metadata(Constants.Metadata.Offset, reader.Header.Offset.ToString(format: null, System.Globalization.CultureInfo.InvariantCulture));
        builder.Metadata(Constants.Metadata.ScaleFactor, reader.Header.ScaleFactor.ToString(format: null, System.Globalization.CultureInfo.InvariantCulture));
#if LAS1_5_OR_GREATER
        builder.Metadata(Constants.Metadata.TimeOffset, reader.Header.TimeOffset.ToString(System.Globalization.CultureInfo.InvariantCulture));
#endif

        return builder.Build();
    }

    private abstract class ColumnBuffer
    {
        public abstract int Count { get; }

        public static ColumnBuffer Create(ArrowTypeId typeId, int capacity) =>
            typeId switch
            {
                ArrowTypeId.Boolean => new BooleanColumnBuffer(capacity),
                ArrowTypeId.UInt8 => new UInt8ColumnBuffer(capacity),
                ArrowTypeId.Int8 => new Int8ColumnBuffer(capacity),
                ArrowTypeId.UInt16 => new UInt16ColumnBuffer(capacity),
                ArrowTypeId.Int16 => new Int16ColumnBuffer(capacity),
                ArrowTypeId.UInt32 => new UInt32ColumnBuffer(capacity),
                ArrowTypeId.Int32 => new Int32ColumnBuffer(capacity),
                ArrowTypeId.UInt64 => new UInt64ColumnBuffer(capacity),
                ArrowTypeId.Int64 => new Int64ColumnBuffer(capacity),
                ArrowTypeId.Float => new SingleColumnBuffer(capacity),
                ArrowTypeId.Double => new DoubleColumnBuffer(capacity),
                _ => throw new NotSupportedException(),
            };

        public abstract void Add(object? value);

        public abstract void Clear();

        public abstract IArrowArray BuildArray();

        private abstract class PrimitiveColumnBuffer<T, TArray, TBuilder>(int capacity) : ColumnBuffer, IEnumerable<T?>
            where T : struct
            where TArray : IArrowArray
            where TBuilder : class, IArrowArrayBuilder<TArray>
        {
            private readonly List<T?> buffer = new(capacity);

            public override int Count => this.buffer.Count;

            public override void Add(object? value)
            {
                if (value is T t)
                {
                    this.buffer.Add(t);
                }
                else if (value is null || value == DBNull.Value)
                {
                    this.buffer.Add(default);
                }
                else
                {
                    // try to cast it
                    this.buffer.Add((T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            public override void Clear() => this.buffer.Clear();

            public override IArrowArray BuildArray()
            {
                var builder = this.GetArrayBuilder();
                foreach (var v in this)
                {
                    if (v.HasValue)
                    {
                        builder.Append(v.Value);
                    }
                    else
                    {
                        builder.AppendNull();
                    }
                }

                return builder.Build(allocator: default);
            }

            public IEnumerator<T?> GetEnumerator() => this.buffer.GetEnumerator();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

            protected abstract IArrowArrayBuilder<T, TArray, TBuilder> GetArrayBuilder();
        }

        private sealed class BooleanColumnBuffer(int capacity) : PrimitiveColumnBuffer<bool, BooleanArray, BooleanArray.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<bool, BooleanArray, BooleanArray.Builder> GetArrayBuilder() => new BooleanArray.Builder();
        }

        private sealed class UInt8ColumnBuffer(int capacity) : PrimitiveColumnBuffer<byte, UInt8Array, UInt8Array.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<byte, UInt8Array, UInt8Array.Builder> GetArrayBuilder() => new UInt8Array.Builder();
        }

        private sealed class Int8ColumnBuffer(int capacity) : PrimitiveColumnBuffer<sbyte, Int8Array, Int8Array.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<sbyte, Int8Array, Int8Array.Builder> GetArrayBuilder() => new Int8Array.Builder();
        }

        private sealed class UInt16ColumnBuffer(int capacity) : PrimitiveColumnBuffer<ushort, UInt16Array, UInt16Array.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<ushort, UInt16Array, UInt16Array.Builder> GetArrayBuilder() => new UInt16Array.Builder();
        }

        private sealed class Int16ColumnBuffer(int capacity) : PrimitiveColumnBuffer<short, Int16Array, Int16Array.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<short, Int16Array, Int16Array.Builder> GetArrayBuilder() => new Int16Array.Builder();
        }

        private sealed class UInt32ColumnBuffer(int capacity) : PrimitiveColumnBuffer<uint, UInt32Array, UInt32Array.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<uint, UInt32Array, UInt32Array.Builder> GetArrayBuilder() => new UInt32Array.Builder();
        }

        private sealed class Int32ColumnBuffer(int capacity) : PrimitiveColumnBuffer<int, Int32Array, Int32Array.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<int, Int32Array, Int32Array.Builder> GetArrayBuilder() => new Int32Array.Builder();
        }

        private sealed class UInt64ColumnBuffer(int capacity) : PrimitiveColumnBuffer<ulong, UInt64Array, UInt64Array.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<ulong, UInt64Array, UInt64Array.Builder> GetArrayBuilder() => new UInt64Array.Builder();
        }

        private sealed class Int64ColumnBuffer(int capacity) : PrimitiveColumnBuffer<long, Int64Array, Int64Array.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<long, Int64Array, Int64Array.Builder> GetArrayBuilder() => new Int64Array.Builder();
        }

        private sealed class SingleColumnBuffer(int capacity) : PrimitiveColumnBuffer<float, FloatArray, FloatArray.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<float, FloatArray, FloatArray.Builder> GetArrayBuilder() => new FloatArray.Builder();
        }

        private sealed class DoubleColumnBuffer(int capacity) : PrimitiveColumnBuffer<double, DoubleArray, DoubleArray.Builder>(capacity)
        {
            protected override IArrowArrayBuilder<double, DoubleArray, DoubleArray.Builder> GetArrayBuilder() => new DoubleArray.Builder();
        }
    }
}