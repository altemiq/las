// -----------------------------------------------------------------------
// <copyright file="ArithmeticDecoder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The arithmetic decoder.
/// </summary>
internal sealed class ArithmeticDecoder : ArithmeticCoder
{
    private Stream? inputStream;

    private uint value;

    private uint length;

    /// <summary>
    /// Initializes this instance against the specified input <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to decode from.</param>
    /// <param name="reallyInit">Set to <see langword="true"/> to prime the decoder with the first 4 bytes.</param>
    /// <returns><see langword="true"/> if initialized, or <see langword="false"/> if <paramref name="stream"/> was <see langword="null"/>.</returns>
    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(inputStream))]
    public bool Initialize(Stream? stream, bool reallyInit = true)
    {
        if (stream is null)
        {
            return false;
        }

        this.inputStream = stream;
        this.length = MaxLength;
        if (!reallyInit)
        {
            return true;
        }

        var tempValue = stream.ReadByteLittleEndian() << 24;
        tempValue |= stream.ReadByteLittleEndian() << 16;
        tempValue |= stream.ReadByteLittleEndian() << 8;
        tempValue |= stream.ReadByteLittleEndian();
        this.value = (uint)tempValue;

        return true;
    }

    /// <inheritdoc />
    public override void Done() => this.inputStream = null;

    /// <summary>
    /// Decodes a bit with modelling.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The decoded bit.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public uint DecodeBit(ArithmeticBitModel model)
    {
        const int BitLengthShift = ArithmeticBitModel.LengthShift;

        // product l x p0
        var x = model.BitZeroProb * (this.length >> BitLengthShift);

        // decision
        var sym = (this.value >= x) ? 1U : 0U;

        // update & shift interval
        if (sym is 0)
        {
            this.length = x;
            model.IncrementBitZeroCount();
        }
        else
        {
            // shifted interval base = 0
            this.value -= x;
            this.length -= x;
        }

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormalizeDecimalInterval();
        }

        if (model.DecrementBitsUntilUpdate() is 0)
        {
            // periodic model update
            model.Update();
        }

        // return data bit value
        return sym;
    }

    /// <summary>
    /// Decodes a symbol with modelling.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The decoded symbol.</returns>
    /// <exception cref="CompressionNotInitializedException">Compression has not been initialized.</exception>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public uint DecodeSymbol(ArithmeticSymbolModel model)
    {
        if (!model.Initialized)
        {
            throw new CompressionNotInitializedException();
        }

        const int SymbolLengthShift = ArithmeticSymbolModel.LengthShift;
        var symbol = default(uint);
        var x = default(uint);
        var y = this.length;

        var distribution = model.Distribution;
        var decoderTable = model.DecoderTable;
        this.length >>= SymbolLengthShift;
        if (decoderTable is null)
        {
            // decode using only multiplications
            var n = model.Symbols;
            var k = n >> 1;

            // decode via bisection search
            do
            {
                var z = this.length * distribution[k];
                if (z > this.value)
                {
                    // value is smaller
                    n = k;
                    y = z;
                }
                else
                {
                    // value is larger or equal
                    symbol = k;
                    x = z;
                }
            }
            while ((k = (symbol + n) >> 1) != symbol);
        }
        else
        {
            // use table look-up for faster decoding
            var dv = this.value / this.length;
            var t = dv >> (int)model.TableShift;

            // initial decision based on table look-up
            symbol = decoderTable[t];
            var n = decoderTable[t + 1] + 1;

            // finish with bisection search
            while (n > symbol + 1)
            {
                var k = (symbol + n) >> 1;
                if (distribution[k] > dv)
                {
                    n = k;
                }
                else
                {
                    symbol = k;
                }
            }

            // compute products
            x = distribution[symbol] * this.length;
            if (symbol != model.LastSymbol)
            {
                y = distribution[symbol + 1] * this.length;
            }
        }

        // update interval
        this.value -= x;
        this.length = y - x;

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormalizeDecimalInterval();
        }

        ++model.SymbolCount[symbol];

        // periodic model update
        if (model.DecrementSymbolsUntilUpdate() is 0)
        {
            model.Update();
        }

        return symbol;
    }

    /// <summary>
    /// Decodes the next bit without using a model.
    /// </summary>
    /// <returns>The decoded bit (0 or 1).</returns>
    public uint ReadBit()
    {
        // decode symbol, change length
        this.length >>= 1;
        var sym = this.value / this.length;

        // update interval
        this.value -= this.length * sym;

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormalizeDecimalInterval();
        }

        return sym;
    }

    /// <summary>
    /// Decodes the next <paramref name="bits"/> bits without using a model.
    /// </summary>
    /// <param name="bits">The number of bits to read (1-32).</param>
    /// <returns>The decoded value.</returns>
    public uint ReadBits(uint bits)
    {
        if (bits > 19)
        {
            var tmp = this.ReadUInt16();
            return (this.ReadBits(bits - 16) << 16) | tmp;
        }

        // decode symbol, change length
        this.length >>= (int)bits;
        var sym = this.value / this.length;

        // update interval
        this.value -= this.length * sym;

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormalizeDecimalInterval();
        }

        return sym;
    }

    /// <summary>
    /// Decodes the next byte without using a model.
    /// </summary>
    /// <returns>The decoded byte.</returns>
    public byte ReadByte()
    {
        // decode symbol, change length
        this.length >>= 8;
        var sym = this.value / this.length;

        // update interval
        this.value -= this.length * sym;

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormalizeDecimalInterval();
        }

        return (byte)sym;
    }

    /// <summary>
    /// Decodes the next <see cref="ushort"/> without using a model.
    /// </summary>
    /// <returns>The decoded value.</returns>
    public ushort ReadUInt16()
    {
        // decode symbol, change length
        this.length >>= 16;
        var sym = this.value / this.length;

        // update interval
        this.value -= this.length * sym;

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormalizeDecimalInterval();
        }

        return (ushort)sym;
    }

    /// <summary>
    /// Decodes the next <see cref="uint"/> without using a model.
    /// </summary>
    /// <returns>The decoded value.</returns>
    public uint ReadUInt32()
    {
        uint lowerInt = this.ReadUInt16();
        uint upperInt = this.ReadUInt16();
        return (upperInt << 16) | lowerInt;
    }

    /// <summary>
    /// Decodes the next <see cref="float"/> without using a model.
    /// </summary>
    /// <returns>The decoded value.</returns>
    public float ReadSingle() => BitConverter.UInt32BitsToSingle(this.ReadUInt32());

    /// <summary>
    /// Decodes the next <see cref="ulong"/> without using a model.
    /// </summary>
    /// <returns>The decoded value.</returns>
    public ulong ReadUInt64()
    {
        ulong lowerInt = this.ReadUInt32();
        ulong upperInt = this.ReadUInt32();
        return (upperInt << 32) | lowerInt;
    }

    /// <summary>
    /// Decodes the next <see cref="double"/> without using a model.
    /// </summary>
    /// <returns>The decoded value.</returns>
    public double ReadDouble() => BitConverter.UInt64BitsToDouble(this.ReadUInt64());

    /// <summary>
    /// Gets the underlying input stream.
    /// </summary>
    /// <returns>The stream.</returns>
    /// <exception cref="CompressionNotInitializedException">The decoder has not been initialized.</exception>
    public Stream GetStream() => this.inputStream ?? throw new CompressionNotInitializedException();

    private void RenormalizeDecimalInterval()
    {
        var reader = this.GetStream();

        do
        {
            this.value = (this.value << 8) | reader.ReadByteLittleEndian();
        }
        while ((this.length <<= 8) < MinLength);
    }
}