// -----------------------------------------------------------------------
// <copyright file="GpsTimeWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed writer for GPS time values, version 2.
/// </summary>
internal sealed class GpsTimeWriter : ISimpleWriter
{
    private const int Multiple = 500;

    private const int MultipleMinus = -10;

    private const int MultipleUnchanged = Multiple - MultipleMinus + 1;

    private const int MultipleCodeFull = Multiple - MultipleMinus + 2;

    private const int MultipleTotal = Multiple - MultipleMinus + 6;

    private readonly IEntropyEncoder encoder;

    private readonly ulong[] lastGpsTime = new ulong[4];

    private readonly int[] lastGpsTimeDiff = new int[4];

    private readonly int[] multiExtremeCounter = new int[4];

    private readonly ISymbolModel gpsTimeMultiModel;

    private readonly ISymbolModel gpsTimeZeroDiffModel;

    private readonly IntegerCompressor gpsTimeIntegerCompressor;

    private uint last;

    private uint next;

    /// <summary>
    /// Initializes a new instance of the <see cref="GpsTimeWriter"/> class.
    /// </summary>
    /// <param name="encoder">The encoder.</param>
    public GpsTimeWriter(IEntropyEncoder encoder)
    {
        this.encoder = encoder;

        // create models and integer compressors
        this.gpsTimeMultiModel = this.encoder.CreateSymbolModel(MultipleTotal);
        this.gpsTimeZeroDiffModel = this.encoder.CreateSymbolModel(6);
        this.gpsTimeIntegerCompressor = new(this.encoder, 32, 9);
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item)
    {
        // init state
        this.last = default;
        this.next = default;
        Array.Clear(this.lastGpsTimeDiff, 0, 4);
        Array.Clear(this.multiExtremeCounter, 0, 4);

        // init models and integer compressors
        _ = this.gpsTimeMultiModel.Initialize();
        _ = this.gpsTimeZeroDiffModel.Initialize();
        this.gpsTimeIntegerCompressor.Initialize();

        this.lastGpsTime[0] = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(item);
        Array.Clear(this.lastGpsTime, 1, 3);

        return true;
    }

