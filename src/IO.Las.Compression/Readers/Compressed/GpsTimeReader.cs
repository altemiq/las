// -----------------------------------------------------------------------
// <copyright file="GpsTimeReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The gps time reader.
/// </summary>
/// <param name="decoder">The decoder.</param>
internal sealed class GpsTimeReader(IEntropyDecoder decoder) : ISimpleReader
{
    private const int Multiple = 500;

    private const int MultipleMinus = -10;

    private const int MultipleUnchanged = Multiple - MultipleMinus + 1;

    private const int MultipleCodeFull = Multiple - MultipleMinus + 2;

    private const int MultipleTotal = Multiple - MultipleMinus + 6;

    private readonly ulong[] lastGpsTime = new ulong[4];
    private readonly int[] lastGpsTimeDiff = new int[4];
    private readonly int[] multiExtremeCounter = new int[4];

    private readonly ISymbolModel gpsTimeMulti = decoder.CreateSymbolModel(MultipleTotal);
    private readonly ISymbolModel gpsTimeZeroDiff = decoder.CreateSymbolModel(6);
    private readonly IntegerDecompressor gpsTimeIntegerDecompressor = new(decoder, 32, 9);

    private uint last;
    private uint next;

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item)
    {
        this.last = this.next = default;
        Array.Clear(this.lastGpsTimeDiff, 0, 4);
        Array.Clear(this.multiExtremeCounter, 0, 4);

        // init models and integer compressors
        _ = this.gpsTimeMulti.Initialize();
        _ = this.gpsTimeZeroDiff.Initialize();
        this.gpsTimeIntegerDecompressor.Initialize();

        // init last item
        this.lastGpsTime[0] = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(item);
        Array.Clear(this.lastGpsTimeDiff, 1, 3);
        return true;
    }

    /// <inheritdoc/>
    public void Read(Span<byte> item)
    {
        // if the last integer difference was zero
        if (this.lastGpsTimeDiff[this.last] is 0)
        {
            var multi = (int)decoder.DecodeSymbol(this.gpsTimeZeroDiff);

            switch (multi)
            {
                // the difference can be represented with 32 bits
                case 1:
                    this.lastGpsTimeDiff[this.last] = this.gpsTimeIntegerDecompressor.Decompress(0);
                    this.lastGpsTime[this.last] = (ulong)((long)this.lastGpsTime[this.last] + this.lastGpsTimeDiff[this.last]);
                    this.multiExtremeCounter[this.last] = default;
                    break;

                // the difference is huge
                case 2:
                    this.next = (this.next + 1) & 3;
                    this.lastGpsTime[this.next] = (ulong)this.gpsTimeIntegerDecompressor.Decompress((int)(this.lastGpsTime[this.last] >> 32), 8);
                    this.lastGpsTime[this.next] <<= 32;
                    this.lastGpsTime[this.next] |= decoder.ReadUInt32();
                    this.last = this.next;
                    this.lastGpsTimeDiff[this.last] = default;
                    this.multiExtremeCounter[this.last] = default;
                    break;

                // we switch to another sequence
                case > 2:
                    this.last = (uint)((this.last + multi - 2) & 3);
                    this.Read(item);
                    break;
            }
        }
        else
        {
            var multi = (int)decoder.DecodeSymbol(this.gpsTimeMulti);
            switch (multi)
            {
                case 1:
                    this.lastGpsTime[this.last] = (ulong)((long)this.lastGpsTime[this.last] + this.gpsTimeIntegerDecompressor.Decompress(this.lastGpsTimeDiff[this.last], 1));
                    this.multiExtremeCounter[this.last] = default;
                    break;
                case < MultipleUnchanged:
                    int gpsTimeDiff;
                    switch (multi)
                    {
                        case 0:
                            gpsTimeDiff = this.gpsTimeIntegerDecompressor.Decompress(0, 7);
                            this.multiExtremeCounter[this.last]++;
                            if (this.multiExtremeCounter[this.last] > 3)
                            {
                                this.lastGpsTimeDiff[this.last] = gpsTimeDiff;
                                this.multiExtremeCounter[this.last] = default;
                            }

                            break;

                        case < Multiple:
                            var context = multi switch
                            {
                                < 10 => 2U,
                                _ => 3U,
                            };

                            gpsTimeDiff = this.gpsTimeIntegerDecompressor.Decompress(multi * this.lastGpsTimeDiff[this.last], context);

                            break;

                        case Multiple:
                            gpsTimeDiff = this.gpsTimeIntegerDecompressor.Decompress(Multiple * this.lastGpsTimeDiff[this.last], 4);
                            this.multiExtremeCounter[this.last]++;
                            if (this.multiExtremeCounter[this.last] > 3)
                            {
                                this.lastGpsTimeDiff[this.last] = gpsTimeDiff;
                                this.multiExtremeCounter[this.last] = default;
                            }

                            break;

                        default:
                            multi = Multiple - multi;
                            if (multi > MultipleMinus)
                            {
                                gpsTimeDiff = this.gpsTimeIntegerDecompressor.Decompress(multi * this.lastGpsTimeDiff[this.last], 5);
                            }
                            else
                            {
                                gpsTimeDiff = this.gpsTimeIntegerDecompressor.Decompress(MultipleMinus * this.lastGpsTimeDiff[this.last], 6);
                                this.multiExtremeCounter[this.last]++;
                                if (this.multiExtremeCounter[this.last] > 3)
                                {
                                    this.lastGpsTimeDiff[this.last] = gpsTimeDiff;
                                    this.multiExtremeCounter[this.last] = default;
                                }
                            }

                            break;
                    }

                    this.lastGpsTime[this.last] = (ulong)((long)this.lastGpsTime[this.last] + gpsTimeDiff);
                    break;

                case MultipleCodeFull:
                    this.next = (this.next + 1) & 3;
                    this.lastGpsTime[this.next] = (ulong)this.gpsTimeIntegerDecompressor.Decompress((int)(this.lastGpsTime[this.last] >> 32), 8);
                    this.lastGpsTime[this.next] <<= 32;
                    this.lastGpsTime[this.next] |= decoder.ReadUInt32();
                    this.last = this.next;
                    this.lastGpsTimeDiff[this.last] = default;
                    this.multiExtremeCounter[this.last] = default;
                    break;
                case >= MultipleCodeFull:
                    this.last = (uint)((this.last + multi - MultipleCodeFull) & 3);
                    this.Read(item);
                    break;
            }
        }

        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(item, this.lastGpsTime[this.last]);
    }
}