// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsPointDataRecordReader{T}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="IExtendedPointDataRecord"/> instances.
/// </summary>
/// <typeparam name="T">The type of extended point data record.</typeparam>
internal abstract class ExtendedGpsPointDataRecordReader<T> : IPointDataRecordReader, IContext
    where T : IExtendedPointDataRecord
{
    private const int Multiple = 500;

    private const int MultipleMinus = -10;

    private const int MultipleCodeFull = Multiple - MultipleMinus + 1;

    private const int MultipleTotal = Multiple - MultipleMinus + 5;

    private readonly IEntropyDecoder decoder;
    private readonly Context[] contexts = new Context[4];
    private readonly LayeredValue valueChannelReturnsXY;
    private readonly LayeredValue valueZ;
    private readonly LayeredValue valueClassification;
    private readonly LayeredValue valueFlags;
    private readonly LayeredValue valueIntensity;
    private readonly LayeredValue valueScanAngle;
    private readonly LayeredValue valueUserData;
    private readonly LayeredValue valuePointSource;
    private readonly LayeredValue valueGpsTime;

    private readonly byte[] data;
    private readonly int pointDataLength;
    private readonly int basePointDataLength;

    private byte[] bytes = [];
    private uint currentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedGpsPointDataRecordReader{T}"/> class.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    /// <param name="pointDataLength">The point data length.</param>
    /// <param name="basePointDataLength">The base point data length.</param>
    /// <param name="decompressSelective">The selective compress value.</param>
    protected ExtendedGpsPointDataRecordReader(IEntropyDecoder decoder, int pointDataLength, int basePointDataLength, DecompressSelections decompressSelective = DecompressSelections.All)
    {
        this.decoder = decoder;

        // zero instreams and decoders
        this.valueChannelReturnsXY = new(requested: true);
        this.valueZ = new(decompressSelective.HasFlag(DecompressSelections.Z));
        this.valueClassification = new(decompressSelective.HasFlag(DecompressSelections.Classification));
        this.valueFlags = new(decompressSelective.HasFlag(DecompressSelections.Flags));
        this.valueIntensity = new(decompressSelective.HasFlag(DecompressSelections.Intensity));
        this.valueScanAngle = new(decompressSelective.HasFlag(DecompressSelections.ScanAngle));
        this.valueUserData = new(decompressSelective.HasFlag(DecompressSelections.UserData));
        this.valuePointSource = new(decompressSelective.HasFlag(DecompressSelections.PointSource));
        this.valueGpsTime = new(decompressSelective.HasFlag(DecompressSelections.GpsTime));

        this.contexts[0] = new(this.valueChannelReturnsXY, this.valueZ, this.valueIntensity, this.valueScanAngle, this.valuePointSource, this.valueGpsTime);
        this.contexts[1] = new(this.valueChannelReturnsXY, this.valueZ, this.valueIntensity, this.valueScanAngle, this.valuePointSource, this.valueGpsTime);
        this.contexts[2] = new(this.valueChannelReturnsXY, this.valueZ, this.valueIntensity, this.valueScanAngle, this.valuePointSource, this.valueGpsTime);
        this.contexts[3] = new(this.valueChannelReturnsXY, this.valueZ, this.valueIntensity, this.valueScanAngle, this.valuePointSource, this.valueGpsTime);

        this.data = new byte[pointDataLength];
        this.pointDataLength = pointDataLength;
        this.basePointDataLength = basePointDataLength;
    }

    /// <inheritdoc/>
    public virtual bool ChunkSizes()
    {
        // for layered compression 'decoder' only hands over the stream
        var reader = this.decoder.GetStream();

        // read bytes per layer
        this.valueChannelReturnsXY.ByteCount = reader.ReadUInt32LittleEndian();
        this.valueZ.ByteCount = reader.ReadUInt32LittleEndian();
        this.valueClassification.ByteCount = reader.ReadUInt32LittleEndian();
        this.valueFlags.ByteCount = reader.ReadUInt32LittleEndian();
        this.valueIntensity.ByteCount = reader.ReadUInt32LittleEndian();
        this.valueScanAngle.ByteCount = reader.ReadUInt32LittleEndian();
        this.valueUserData.ByteCount = reader.ReadUInt32LittleEndian();
        this.valuePointSource.ByteCount = reader.ReadUInt32LittleEndian();
        this.valueGpsTime.ByteCount = reader.ReadUInt32LittleEndian();

        return true;
    }

    /// <inheritdoc/>
    LasPointSpan IPointDataRecordReader.Read(ReadOnlySpan<byte> source)
    {
        ReadOnlySpan<byte> processedData = this.ProcessData();
        return new(this.Read(processedData[..this.basePointDataLength]), processedData[this.basePointDataLength..]);
    }

    /// <inheritdoc/>
    async ValueTask<LasPointMemory> IPointDataRecordReader.ReadAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken)
    {
        ReadOnlyMemory<byte> processedData = await this.ProcessDataAsync(cancellationToken).ConfigureAwait(false);
        return new(await this.ReadAsync(processedData[..this.basePointDataLength], cancellationToken).ConfigureAwait(false), processedData[this.basePointDataLength..]);
    }

    /// <inheritdoc/>
    public virtual bool Initialize(ReadOnlySpan<byte> item, ref uint context) => this.Initialize(item, ref context, item.Length);

    /// <summary>
    /// Initializes the reader with the specified data.
    /// </summary>
    /// <param name="item">The item to initialize from.</param>
    /// <param name="context">The context.</param>
    /// <param name="gpsTimeChangeIndex">The GPS time change index.</param>
    /// <returns><see langword="true"/> if successfully initialized; otherwise <see langword="false"/>.</returns>
    protected virtual bool Initialize(ReadOnlySpan<byte> item, ref uint context, int gpsTimeChangeIndex)
    {
        // for layered compression 'decoder' only hands over the stream
        var stream = this.decoder.GetStream();

        // how many bytes do we need to read
        var byteCount = this.valueChannelReturnsXY.ByteCount
            + this.valueZ.GetByteCountIfRequested()
            + this.valueClassification.GetByteCountIfRequested()
            + this.valueFlags.GetByteCountIfRequested()
            + this.valueIntensity.GetByteCountIfRequested()
            + this.valueScanAngle.GetByteCountIfRequested()
            + this.valueUserData.GetByteCountIfRequested()
            + this.valuePointSource.GetByteCountIfRequested()
            + this.valueGpsTime.GetByteCountIfRequested();

        // make sure the buffer is sufficiently large
        if (byteCount > this.bytes.Length)
        {
            this.bytes = new byte[byteCount];
        }

        // load the requested bytes and Init the corresponding instreams and decoders
        var index = this.valueChannelReturnsXY.Initialize(stream, this.bytes);
        index += this.valueZ.InitializeIfRequested(stream, this.bytes, (int)index);
        index += this.valueClassification.InitializeIfRequested(stream, this.bytes, (int)index);
        index += this.valueFlags.InitializeIfRequested(stream, this.bytes, (int)index);
        index += this.valueIntensity.InitializeIfRequested(stream, this.bytes, (int)index);
        index += this.valueScanAngle.InitializeIfRequested(stream, this.bytes, (int)index);
        index += this.valueUserData.InitializeIfRequested(stream, this.bytes, (int)index);
        index += this.valuePointSource.InitializeIfRequested(stream, this.bytes, (int)index);
        _ = this.valueGpsTime.InitializeIfRequested(stream, this.bytes, (int)index);

        // mark the four scanner channel contexts as unused
        for (var c = 0; c < 4; c++)
        {
            this.contexts[c].Unused = true;
        }

        // set scanner channel as current context
        this.currentContext = new ExtendedGpsPointDataRecord(item).ScannerChannel;
        context = this.currentContext; // the POINT14 reader sets context for all other items

        // create and Init models and decompressors
        this.CreateAndInitModelsAndDecompressors(this.currentContext, item, gpsTimeChangeIndex);

        return true;
    }

    /// <inheritdoc cref="IPointDataRecordReader.Read"/>
    protected abstract T Read(ReadOnlySpan<byte> source);

    /// <inheritdoc cref="IPointDataRecordReader.ReadAsync"/>
    protected virtual ValueTask<T> ReadAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(this.Read(source.Span));
    }

    /// <summary>
    /// Processes the data.
    /// </summary>
    /// <returns>The processed data.</returns>
    protected virtual byte[] ProcessData()
    {
        var context = default(uint);
        return this.ProcessData(ref context);
    }

    /// <summary>
    /// Processes the data.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The processed data.</returns>
    protected virtual byte[] ProcessData(ref uint context) => this.ProcessData(ref context, this.pointDataLength);

    /// <summary>
    /// Processes the data.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gpsTimeChangeIndex">The GPS time change index.</param>
    /// <returns>The processed data.</returns>
    protected byte[] ProcessData(ref uint context, int gpsTimeChangeIndex)
    {
        var processingContext = this.contexts[this.currentContext];

        // get last
        var lastPoint = processingContext.LastPoint;

        ////////////////////////////////////////
        // decompress XY layer
        ////////////////////////////////////////

        // create single (3) / first (1) / last (2) / intermediate (0) context from last point return
        var lastPointReturn = GetLastPointReturn(lastPoint, lastPoint[gpsTimeChangeIndex]);

        // decompress which values have changed with last point return context
        var changedValues = (int)this.valueChannelReturnsXY.Decoder.DecodeSymbol(processingContext.ChangedValuesModels[lastPointReturn]);

        // if scanner channel has changed
        if ((changedValues & (1 << 6)) is not 0)
        {
            var scannerChannel = (this.currentContext + this.valueChannelReturnsXY.Decoder.DecodeSymbol(processingContext.ScannerChannelModel) + 1) % 4;

            // maybe create and init entropy models and integer compressors
            if (this.contexts[scannerChannel].Unused)
            {
                // create and init entropy models and integer decompressors
                this.CreateAndInitModelsAndDecompressors(scannerChannel, lastPoint, gpsTimeChangeIndex);

                // set the scanner channel
                FieldAccessors.ExtendedPointDataRecord.SetScannerChannel(ref this.contexts[scannerChannel].LastPoint[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset], (byte)scannerChannel);
            }

            // switch context to current scanner channel
            this.currentContext = scannerChannel;

            // the POINT14 reader sets context for all other items
            context = this.currentContext;

            // update the processing context
            processingContext = this.contexts[this.currentContext];

            // get last for new context
            lastPoint = processingContext.LastPoint;
        }

        // determine changed attributes
        var pointSourceChange = (changedValues & (1 << 5)) is not 0;
        var gpsTimeChange = (changedValues & (1 << 4)) is not 0;
        var scanAngleChange = (changedValues & (1 << 3)) is not 0;

        // get last return counts
        uint lastNumberOfReturns = FieldAccessors.ExtendedPointDataRecord.GetNumberOfReturns(lastPoint);
        uint lastReturnNumber = FieldAccessors.ExtendedPointDataRecord.GetReturnNumber(lastPoint);

        // if number of returns is different we decompress it
        uint numberOfReturns;
        if ((changedValues & (1 << 2)) is not 0)
        {
            var model = processingContext.NumberOfReturnsModels[lastNumberOfReturns];
            if (model is null)
            {
                model = this.valueChannelReturnsXY.Decoder.CreateSymbolModel(16);
                processingContext.NumberOfReturnsModels[lastNumberOfReturns] = model;
                _ = model.Initialize();
            }

            numberOfReturns = this.valueChannelReturnsXY.Decoder.DecodeSymbol(model);
            FieldAccessors.ExtendedPointDataRecord.SetNumberOfReturns(lastPoint, (byte)numberOfReturns);
        }
        else
        {
            numberOfReturns = lastNumberOfReturns;
        }

        // how is the return number different
        uint returnNumber;
        switch (changedValues & 3)
        {
            case 0:
                // same return number
                returnNumber = lastReturnNumber;
                break;
            case 1:
                // return number plus 1 mod 16
                returnNumber = (lastReturnNumber + 1) % 16;
                FieldAccessors.ExtendedPointDataRecord.SetReturnNumber(lastPoint, (byte)returnNumber);
                break;
            case 2:
                // return number minus 1 mod 16
                returnNumber = (lastReturnNumber + 15) % 16;
                FieldAccessors.ExtendedPointDataRecord.SetReturnNumber(lastPoint, (byte)returnNumber);
                break;
            default:
            {
                // the return number difference is bigger than +1 / -1 so we decompress how it is different
                if (gpsTimeChange)
                {
                    // if the GPS time has changed
                    var model = processingContext.ReturnNumberModels[lastReturnNumber];
                    if (model is null)
                    {
                        model = this.valueChannelReturnsXY.Decoder.CreateSymbolModel(16);
                        processingContext.ReturnNumberModels[lastReturnNumber] = model;
                        _ = model.Initialize();
                    }

                    returnNumber = this.valueChannelReturnsXY.Decoder.DecodeSymbol(model);
                }
                else
                {
                    // if the GPS time has not changed
                    var sym = this.valueChannelReturnsXY.Decoder.DecodeSymbol(processingContext.ReturnNumberGpsSameModel);
                    returnNumber = (lastReturnNumber + sym + 2) % 16;
                }

                FieldAccessors.ExtendedPointDataRecord.SetReturnNumber(lastPoint, (byte)returnNumber);
                break;
            }
        }

        // get return map and return level context for current point
        uint returnMap = Common.NumberReturnMap6Context[numberOfReturns][returnNumber];
        uint returnLevel = Common.NumberReturnLevel8Context[numberOfReturns][returnNumber];

        // create single (3) / first (1) / last (2) / intermediate (0) return context for current point
        var currentPointReturn = returnNumber is 1 ? 2 : 0; // first ?
        currentPointReturn += returnNumber >= numberOfReturns ? 1 : 0; // last ?

        // compute contexts
        var coordinateContext = numberOfReturns is 1 ? 1U : 0U;
        var gpsContext = gpsTimeChange ? 1U : 0U;
        var medianContext = (returnMap << 1) | gpsContext;

        // decompress X coordinate
        var median = processingContext.LastXDiffMedian5[medianContext].Get();
        var diff = processingContext.DeltaXIntegerDecompressor.Decompress(median, coordinateContext);
        FieldAccessors.ExtendedPointDataRecord.SetX(lastPoint, FieldAccessors.ExtendedPointDataRecord.GetX(lastPoint) + diff);
        processingContext.LastXDiffMedian5[medianContext].Add(diff);

        // decompress Y coordinate
        median = processingContext.LastYDiffMedian5[medianContext].Get();
        var kBits = processingContext.DeltaXIntegerDecompressor.K;
        diff = processingContext.DeltaYIntegerDecompressor.Decompress(median, coordinateContext + (kBits < 20U ? kBits.ZeroBit0() : 20U));
        FieldAccessors.ExtendedPointDataRecord.SetY(lastPoint, FieldAccessors.ExtendedPointDataRecord.GetY(lastPoint) + diff);
        processingContext.LastYDiffMedian5[medianContext].Add(diff);

        ////////////////////////////////////////
        // decompress Z layer (if changed and requested)
        ////////////////////////////////////////

        // if the Z coordinate should be decompressed and changes within this chunk
        if (this.valueZ.Changed)
        {
            kBits = (processingContext.DeltaXIntegerDecompressor.K + processingContext.DeltaYIntegerDecompressor.K) / 2;
            var z = processingContext.ZIntegerDecompressor.Decompress(processingContext.LastZ[returnLevel], coordinateContext + (kBits < 18U ? kBits.ZeroBit0() : 18U));
            FieldAccessors.ExtendedPointDataRecord.SetZ(lastPoint, z);
            processingContext.LastZ[returnLevel] = z;
        }

        ////////////////////////////////////////
        // decompress classifications layer (if changed and requested)
        ////////////////////////////////////////

        // if the classification should be decompressed and changes within this chunk
        if (this.valueClassification.Changed)
        {
            var lastClassification = (uint)lastPoint[Constants.ExtendedPointDataRecord.ClassificationFieldOffset];
            var classificationContext = (int)(((lastClassification & 0x1F) << 1) + (currentPointReturn is 3 ? 1U : 0U));
            var model = processingContext.ClassificationModels[classificationContext];
            if (model is null)
            {
                model = this.valueClassification.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                processingContext.ClassificationModels[classificationContext] = model;
                _ = model.Initialize();
            }

            lastPoint[Constants.ExtendedPointDataRecord.ClassificationFieldOffset] = (byte)this.valueClassification.Decoder.DecodeSymbol(model);
        }

        ////////////////////////////////////////
        // decompress flags layer (if changed and requested)
        ////////////////////////////////////////

        // if the flags should be decompressed and change within this chunk
        if (this.valueFlags.Changed)
        {
            var flagValue = lastPoint[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset];
            var lastFlags = ((FieldAccessors.ExtendedPointDataRecord.GetSynthetic(flagValue) ? 1 : 0) << 0)
                | ((FieldAccessors.ExtendedPointDataRecord.GetKeyPoint(flagValue) ? 1 : 0) << 1)
                | ((FieldAccessors.ExtendedPointDataRecord.GetWithheld(flagValue) ? 1 : 0) << 2)
                | ((FieldAccessors.ExtendedPointDataRecord.GetOverlap(flagValue) ? 1 : 0) << 3)
                | ((FieldAccessors.ExtendedPointDataRecord.GetScanDirectionFlag(flagValue) ? 1 : 0) << 4)
                | ((FieldAccessors.ExtendedPointDataRecord.GetEdgeOfFlightLine(flagValue) ? 1 : 0) << 5);
            var model = processingContext.FlagsModels[lastFlags];
            if (model is null)
            {
                model = this.valueFlags.Decoder.CreateSymbolModel(64);
                processingContext.FlagsModels[lastFlags] = model;
                _ = model.Initialize();
            }

            var flags = this.valueFlags.Decoder.DecodeSymbol(model);
            FieldAccessors.ExtendedPointDataRecord.SetSynthetic(ref flagValue, (flags & (1 << 0)) is not 0U);
            FieldAccessors.ExtendedPointDataRecord.SetKeyPoint(ref flagValue, (flags & (1 << 1)) is not 0U);
            FieldAccessors.ExtendedPointDataRecord.SetWithheld(ref flagValue, (flags & (1 << 2)) is not 0U);
            FieldAccessors.ExtendedPointDataRecord.SetOverlap(ref flagValue, (flags & (1 << 3)) is not 0U);
            FieldAccessors.ExtendedPointDataRecord.SetScanDirectionFlag(ref flagValue, (flags & (1 << 4)) is not 0U);
            FieldAccessors.ExtendedPointDataRecord.SetEdgeOfFlightLine(ref flagValue, (flags & (1 << 5)) is not 0U);
            lastPoint[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset] = flagValue;
        }

        ////////////////////////////////////////
        // decompress intensity layer (if changed and requested)
        ////////////////////////////////////////

        // if the intensity should be decompressed and changes within this chunk
        if (this.valueIntensity.Changed)
        {
            var intensityContext = (currentPointReturn << 1) | (gpsTimeChange ? 1 : 0);
            var intensity = (ushort)processingContext.IntensityIntegerDecompressor.Decompress(processingContext.LastIntensity[intensityContext], (uint)currentPointReturn);
            processingContext.LastIntensity[intensityContext] = intensity;
            FieldAccessors.ExtendedPointDataRecord.SetIntensity(lastPoint, intensity);
        }

        ////////////////////////////////////////
        // decompress scan angle layer (if changed and requested)
        ////////////////////////////////////////

        // if the scan angle should be decompressed and changes within this chunk
        if (this.valueScanAngle.Changed && scanAngleChange)
        {
            FieldAccessors.ExtendedPointDataRecord.SetScanAngle(lastPoint, (short)processingContext.ScanAngleIntegerDecompressor.Decompress(FieldAccessors.ExtendedPointDataRecord.GetScanAngle(lastPoint), gpsContext));
        }

        ////////////////////////////////////////
        // decompress user data layer (if changed and requested)
        ////////////////////////////////////////

        // if the user data should be decompressed and changes within this chunk
        if (this.valueUserData.Changed)
        {
            var userDataContext = lastPoint[Constants.ExtendedPointDataRecord.UserDataFieldOffset] / 4;
            var model = processingContext.UserDataModels[userDataContext];
            if (model is null)
            {
                model = this.valueUserData.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                processingContext.UserDataModels[userDataContext] = model;
                _ = model.Initialize();
            }

            lastPoint[Constants.ExtendedPointDataRecord.UserDataFieldOffset] = (byte)this.valueUserData.Decoder.DecodeSymbol(model);
        }

        ////////////////////////////////////////
        // decompress point source layer (if changed and requested)
        ////////////////////////////////////////

        // if the point source ID should be decompressed and changes within this chunk
        if (this.valuePointSource.Changed && pointSourceChange)
        {
            FieldAccessors.ExtendedPointDataRecord.SetPointSourceId(lastPoint, (ushort)processingContext.PointSourceIdIntegerDecompressor.Decompress(FieldAccessors.ExtendedPointDataRecord.GetPointSourceId(lastPoint)));
        }

        ////////////////////////////////////////
        // decompress GPS time layer (if changed and requested)
        ////////////////////////////////////////

        // if the GPS time should be decompressed and changes within this chunk
        if (this.valueGpsTime.Changed && gpsTimeChange)
        {
            this.ReadGpsTime();
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(lastPoint.AsSpan(Constants.ExtendedPointDataRecord.GpsTimeFieldOffset), BitConverter.UInt64BitsToDouble(processingContext.LastGpsTime[processingContext.Last]));
        }

        // copy the last item
        Array.Copy(lastPoint, this.data, ExtendedGpsPointDataRecord.Size);

        // remember if the last point had a GPS time change
        lastPoint[gpsTimeChangeIndex] = gpsTimeChange ? (byte)0x01 : (byte)0x00;
        return this.data;

        static int GetLastPointReturn(ReadOnlySpan<byte> lastPoint, byte gpsTimeChange)
        {
            var returnNumber = FieldAccessors.ExtendedPointDataRecord.GetReturnNumber(lastPoint);

            // whether this is the first return
            var first = returnNumber is 1 ? 1 : 0;

            // whether this is the last return
            var last = returnNumber >= FieldAccessors.ExtendedPointDataRecord.GetNumberOfReturns(lastPoint) ? 2 : 0;

            // whether the GPS time changed in the last return to the context
            var gpsTime = gpsTimeChange is 0 ? 0 : 4;

            return first + last + gpsTime;
        }
    }

    /// <summary>
    /// Processes the data.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The processed data.</returns>
    protected virtual ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        var context = default(uint);
        return this.ProcessDataAsync(ref context, cancellationToken);
    }

    /// <summary>
    /// Processes the data.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The processed data.</returns>
    protected ValueTask<Memory<byte>> ProcessDataAsync(ref uint context, CancellationToken cancellationToken = default) => this.ProcessDataAsync(ref context, this.pointDataLength, cancellationToken);

    /// <summary>
    /// Processes the data.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gpsTimeChangeIndex">The GPS time change index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The processed data.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method", Justification = "This would cause recursion.")]
    protected ValueTask<Memory<byte>> ProcessDataAsync(ref uint context, int gpsTimeChangeIndex, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(this.ProcessData(ref context, gpsTimeChangeIndex));
    }

    private void CreateAndInitModelsAndDecompressors(uint context, ReadOnlySpan<byte> item, int gpsTimeChangeIndex)
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
        contextToInitialize.DeltaXIntegerDecompressor.Initialize();
        contextToInitialize.DeltaYIntegerDecompressor.Initialize();
        for (var i = 0; i < 12; i++)
        {
            contextToInitialize.LastXDiffMedian5[i] = new();
            contextToInitialize.LastYDiffMedian5[i] = new();
        }

        // for the Z layer
        contextToInitialize.ZIntegerDecompressor.Initialize();
        for (var i = 0; i < 8; i++)
        {
            contextToInitialize.LastZ[i] = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(item[8..12]);
        }

        // for the classification, flags, and user data layers
        for (var i = 0; i < 64; i++)
        {
            _ = contextToInitialize.ClassificationModels[i]?.Initialize();
            _ = contextToInitialize.FlagsModels[i]?.Initialize();
            _ = contextToInitialize.UserDataModels[i]?.Initialize();
        }

        // for the intensity layer
        contextToInitialize.IntensityIntegerDecompressor.Initialize();
        for (var i = 0; i < 8; i++)
        {
            contextToInitialize.LastIntensity[i] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[12..14]);
        }

        // for the scan angle layer
        contextToInitialize.ScanAngleIntegerDecompressor.Initialize();

        // for the point source ID layer
        contextToInitialize.PointSourceIdIntegerDecompressor.Initialize();

        // for the GPS time layer
        _ = contextToInitialize.GpsTimeMultiModel.Initialize();
        _ = contextToInitialize.GpsTimeZeroDiffModel.Initialize();
        contextToInitialize.GpsTimeIntegerDecompressor.Initialize();
        contextToInitialize.Last = default;
        contextToInitialize.Next = default;
        Array.Clear(contextToInitialize.LastGpsTimeDiff, 0, 4);
        Array.Clear(contextToInitialize.MultiExtremeCounter, 0, 4);
        contextToInitialize.LastGpsTime[0] = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(item[(ExtendedGpsPointDataRecord.Size - sizeof(double))..]);
        Array.Clear(contextToInitialize.LastGpsTime, 1, 3);

        // init current context from last item
        var lastItem = contextToInitialize.LastPoint;
        item[..ExtendedGpsPointDataRecord.Size].CopyTo(lastItem);
        lastItem[gpsTimeChangeIndex] = default;

        contextToInitialize.Unused = false;
    }

    private void ReadGpsTime()
    {
        while (true)
        {
            var processingContext = this.contexts[this.currentContext];

            // if the last integer difference was zero
            if (processingContext.LastGpsTimeDiff[processingContext.Last] is 0)
            {
                var multi = (int)this.valueGpsTime.Decoder.DecodeSymbol(processingContext.GpsTimeZeroDiffModel);

                switch (multi)
                {
                    // the difference can be represented with 32 bits
                    case 0:
                    {
                        processingContext.LastGpsTimeDiff[processingContext.Last] = processingContext.GpsTimeIntegerDecompressor.Decompress(0);
                        var value = BitConverter.UInt64BitsToInt64Bits(processingContext.LastGpsTime[processingContext.Last]) + processingContext.LastGpsTimeDiff[processingContext.Last];
                        processingContext.LastGpsTime[processingContext.Last] = BitConverter.Int64BitsToUInt64Bits(value);
                        break;
                    }

                    // the difference is huge
                    case 1:
                        processingContext.Next = (processingContext.Next + 1) & 3;
                        processingContext.LastGpsTime[processingContext.Next] = (ulong)processingContext.GpsTimeIntegerDecompressor.Decompress((int)(processingContext.LastGpsTime[processingContext.Last] >> 32), 8);
                        processingContext.LastGpsTime[processingContext.Next] <<= 32;
                        processingContext.LastGpsTime[processingContext.Next] |= this.valueGpsTime.Decoder.ReadUInt32();
                        processingContext.Last = processingContext.Next;
                        processingContext.LastGpsTimeDiff[processingContext.Last] = default;
                        break;

                    // we switch to another sequence
                    default:
                        processingContext.Last = (uint)(processingContext.Last + multi - 1) & 3;
                        continue;
                }

                processingContext.MultiExtremeCounter[processingContext.Last] = default;
            }
            else
            {
                var multi = (int)this.valueGpsTime.Decoder.DecodeSymbol(processingContext.GpsTimeMultiModel);
                switch (multi)
                {
                    case 1:
                    {
                        var value = BitConverter.UInt64BitsToInt64Bits(processingContext.LastGpsTime[processingContext.Last]) + processingContext.GpsTimeIntegerDecompressor.Decompress(processingContext.LastGpsTimeDiff[processingContext.Last], 1);
                        processingContext.LastGpsTime[processingContext.Last] = BitConverter.Int64BitsToUInt64Bits(value);
                        processingContext.MultiExtremeCounter[processingContext.Last] = default;
                        break;
                    }

                    case < MultipleCodeFull:
                    {
                        int gpsTimeDiff;
                        switch (multi)
                        {
                            case 0:
                            {
                                gpsTimeDiff = processingContext.GpsTimeIntegerDecompressor.Decompress(0, 7);
                                processingContext.MultiExtremeCounter[processingContext.Last]++;
                                if (processingContext.MultiExtremeCounter[processingContext.Last] > 3)
                                {
                                    processingContext.LastGpsTimeDiff[processingContext.Last] = gpsTimeDiff;
                                    processingContext.MultiExtremeCounter[processingContext.Last] = default;
                                }

                                break;
                            }

                            case < Multiple:
                                gpsTimeDiff = processingContext.GpsTimeIntegerDecompressor.Decompress(multi * processingContext.LastGpsTimeDiff[processingContext.Last], multi < 10 ? 2U : 3U);
                                break;
                            case Multiple:
                            {
                                gpsTimeDiff = processingContext.GpsTimeIntegerDecompressor.Decompress(Multiple * processingContext.LastGpsTimeDiff[processingContext.Last], 4);
                                processingContext.MultiExtremeCounter[processingContext.Last]++;
                                if (processingContext.MultiExtremeCounter[processingContext.Last] > 3)
                                {
                                    processingContext.LastGpsTimeDiff[processingContext.Last] = gpsTimeDiff;
                                    processingContext.MultiExtremeCounter[processingContext.Last] = default;
                                }

                                break;
                            }

                            default:
                            {
                                multi = Multiple - multi;
                                if (multi > MultipleMinus)
                                {
                                    gpsTimeDiff = processingContext.GpsTimeIntegerDecompressor.Decompress(multi * processingContext.LastGpsTimeDiff[processingContext.Last], 5);
                                }
                                else
                                {
                                    gpsTimeDiff = processingContext.GpsTimeIntegerDecompressor.Decompress(MultipleMinus * processingContext.LastGpsTimeDiff[processingContext.Last], 6);
                                    processingContext.MultiExtremeCounter[processingContext.Last]++;
                                    if (processingContext.MultiExtremeCounter[processingContext.Last] > 3)
                                    {
                                        processingContext.LastGpsTimeDiff[processingContext.Last] = gpsTimeDiff;
                                        processingContext.MultiExtremeCounter[processingContext.Last] = default;
                                    }
                                }

                                break;
                            }
                        }

                        var value = BitConverter.UInt64BitsToInt64Bits(processingContext.LastGpsTime[processingContext.Last]) + gpsTimeDiff;
                        processingContext.LastGpsTime[processingContext.Last] = BitConverter.Int64BitsToUInt64Bits(value);
                        break;
                    }

                    case MultipleCodeFull:
                        processingContext.Next = (processingContext.Next + 1) & 3;
                        processingContext.LastGpsTime[processingContext.Next] = (ulong)processingContext.GpsTimeIntegerDecompressor.Decompress((int)(processingContext.LastGpsTime[processingContext.Last] >> 32), 8);
                        processingContext.LastGpsTime[processingContext.Next] <<= 32;
                        processingContext.LastGpsTime[processingContext.Next] |= this.valueGpsTime.Decoder.ReadUInt32();
                        processingContext.Last = processingContext.Next;
                        processingContext.LastGpsTimeDiff[processingContext.Last] = default;
                        processingContext.MultiExtremeCounter[processingContext.Last] = default;
                        break;
                    default:
                        processingContext.Last = (uint)(processingContext.Last + multi - MultipleCodeFull) & 3;
                        continue;
                }
            }

            break;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsShouldBePrivate", Justification = "This is used as an internal property bag.")]
    private sealed class Context
    {
        public readonly byte[] LastPoint = new byte[ArithmeticCoder.HalfModelCount];
        public readonly ushort[] LastIntensity = new ushort[8];
        public readonly StreamingMedian5[] LastXDiffMedian5 = new StreamingMedian5[12];
        public readonly StreamingMedian5[] LastYDiffMedian5 = new StreamingMedian5[12];
        public readonly int[] LastZ = new int[8];
        public readonly ulong[] LastGpsTime = new ulong[4];
        public readonly int[] LastGpsTimeDiff = new int[4];
        public readonly int[] MultiExtremeCounter = new int[4];

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

        public readonly IntegerDecompressor DeltaXIntegerDecompressor;
        public readonly IntegerDecompressor DeltaYIntegerDecompressor;
        public readonly IntegerDecompressor ZIntegerDecompressor;
        public readonly IntegerDecompressor IntensityIntegerDecompressor;
        public readonly IntegerDecompressor ScanAngleIntegerDecompressor;
        public readonly IntegerDecompressor PointSourceIdIntegerDecompressor;
        public readonly IntegerDecompressor GpsTimeIntegerDecompressor;

        public bool Unused;

        public uint Last;
        public uint Next;

        public Context(LayeredValue channelReturnsXY, LayeredValue z, LayeredValue intensity, LayeredValue scanAngle, LayeredValue pointSource, LayeredValue gpsTime)
        {
            this.ChangedValuesModels[0] = channelReturnsXY.Decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[1] = channelReturnsXY.Decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[2] = channelReturnsXY.Decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[3] = channelReturnsXY.Decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[4] = channelReturnsXY.Decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[5] = channelReturnsXY.Decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[6] = channelReturnsXY.Decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ChangedValuesModels[7] = channelReturnsXY.Decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
            this.ScannerChannelModel = channelReturnsXY.Decoder.CreateSymbolModel(3);

            this.ReturnNumberGpsSameModel = channelReturnsXY.Decoder.CreateSymbolModel(13);

            this.DeltaXIntegerDecompressor = new(channelReturnsXY.Decoder, 32, 2);
            this.DeltaYIntegerDecompressor = new(channelReturnsXY.Decoder, 32, 22);

            // for the Z layer
            this.ZIntegerDecompressor = new(z.Decoder, 32, 20);

            // for the intensity layer
            this.IntensityIntegerDecompressor = new(intensity.Decoder, 16, 4);

            // for the scan angle layer
            this.ScanAngleIntegerDecompressor = new(scanAngle.Decoder, 16, 2);

            // for the point source ID layer
            this.PointSourceIdIntegerDecompressor = new(pointSource.Decoder);

            // for the GPS time layer
            this.GpsTimeMultiModel = gpsTime.Decoder.CreateSymbolModel(MultipleTotal);
            this.GpsTimeZeroDiffModel = gpsTime.Decoder.CreateSymbolModel(5);
            this.GpsTimeIntegerDecompressor = new(gpsTime.Decoder, 32, 9);
        }
    }
}