    /// <inheritdoc/>
    public void Write(Span<byte> item)
    {
        var gpsTime = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(item);

        if (this.lastGpsTimeDiff[this.last] is 0)
        {
            // if the last integer difference was zero
            if (BitConverter.UInt64BitsToInt64Bits(gpsTime) == BitConverter.UInt64BitsToInt64Bits(this.lastGpsTime[this.last]))
            {
                // the doubles have not changed
                this.encoder.EncodeSymbol(this.gpsTimeZeroDiffModel, 0);
            }
            else
            {
                // calculate the difference between the two doubles as an integer
                var currentGpsTimeDiff64 = BitConverter.UInt64BitsToInt64Bits(gpsTime) - BitConverter.UInt64BitsToInt64Bits(this.lastGpsTime[this.last]);
                if (currentGpsTimeDiff64.IsInt32())
                {
                    // the difference can be represented with 32 bits
                    var currentGpsTimeDiff = (int)currentGpsTimeDiff64;
                    this.encoder.EncodeSymbol(this.gpsTimeZeroDiffModel, 1);
                    this.gpsTimeIntegerCompressor.Compress(0, currentGpsTimeDiff);
                    this.lastGpsTimeDiff[this.last] = currentGpsTimeDiff;
                    this.multiExtremeCounter[this.last] = default;
                }
                else
                {
                    // the difference is huge
                    // maybe the double belongs to another time sequence
                    for (var i = 1U; i < 4U; i++)
                    {
                        var otherGpsTimeDiff64 = BitConverter.UInt64BitsToInt64Bits(gpsTime) - BitConverter.UInt64BitsToInt64Bits(this.lastGpsTime[(this.last + i) & 3]);
                        if (!otherGpsTimeDiff64.IsInt32())
                        {
                            continue;
                        }

                        // it belongs to another sequence
                        this.encoder.EncodeSymbol(this.gpsTimeZeroDiffModel, i + 2);
                        this.last = (this.last + i) & 3;
                        this.Write(item);
                        return;
                    }

                    // no other sequence found. start new sequence.
                    this.encoder.EncodeSymbol(this.gpsTimeZeroDiffModel, 2);
                    this.gpsTimeIntegerCompressor.Compress((int)(this.lastGpsTime[this.last] >> 32), (int)(gpsTime >> 32), 8);
                    this.encoder.WriteInt((uint)gpsTime);
                    this.next = (this.next + 1) & 3;
                    this.last = this.next;
                    this.lastGpsTimeDiff[this.last] = default;
                    this.multiExtremeCounter[this.last] = default;
                }

                this.lastGpsTime[this.last] = gpsTime;
            }
        }
        else
        {
            // the last integer difference was *not* zero
            if (BitConverter.UInt64BitsToInt64Bits(gpsTime) == BitConverter.UInt64BitsToInt64Bits(this.lastGpsTime[this.last]))
            {
                // if the doubles have not changed use a special symbol
                this.encoder.EncodeSymbol(this.gpsTimeMultiModel, MultipleUnchanged);
            }
            else
            {
                // calculate the difference between the two doubles as an integer
                var currentGpsTimeDiff64 = BitConverter.UInt64BitsToInt64Bits(gpsTime) - BitConverter.UInt64BitsToInt64Bits(this.lastGpsTime[this.last]);
                var currentGpsTimeDiff = (int)currentGpsTimeDiff64;

                // if the current GPS time difference can be represented with 32 bits
                if (currentGpsTimeDiff64 == currentGpsTimeDiff)
                {
                    // compute multiplier between current and last integer difference
                    var multi = (currentGpsTimeDiff / (float)this.lastGpsTimeDiff[this.last]).Quantize();

                    switch (multi)
                    {
                        // compress the residual current GPS time difference in dependence on the multiplier
                        case 1:
                            // this is the case we assume we get most often for regular spaced pulses
                            this.encoder.EncodeSymbol(this.gpsTimeMultiModel, 1);
                            this.gpsTimeIntegerCompressor.Compress(this.lastGpsTimeDiff[this.last], currentGpsTimeDiff, 1);
                            this.multiExtremeCounter[this.last] = default;
                            break;
                        case > 0 and < Multiple:
                            // positive multipliers up to LASZIPGPSTIMEMULTI are compressed directly
                            this.encoder.EncodeSymbol(this.gpsTimeMultiModel, (uint)multi);
                            this.gpsTimeIntegerCompressor.Compress(multi * this.lastGpsTimeDiff[this.last], currentGpsTimeDiff, multi < 10 ? 2U : 3U);
                            break;
                        case > 0:
                        {
                            this.encoder.EncodeSymbol(this.gpsTimeMultiModel, Multiple);
                            this.gpsTimeIntegerCompressor.Compress(Multiple * this.lastGpsTimeDiff[this.last], currentGpsTimeDiff, 4);
                            this.multiExtremeCounter[this.last]++;
                            if (this.multiExtremeCounter[this.last] > 3)
                            {
                                this.lastGpsTimeDiff[this.last] = currentGpsTimeDiff;
                                this.multiExtremeCounter[this.last] = default;
                            }

                            break;
                        }

                        case < 0 and > MultipleMinus:
                            // negative multipliers larger than LASZIPGPSTIMEMULTIMINUS are compressed directly
                            this.encoder.EncodeSymbol(this.gpsTimeMultiModel, (uint)(Multiple - multi));
                            this.gpsTimeIntegerCompressor.Compress(multi * this.lastGpsTimeDiff[this.last], currentGpsTimeDiff, 5);
                            break;
                        case < 0:
                        {
                            this.encoder.EncodeSymbol(this.gpsTimeMultiModel, Multiple - MultipleMinus);
                            this.gpsTimeIntegerCompressor.Compress(MultipleMinus * this.lastGpsTimeDiff[this.last], currentGpsTimeDiff, 6);
                            this.multiExtremeCounter[this.last]++;
                            if (this.multiExtremeCounter[this.last] > 3)
                            {
                                this.lastGpsTimeDiff[this.last] = currentGpsTimeDiff;
                                this.multiExtremeCounter[this.last] = default;
                            }

                            break;
                        }

                        default:
                        {
                            this.encoder.EncodeSymbol(this.gpsTimeMultiModel, 0);
                            this.gpsTimeIntegerCompressor.Compress(0, currentGpsTimeDiff, 7);
                            this.multiExtremeCounter[this.last]++;
                            if (this.multiExtremeCounter[this.last] > 3)
                            {
                                this.lastGpsTimeDiff[this.last] = currentGpsTimeDiff;
                                this.multiExtremeCounter[this.last] = default;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    // the difference is huge
                    // maybe the double belongs to another time sequence
                    for (var i = 1U; i < 4U; i++)
                    {
                        var otherGpsTimeDiff64 = BitConverter.UInt64BitsToInt64Bits(gpsTime) - BitConverter.UInt64BitsToInt64Bits(this.lastGpsTime[(this.last + i) & 3]);
                        var otherGpsTimeDiff = (int)otherGpsTimeDiff64;
                        if (otherGpsTimeDiff64 != otherGpsTimeDiff)
                        {
                            continue;
                        }

                        // it belongs to this sequence
                        this.encoder.EncodeSymbol(this.gpsTimeMultiModel, MultipleCodeFull + i);
                        this.last = (this.last + i) & 3;
                        this.Write(item);
                        return;
                    }

                    // no other sequence found. start new sequence.
                    this.encoder.EncodeSymbol(this.gpsTimeMultiModel, MultipleCodeFull);
                    this.gpsTimeIntegerCompressor.Compress((int)(this.lastGpsTime[this.last] >> 32), (int)(gpsTime >> 32), 8);
                    this.encoder.WriteInt((uint)gpsTime);
                    this.next = (this.next + 1) & 3;
                    this.last = this.next;
                    this.lastGpsTimeDiff[this.last] = default;
                    this.multiExtremeCounter[this.last] = default;
                }

                this.lastGpsTime[this.last] = gpsTime;
            }
        }
    }

    /// <inheritdoc/>
    public ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.Write(item.Span);
        return default;
    }
}