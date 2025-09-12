// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsPointDataRecordWriter{T}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed <see cref="Writers.IPointDataRecordWriter"/> for <see cref="IExtendedPointDataRecord"/> instances.
/// </summary>
/// <typeparam name="T">The type of extended point data record.</typeparam>
internal abstract class ExtendedGpsPointDataRecordWriter<T> : Writers.PointDataRecordWriter<T>, IContextWriter
    where T : IExtendedPointDataRecord
{
    private const int Multiple = 500;

    private const int MultipleMinus = -10;

    private const int MultipleCodeFull = Multiple - MultipleMinus + 1;

    private const int MultipleTotal = Multiple - MultipleMinus + 5;

    private readonly IEntropyEncoder encoder;
    private readonly Context[] contexts = new Context[4];
    private readonly LayeredValue valueChannelReturnsXY = new();
    private readonly LayeredValue valueZ = new();
    private readonly LayeredValue valueClassification = new();
    private readonly LayeredValue valueFlags = new();
    private readonly LayeredValue valueIntensity = new();
    private readonly LayeredValue valueScanAngle = new();
    private readonly LayeredValue valueUserData = new();
    private readonly LayeredValue valuePointSource = new();
    private readonly LayeredValue valueGpsTime = new();
    private uint currentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedGpsPointDataRecordWriter{T}"/> class.
    /// </summary>
    /// <param name="encoder">The Encoder.</param>
    protected ExtendedGpsPointDataRecordWriter(IEntropyEncoder encoder)
    {
        this.encoder = encoder;
        this.contexts[0] = new(this.valueChannelReturnsXY, this.valueZ, this.valueIntensity, this.valueScanAngle, this.valuePointSource, this.valueGpsTime);
        this.contexts[1] = new(this.valueChannelReturnsXY, this.valueZ, this.valueIntensity, this.valueScanAngle, this.valuePointSource, this.valueGpsTime);
        this.contexts[2] = new(this.valueChannelReturnsXY, this.valueZ, this.valueIntensity, this.valueScanAngle, this.valuePointSource, this.valueGpsTime);
        this.contexts[3] = new(this.valueChannelReturnsXY, this.valueZ, this.valueIntensity, this.valueScanAngle, this.valuePointSource, this.valueGpsTime);
    }

    /// <inheritdoc/>
    public virtual bool ChunkSizes()
    {
        var writer = this.encoder.GetStream();

        // finish the encoders
        this.valueChannelReturnsXY.EncoderDone();
        this.valueZ.EncoderDone();
        this.valueClassification.EncoderDoneIfChanged();
        this.valueFlags.EncoderDoneIfChanged();
        this.valueIntensity.EncoderDoneIfChanged();
        this.valueScanAngle.EncoderDoneIfChanged();
        this.valueUserData.EncoderDoneIfChanged();
        this.valuePointSource.EncoderDoneIfChanged();
        this.valueGpsTime.EncoderDoneIfChanged();

        // output the sizes of all layer (i.e. number of bytes per layer)
        writer.WriteUInt32LittleEndian(this.valueChannelReturnsXY.GetByteCount());
        writer.WriteUInt32LittleEndian(this.valueZ.GetByteCount());
        writer.WriteUInt32LittleEndian(this.valueClassification.GetByteCountIfChanged());
        writer.WriteUInt32LittleEndian(this.valueFlags.GetByteCountIfChanged());
        writer.WriteUInt32LittleEndian(this.valueIntensity.GetByteCountIfChanged());
        writer.WriteUInt32LittleEndian(this.valueScanAngle.GetByteCountIfChanged());
        writer.WriteUInt32LittleEndian(this.valueUserData.GetByteCountIfChanged());
        writer.WriteUInt32LittleEndian(this.valuePointSource.GetByteCountIfChanged());
        writer.WriteUInt32LittleEndian(this.valueGpsTime.GetByteCountIfChanged());

        return true;
    }

    /// <inheritdoc/>
    public virtual bool ChunkBytes()
    {
        var stream = this.encoder.GetStream();

        // output the bytes of all layers
        this.valueChannelReturnsXY.CopyTo(stream);
        this.valueZ.CopyTo(stream);
        this.valueClassification.CopyToIfChanged(stream);
        this.valueFlags.CopyToIfChanged(stream);
        this.valueIntensity.CopyToIfChanged(stream);
        this.valueScanAngle.CopyToIfChanged(stream);
        this.valueUserData.CopyToIfChanged(stream);
        this.valuePointSource.CopyToIfChanged(stream);
        this.valueGpsTime.CopyToIfChanged(stream);

        return true;
    }

    /// <inheritdoc/>
    public virtual bool Initialize(ReadOnlySpan<byte> item, ref uint context) => this.Initialize(item, ref context, item.Length);

    /// <inheritdoc/>
    public sealed override int Write(Span<byte> destination, [System.Diagnostics.CodeAnalysis.NotNull] T record, ReadOnlySpan<byte> extraBytes)
    {
        var context = default(uint);
        var bytesWritten = record.CopyTo(destination);
        extraBytes.CopyTo(destination[bytesWritten..]);
        bytesWritten += extraBytes.Length;
        this.Write(destination[..bytesWritten], ref context);
        return default;
    }

    /// <inheritdoc/>
    public virtual void Write(Span<byte> item, ref uint context) => this.Write(item, ref context, item.Length);

    /// <inheritdoc/>
    public sealed override async ValueTask<int> WriteAsync(Memory<byte> destination, [System.Diagnostics.CodeAnalysis.NotNull] T record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default)
    {
        var bytesWritten = record.CopyTo(destination.Span);
        extraBytes.CopyTo(destination[bytesWritten..]);
        bytesWritten += extraBytes.Length;
        _ = await this.WriteAsync(destination[..bytesWritten], default, cancellationToken).ConfigureAwait(false);
        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask<uint> WriteAsync(Memory<byte> item, uint context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.Write(item.Span, ref context);
        return new(context);
    }

    /// <summary>
    /// Initializes the reader with the specified data.
    /// </summary>
    /// <param name="item">The item to initialize from.</param>
    /// <param name="context">The context.</param>
    /// <param name="gpsTimeChangedIndex">The GPS time changed index.</param>
    /// <returns><see langword="true"/> if successfully initialized; otherwise <see langword="false"/>.</returns>
    protected bool Initialize(ReadOnlySpan<byte> item, ref uint context, int gpsTimeChangedIndex)
    {
        // on the first Init create instreams and Encoders
        this.valueChannelReturnsXY.Initialize();
        this.valueZ.Initialize();
        this.valueClassification.Initialize();
        this.valueFlags.Initialize();
        this.valueIntensity.Initialize();
        this.valueScanAngle.Initialize();
        this.valueUserData.Initialize();
        this.valuePointSource.Initialize();
        this.valueGpsTime.Initialize();

        // mark the four scanner channel contexts as unused
        for (var c = 0; c < 4; c++)
        {
            this.contexts[c].Unused = true;
        }

        // set scanner channel as current context
        this.currentContext = new ExtendedGpsPointDataRecord(item).ScannerChannel;
        context = this.currentContext; // the POINT14 Writer sets context for all other items

        // create and init entropy models and integer compressors (and init context from item)
        this.CreateAndInitModelsAndCompressors(this.currentContext, item, gpsTimeChangedIndex);

        return true;
    }

    /// <summary>
    /// Writes the value into the specified item at the start index, with a context.
    /// </summary>
    /// <param name="item">The item to write to.</param>
    /// <param name="context">The context.</param>
    /// <param name="gpsTimeChangeIndex">The gps time change index.</param>
    protected void Write(Span<byte> item, ref uint context, int gpsTimeChangeIndex)
    {
        var processingContext = this.contexts[this.currentContext];

        // get last
        var lastItem = processingContext.LastItem;

        ////////////////////////////////////////
        // compress XY layer
        ////////////////////////////////////////

        // create single (3) / first (1) / last (2) / intermediate (0) context from last point return
        var lastPoint = new ExtendedGpsPointDataRecord(lastItem);
        var lastPointReturn = GetLastPointReturn(lastPoint, lastItem[gpsTimeChangeIndex]);

        // get the (potentially new) context
        var point = new ExtendedGpsPointDataRecord(item);
        uint scannerChannel = point.ScannerChannel;

        // if context has changed (and the new context already exists) get last for new context
        if (scannerChannel != this.currentContext && !this.contexts[scannerChannel].Unused)
        {
            lastItem = this.contexts[scannerChannel].LastItem;
            lastPoint = new(lastItem);
        }

        // determine changed attributes
        var pointSourceChange = point.PointSourceId != lastPoint.PointSourceId;
        var gpsTimeChange = !point.GpsTime.Equals(lastPoint.GpsTime);
        var scanAngleChange = point.ScanAngle != lastPoint.ScanAngle;

        // get last and current return counts
        uint lastNumberOfReturns = lastPoint.NumberOfReturns;
        uint lastReturnNumber = lastPoint.ReturnNumber;

        uint numberOfReturns = point.NumberOfReturns;
        uint returnNumber = point.ReturnNumber;

        // create the 7 bit mask that encodes various changes (its value ranges from 0 to 127)
        var changedValues = ((scannerChannel != this.currentContext ? 1 : 0) << 6) | // scanner channel compared to last point (same = 0 / different = 1)
                             ((pointSourceChange ? 1 : 0) << 5) | // point source ID compared to last point from *same* scanner channel (same = 0 / different = 1)
                             ((gpsTimeChange ? 1 : 0) << 4) | // GPS time stamp compared to last point from *same* scanner channel (same = 0 / different = 1)
                             ((scanAngleChange ? 1 : 0) << 3) | // scan angle compared to last point from *same* scanner channel (same = 0 / different = 1)
                             (((numberOfReturns != lastNumberOfReturns) ? 1 : 0) << 2); // number of returns compared to last point from *same* scanner channel (same = 0 / different = 1)

        // return number compared to last point of *same* scanner channel (same = 0 / plus one mod 16 = 1 / minus one mod 16 = 2 / other difference = 3)
        if (returnNumber != lastReturnNumber)
        {
            if (returnNumber == ((lastReturnNumber + 1) % 16))
            {
                changedValues |= 1;
            }
            else if (returnNumber == ((lastReturnNumber + 15) % 16))
            {
                changedValues |= 2;
            }
            else
            {
                changedValues |= 3;
            }
        }

        // compress the 7 bit mask that encodes changes with last point return context
        this.valueChannelReturnsXY.Encoder.EncodeSymbol(processingContext.ChangedValuesModels[lastPointReturn], (uint)changedValues);

        // if scanner channel has changed, record change
        if ((changedValues & (1 << 6)) is not 0)
        {
            if (scannerChannel > this.currentContext)
            {
                this.valueChannelReturnsXY.Encoder.EncodeSymbol(processingContext.ScannerChannelModel, scannerChannel - this.currentContext - 1U);
            }
            else
            {
                this.valueChannelReturnsXY.Encoder.EncodeSymbol(processingContext.ScannerChannelModel, scannerChannel - this.currentContext + 4U - 1U);
            }

            // maybe create and init entropy models and integer compressors
            if (this.contexts[scannerChannel].Unused)
            {
                // create and init entropy models and integer compressors (and init context from last item)
                this.CreateAndInitModelsAndCompressors(scannerChannel, processingContext.LastItem, gpsTimeChangeIndex);

                // get last for new context
                lastItem = this.contexts[scannerChannel].LastItem;
                lastPoint = new(lastItem);
            }

            // switch context to current scanner channel
            this.currentContext = scannerChannel;
            context = this.currentContext; // the POINT14 writer sets context for all other items
            processingContext = this.contexts[this.currentContext];
        }

        ISymbolModel? model;

        // if number of returns is different we compress it
        if ((changedValues & (1 << 2)) is not 0)
        {
            model = processingContext.NumberOfReturnsModels[lastNumberOfReturns];
            if (model is null)
            {
                model = this.valueChannelReturnsXY.Encoder.CreateSymbolModel(16);
                processingContext.NumberOfReturnsModels[lastNumberOfReturns] = model;
                _ = model.Initialize();
            }

            this.valueChannelReturnsXY.Encoder.EncodeSymbol(model, numberOfReturns);
        }

        // if return number is different and difference is bigger than +1 / -1 we compress how it is different
        if ((changedValues & 3) is 3)
        {
            if (gpsTimeChange)
            {
                // if the GPS time has changed
                model = processingContext.ReturnNumberModels[lastReturnNumber];
                if (model is null)
                {
                    model = this.valueChannelReturnsXY.Encoder.CreateSymbolModel(16);
                    processingContext.ReturnNumberModels[lastReturnNumber] = model;
                    _ = model.Initialize();
                }

                this.valueChannelReturnsXY.Encoder.EncodeSymbol(model, returnNumber);
            }
            else
            {
                // if the GPS time has not changed
                if (returnNumber > lastReturnNumber)
                {
                    // r = lastR + (sym + 2) with sym = diff - 2
                    this.valueChannelReturnsXY.Encoder.EncodeSymbol(processingContext.ReturnNumberGpsSameModel, returnNumber - lastReturnNumber - 2U);
                }
                else
                {
                    // r = (lastR + (sym + 2)) % 16 with sym = diff + 16 - 2
                    this.valueChannelReturnsXY.Encoder.EncodeSymbol(processingContext.ReturnNumberGpsSameModel, returnNumber - lastReturnNumber + 16U - 2U);
                }
            }
        }

        // get return map m and return level l context for current point
        uint m = Common.NumberReturnMap6Context[numberOfReturns][returnNumber];
        uint l = Common.NumberReturnLevel8Context[numberOfReturns][returnNumber];

        // create single (3) / first (1) / last (2) / intermediate (0) return context for current point
        var currentPointReturn = returnNumber is 1 ? 2 : 0; // first ?
        currentPointReturn += returnNumber >= numberOfReturns ? 1 : 0; // last ?

        // compress X coordinate
        var mapContext = (m << 1) | (gpsTimeChange ? 1U : 0U);
        var median = processingContext.LastXDiffMedian5[mapContext].Get();
        var diff = point.X - lastPoint.X;
        var coordinateContext = numberOfReturns is 1U ? 1U : 0U;
        processingContext.DeltaXIntegerCompressor.Compress(median, diff, coordinateContext);
        processingContext.LastXDiffMedian5[mapContext].Add(diff);

        // compress Y coordinate
        var kBits = processingContext.DeltaXIntegerCompressor.K;
        median = processingContext.LastYDiffMedian5[mapContext].Get();
        diff = point.Y - lastPoint.Y;
        processingContext.DeltaYIntegerCompressor.Compress(median, diff, coordinateContext + (kBits < 20U ? kBits.ZeroBit0() : 20));
        processingContext.LastYDiffMedian5[mapContext].Add(diff);

        ////////////////////////////////////////
        // compress Z layer
        ////////////////////////////////////////

        kBits = (processingContext.DeltaXIntegerCompressor.K + processingContext.DeltaYIntegerCompressor.K) / 2;
        processingContext.ZIntegerCompressor.Compress(processingContext.LastZ[l], point.Z, coordinateContext + (kBits < 18U ? kBits.ZeroBit0() : 18));
        processingContext.LastZ[l] = point.Z;

        ////////////////////////////////////////
        // compress classifications layer
        ////////////////////////////////////////

        var lastClassification = (uint)lastPoint.Classification;
        var classification = (uint)point.Classification;

        this.valueClassification.Changed |= classification != lastClassification;

        var ccc = (int)(((lastClassification & 0x1F) << 1) + (currentPointReturn is 3 ? 1U : 0U));
        model = processingContext.ClassificationModels[ccc];
        if (model is null)
        {
            model = this.valueClassification.Encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
            processingContext.ClassificationModels[ccc] = model;
            _ = model.Initialize();
        }

        this.valueClassification.Encoder.EncodeSymbol(model, classification);

        ////////////////////////////////////////
        // compress flags layer
        ////////////////////////////////////////

        var lastFlags = ((lastPoint.Synthetic ? 1U : 0U) << 0)
            | ((lastPoint.KeyPoint ? 1U : 0U) << 1)
            | ((lastPoint.Withheld ? 1U : 0U) << 2)
            | ((lastPoint.Overlap ? 1U : 0U) << 3)
            | ((lastPoint.ScanDirectionFlag ? 1U : 0U) << 4)
            | ((lastPoint.EdgeOfFlightLine ? 1U : 0U) << 5);
        var flags = ((point.Synthetic ? 1U : 0U) << 0)
            | ((point.KeyPoint ? 1U : 0U) << 1)
            | ((point.Withheld ? 1U : 0U) << 2)
            | ((point.Overlap ? 1U : 0U) << 3)
            | ((point.ScanDirectionFlag ? 1U : 0U) << 4)
            | ((point.EdgeOfFlightLine ? 1U : 0U) << 5);

        this.valueFlags.Changed |= flags != lastFlags;
        model = processingContext.FlagsModels[lastFlags];
        if (model is null)
        {
            model = this.valueFlags.Encoder.CreateSymbolModel(64);
            processingContext.FlagsModels[lastFlags] = model;
            _ = model.Initialize();
        }

        this.valueFlags.Encoder.EncodeSymbol(model, flags);

        ////////////////////////////////////////
        // compress intensity layer
        ////////////////////////////////////////
        this.valueIntensity.Changed |= point.Intensity != lastPoint.Intensity;
        processingContext.IntensityIntegerCompressor.Compress(processingContext.LastIntensity[(uint)(currentPointReturn << 1) | (gpsTimeChange ? 1U : 0U)], point.Intensity, (uint)currentPointReturn);
        processingContext.LastIntensity[(uint)(currentPointReturn << 1) | (gpsTimeChange ? 1U : 0U)] = point.Intensity;

        ////////////////////////////////////////
        // compress scan angle layer
        ////////////////////////////////////////

        if (scanAngleChange)
        {
            this.valueScanAngle.Changed = true;
            processingContext.ScanAngleIntegerCompressor.Compress(lastPoint.ScanAngle, (uint)point.ScanAngle, gpsTimeChange ? 1U : 0U); // if the GPS time has changed
        }

        ////////////////////////////////////////
        // compress user data layer
        ////////////////////////////////////////
        this.valueUserData.Changed |= point.UserData != lastPoint.UserData;
        model = processingContext.UserDataModels[lastPoint.UserData / 4];
        if (model is null)
        {
            model = this.valueUserData.Encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
            processingContext.UserDataModels[lastPoint.UserData / 4] = model;
            _ = model.Initialize();
        }

        this.valueUserData.Encoder.EncodeSymbol(model, point.UserData);

        ////////////////////////////////////////
        // compress point source layer
        ////////////////////////////////////////

        if (pointSourceChange)
        {
            this.valuePointSource.Changed = true;
            processingContext.PointSourceIdIntegerCompressor.Compress(lastPoint.PointSourceId, point.PointSourceId);
        }

        ////////////////////////////////////////
        // compress GPS time layer
        ////////////////////////////////////////

        if (gpsTimeChange)
        {
            // if the GPS time has changed
            this.valueGpsTime.Changed = true;
            this.WriteGpsTime(BitConverter.DoubleToUInt64Bits(System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(item[(ExtendedGpsPointDataRecord.Size - sizeof(double))..])));
        }

        // copy the last item
        item[..ExtendedGpsPointDataRecord.Size].CopyTo(lastItem);

        // remember if the last point had a GPS time change
        lastItem[gpsTimeChangeIndex] = gpsTimeChange ? (byte)1 : default;

        static int GetLastPointReturn(ExtendedGpsPointDataRecord lastPoint, byte gpsTimeChange)
        {
            // Whether this is the first return.
            var first = lastPoint.ReturnNumber is 1 ? 1 : 0;

            // Whether this is the last return.
            var last = lastPoint.ReturnNumber >= lastPoint.NumberOfReturns ? 2 : 0;

            // whether the GPS time changed
            var gps = gpsTimeChange is 0 ? 0 : 4;

            return first + last + gps;
        }
    }

    private void CreateAndInitModelsAndCompressors(uint context, ReadOnlySpan<byte> item, int gpsTimeChangeIndex)
    {
        var contextToInitialize = this.contexts[context];

        // then init entropy models and integer compressors

        // for the XY layer
        _ = contextToInitialize.ChangedValuesModels[0].Initialize();
        _ = contextToInitialize.ChangedValuesModels[1].Initialize();
        _ = contextToInitialize.ChangedValuesModels[2].Initialize();
        _ = contextToInitialize.ChangedValuesModels[3].Initialize();
        _ = contextToInitialize.ChangedValuesModels[4].Initialize();
        _ = contextToInitialize.ChangedValuesModels[5].Initialize();
        _ = contextToInitialize.ChangedValuesModels[6].Initialize();
        _ = contextToInitialize.ChangedValuesModels[7].Initialize();
        _ = contextToInitialize.ScannerChannelModel.Initialize();
        for (var i = 0; i < 16; i++)
        {
            _ = contextToInitialize.NumberOfReturnsModels[i]?.Initialize();
            _ = contextToInitialize.ReturnNumberModels[i]?.Initialize();
        }

        _ = contextToInitialize.ReturnNumberGpsSameModel.Initialize();
        contextToInitialize.DeltaXIntegerCompressor.Initialize();
        contextToInitialize.DeltaYIntegerCompressor.Initialize();
        for (var i = 0; i < 12; i++)
        {
            contextToInitialize.LastXDiffMedian5[i] = new();
            contextToInitialize.LastYDiffMedian5[i] = new();
        }

        // for the Z layer
        contextToInitialize.ZIntegerCompressor.Initialize();
        for (var i = 0; i < 8; i++)
        {
            contextToInitialize.LastZ[i] = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(item[8..]);
        }

        // for the classification, flags, and user data layers
        for (var i = 0; i < 64; i++)
        {
            _ = contextToInitialize.ClassificationModels[i]?.Initialize();
            _ = contextToInitialize.FlagsModels[i]?.Initialize();
            _ = contextToInitialize.UserDataModels[i]?.Initialize();
        }

        // for the intensity layer
        contextToInitialize.IntensityIntegerCompressor.Initialize();
        for (var i = 0; i < 8; i++)
        {
            contextToInitialize.LastIntensity[i] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[12..]);
        }

        // for the scan angle layer
        contextToInitialize.ScanAngleIntegerCompressor.Initialize();

        // for the point source ID layer
        contextToInitialize.PointSourceIdIntegerCompressor.Initialize();

        // for the GPS time layer
        _ = contextToInitialize.GpsTimeMultiModel.Initialize();
        _ = contextToInitialize.GpsTimeZeroDiffModel.Initialize();
        contextToInitialize.GpsTimeIntegerCompressor.Initialize();
        contextToInitialize.Last = default;
        contextToInitialize.Next = default;
        Array.Clear(contextToInitialize.LastGpsTimeDiff, 0, 4);
        Array.Clear(contextToInitialize.MultiExtremeCounter, 0, 4);
        contextToInitialize.LastGpsTime[0] = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(item[(ExtendedGpsPointDataRecord.Size - sizeof(double))..]);
        Array.Clear(contextToInitialize.LastGpsTime, 1, 3);

        // init current context from last item
        item[..ExtendedGpsPointDataRecord.Size].CopyTo(contextToInitialize.LastItem);
        contextToInitialize.LastItem[gpsTimeChangeIndex] = default;

        contextToInitialize.Unused = false;
    }

    private void WriteGpsTime(ulong gpsTime)
    {
        var processingContext = this.contexts[this.currentContext];

        if (processingContext.LastGpsTimeDiff[processingContext.Last] is 0)
        {
            // if the last integer difference was zero
            // calculate the difference between the two doubles as an integer
            var currentGpsTimeDiff64 = BitConverter.UInt64BitsToInt64Bits(gpsTime) - BitConverter.UInt64BitsToInt64Bits(processingContext.LastGpsTime[processingContext.Last]);
            if (currentGpsTimeDiff64.IsInt32())
            {
                // the difference can be represented with 32 bits
                var currentGpsTimeDiff = (int)currentGpsTimeDiff64;
                this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeZeroDiffModel, 0);
                processingContext.GpsTimeIntegerCompressor.Compress(0, currentGpsTimeDiff);
                processingContext.LastGpsTimeDiff[processingContext.Last] = currentGpsTimeDiff;
                processingContext.MultiExtremeCounter[processingContext.Last] = default;
            }
            else
            {
                // the difference is huge
                // maybe the double belongs to another time sequence
                for (var i = 1U; i < 4U; i++)
                {
                    var otherGpsTimeDiff64 = BitConverter.UInt64BitsToInt64Bits(gpsTime) - BitConverter.UInt64BitsToInt64Bits(processingContext.LastGpsTime[(processingContext.Last + i) & 3]);
                    if (otherGpsTimeDiff64.IsInt32())
                    {
                        // it belongs to another sequence
                        this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeZeroDiffModel, i + 1);
                        processingContext.Last = (processingContext.Last + i) & 3;
                        this.WriteGpsTime(gpsTime);
                        return;
                    }
                }

                // no other sequence found. start new sequence.
                this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeZeroDiffModel, 1);
                processingContext.GpsTimeIntegerCompressor.Compress((int)(processingContext.LastGpsTime[processingContext.Last] >> 32), (int)(gpsTime >> 32), 8);
                this.valueGpsTime.Encoder.WriteInt((uint)gpsTime);
                processingContext.Next = (processingContext.Next + 1) & 3;
                processingContext.Last = processingContext.Next;
                processingContext.LastGpsTimeDiff[processingContext.Last] = default;
                processingContext.MultiExtremeCounter[processingContext.Last] = default;
            }

            processingContext.LastGpsTime[processingContext.Last] = gpsTime;
        }
        else
        {
            // the last integer difference was *not* zero
            // calculate the difference between the two doubles as an integer
            var currentGpsTimeDiff64 = BitConverter.UInt64BitsToInt64Bits(gpsTime) - BitConverter.UInt64BitsToInt64Bits(processingContext.LastGpsTime[processingContext.Last]);

            // if the current GPS time difference can be represented with 32 bits
            if (currentGpsTimeDiff64.IsInt32())
            {
                // compute multiplier between current and last integer difference
                var currentGpsTimeDiff = (int)currentGpsTimeDiff64;
                var multi = (currentGpsTimeDiff / (float)processingContext.LastGpsTimeDiff[processingContext.Last]).Quantize();

                // compress the residual current GPS time difference in dependence on the multiplier
                if (multi is 1)
                {
                    // this is the case we assume we get most often for regular spaced pulses
                    this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeMultiModel, 1);
                    processingContext.GpsTimeIntegerCompressor.Compress(processingContext.LastGpsTimeDiff[processingContext.Last], currentGpsTimeDiff, 1);
                    processingContext.MultiExtremeCounter[processingContext.Last] = default;
                }
                else if (multi > 0)
                {
                    if (multi < Multiple)
                    {
                        // positive multipliers up to LASZIPGPSTIMEMULTI are compressed directly
                        this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeMultiModel, (uint)multi);
                        processingContext.GpsTimeIntegerCompressor.Compress(multi * processingContext.LastGpsTimeDiff[processingContext.Last], currentGpsTimeDiff, multi < 10 ? 2U : 3U);
                    }
                    else
                    {
                        this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeMultiModel, Multiple);
                        processingContext.GpsTimeIntegerCompressor.Compress(Multiple * processingContext.LastGpsTimeDiff[processingContext.Last], currentGpsTimeDiff, 4);
                        processingContext.MultiExtremeCounter[processingContext.Last]++;
                        if (processingContext.MultiExtremeCounter[processingContext.Last] > 3)
                        {
                            processingContext.LastGpsTimeDiff[processingContext.Last] = currentGpsTimeDiff;
                            processingContext.MultiExtremeCounter[processingContext.Last] = default;
                        }
                    }
                }
                else if (multi < 0)
                {
                    if (multi > MultipleMinus)
                    {
                        // negative multipliers larger than LASZIPGPSTIMEMULTIMINUS are compressed directly
                        this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeMultiModel, (uint)(Multiple - multi));
                        processingContext.GpsTimeIntegerCompressor.Compress(multi * processingContext.LastGpsTimeDiff[processingContext.Last], currentGpsTimeDiff, 5);
                    }
                    else
                    {
                        this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeMultiModel, Multiple - MultipleMinus);
                        processingContext.GpsTimeIntegerCompressor.Compress(MultipleMinus * processingContext.LastGpsTimeDiff[processingContext.Last], currentGpsTimeDiff, 6);
                        processingContext.MultiExtremeCounter[processingContext.Last]++;
                        if (processingContext.MultiExtremeCounter[processingContext.Last] > 3)
                        {
                            processingContext.LastGpsTimeDiff[processingContext.Last] = currentGpsTimeDiff;
                            processingContext.MultiExtremeCounter[processingContext.Last] = default;
                        }
                    }
                }
                else
                {
                    this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeMultiModel, 0);
                    processingContext.GpsTimeIntegerCompressor.Compress(0, currentGpsTimeDiff, 7);
                    processingContext.MultiExtremeCounter[processingContext.Last]++;
                    if (processingContext.MultiExtremeCounter[processingContext.Last] > 3)
                    {
                        processingContext.LastGpsTimeDiff[processingContext.Last] = currentGpsTimeDiff;
                        processingContext.MultiExtremeCounter[processingContext.Last] = default;
                    }
                }
            }
            else
            {
                // the difference is huge
                // maybe the double belongs to another time sequence
                for (var i = 1U; i < 4U; i++)
                {
                    var otherGpsTimeDiff64 = BitConverter.UInt64BitsToInt64Bits(gpsTime) - BitConverter.UInt64BitsToInt64Bits(processingContext.LastGpsTime[(processingContext.Last + i) & 3]);
                    if (otherGpsTimeDiff64.IsInt32())
                    {
                        // it belongs to this sequence
                        this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeMultiModel, MultipleCodeFull + i);
                        processingContext.Last = (processingContext.Last + i) & 3;
                        this.WriteGpsTime(gpsTime);
                        return;
                    }
                }

                // no other sequence found. start new sequence.
                this.valueGpsTime.Encoder.EncodeSymbol(processingContext.GpsTimeMultiModel, MultipleCodeFull);
                processingContext.GpsTimeIntegerCompressor.Compress((int)(processingContext.LastGpsTime[processingContext.Last] >> 32), (int)(gpsTime >> 32), 8);
                this.valueGpsTime.Encoder.WriteInt((uint)gpsTime);
                processingContext.Next = (processingContext.Next + 1) & 3;
                processingContext.Last = processingContext.Next;
                processingContext.LastGpsTimeDiff[processingContext.Last] = default;
                processingContext.MultiExtremeCounter[processingContext.Last] = default;
            }

            processingContext.LastGpsTime[processingContext.Last] = gpsTime;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsShouldBePrivate", Justification = "This is used as an internal property bag.")]
    private sealed class Context
    {
        public readonly ulong[] LastGpsTime = new ulong[4];
        public readonly int[] LastGpsTimeDiff = new int[4];
        public readonly int[] MultiExtremeCounter = new int[4];

        public readonly byte[] LastItem = new byte[ArithmeticCoder.HalfModelCount];
        public readonly ushort[] LastIntensity = new ushort[8];
        public readonly StreamingMedian5[] LastXDiffMedian5 = new StreamingMedian5[12];
        public readonly StreamingMedian5[] LastYDiffMedian5 = new StreamingMedian5[12];
        public readonly int[] LastZ = new int[8];

        public readonly ISymbolModel[] ChangedValuesModels = new ISymbolModel[8];
        public readonly ISymbolModel?[] NumberOfReturnsModels = new ISymbolModel[16];
        public readonly ISymbolModel?[] ReturnNumberModels = new ISymbolModel[16];
        public readonly ISymbolModel?[] ClassificationModels = new ISymbolModel[64];
        public readonly ISymbolModel?[] FlagsModels = new ISymbolModel[64];
        public readonly ISymbolModel?[] UserDataModels = new ISymbolModel[64];

        public readonly ISymbolModel ScannerChannelModel;
        public readonly ISymbolModel ReturnNumberGpsSameModel;
        public readonly ISymbolModel GpsTimeMultiModel;
        public readonly ISymbolModel GpsTimeZeroDiffModel;

        public readonly IntegerCompressor DeltaXIntegerCompressor;
        public readonly IntegerCompressor DeltaYIntegerCompressor;
        public readonly IntegerCompressor ZIntegerCompressor;
        public readonly IntegerCompressor IntensityIntegerCompressor;
        public readonly IntegerCompressor ScanAngleIntegerCompressor;
        public readonly IntegerCompressor PointSourceIdIntegerCompressor;
        public readonly IntegerCompressor GpsTimeIntegerCompressor;

        // GPS time stuff
        public uint Last;
        public uint Next;

        public bool Unused = true;

        public Context(
            LayeredValue valueChannelReturnsXY,
            LayeredValue valueZ,
            LayeredValue valueIntensity,
            LayeredValue valueScanAngle,
            LayeredValue valuePointSource,
            LayeredValue valueGpsTime)
        {
            // for the XY layer
            this.ChangedValuesModels[0] = valueChannelReturnsXY.Encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[1] = valueChannelReturnsXY.Encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[2] = valueChannelReturnsXY.Encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[3] = valueChannelReturnsXY.Encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[4] = valueChannelReturnsXY.Encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[5] = valueChannelReturnsXY.Encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[6] = valueChannelReturnsXY.Encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[7] = valueChannelReturnsXY.Encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ScannerChannelModel = valueChannelReturnsXY.Encoder.CreateSymbolModel(3);

            this.ReturnNumberGpsSameModel = valueChannelReturnsXY.Encoder.CreateSymbolModel(13);

            this.DeltaXIntegerCompressor = new(valueChannelReturnsXY.Encoder, 32, 2);
            this.DeltaYIntegerCompressor = new(valueChannelReturnsXY.Encoder, 32, 22);

            // for the Z layer
            this.ZIntegerCompressor = new(valueZ.Encoder, 32, 20);

            // for the intensity layer
            this.IntensityIntegerCompressor = new(valueIntensity.Encoder, 16, 4);

            // for the scan angle layer
            this.ScanAngleIntegerCompressor = new(valueScanAngle.Encoder, 16, 2);

            // for the point source ID layer
            this.PointSourceIdIntegerCompressor = new(valuePointSource.Encoder);

            // for the GPS time layer
            this.GpsTimeMultiModel = valueGpsTime.Encoder.CreateSymbolModel(MultipleTotal);
            this.GpsTimeZeroDiffModel = valueGpsTime.Encoder.CreateSymbolModel(5);
            this.GpsTimeIntegerCompressor = new(valueGpsTime.Encoder, 32, 9);
        }
    }
}