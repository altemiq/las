// -----------------------------------------------------------------------
// <copyright file="ArithmeticEncoder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The arithmetic encoder.
/// </summary>
internal sealed class ArithmeticEncoder : ArithmeticCoder
{
    private const int Size = sizeof(byte) * 2 * BufferSize;
    private readonly byte[] outBuffer = new byte[Size];
    private Stream? outputStream;
    private int outByte;
    private int endByte;

    private uint @base;
    private uint length;

    /// <summary>
    /// Initializes this instance against the specified output <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to encode into.</param>
    /// <returns><see langword="true"/> if initialized, or <see langword="false"/> if <paramref name="stream"/> was <see langword="null"/>.</returns>
    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(outputStream))]
    public bool Initialize(Stream? stream)
    {
        if (stream is null)
        {
            return false;
        }

        this.outputStream = stream;
        this.@base = default;
        this.length = MaxLength;
        this.outByte = default;
        this.endByte = Size;

        return true;
    }

    /// <inheritdoc />
    public override void Done()
    {
        if (this.outputStream is null)
        {
            // not initialized.
            return;
        }

        // done encoding: set final data bytes
        var initialBase = this.@base;
        var anotherByte = true;

        if (this.length > 2 * MinLength)
        {
            // base offset
            this.@base += MinLength;

            // set new length for 1 more byte
            this.length = MinLength >> 1;
        }
        else
        {
            // base offset
            this.@base += MinLength >> 1;

            // set new length for 2 more bytes
            this.length = MinLength >> 9;
            anotherByte = false;
        }

        if (initialBase > this.@base)
        {
            // overflow = carry
            this.PropagateCarry();
        }

        // renormalization = output last bytes
        this.RenormEncInterval();

        if (this.endByte != Size)
        {
            this.outputStream.Write(this.outBuffer, BufferSize, BufferSize);
        }

        var bufferSize = this.outByte;
        if (bufferSize is not 0)
        {
            this.outputStream.Write(this.outBuffer, 0, bufferSize);
        }

        // write two or three zero bytes to be in sync with the decoder's byte reads
        this.outputStream.WriteByteLittleEndian(byte.MinValue);
        this.outputStream.WriteByteLittleEndian(byte.MinValue);
        if (anotherByte)
        {
            this.outputStream.WriteByteLittleEndian(byte.MinValue);
        }

        this.outputStream = null;
    }

    /// <summary>
    /// Encodes a bit with modelling.
    /// </summary>
    /// <param name="model">The bit model.</param>
    /// <param name="sym">The symbol (0 or 1).</param>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void EncodeBit(ArithmeticBitModel model, uint sym)
    {
        // product l x p0 update interval
        var x = model.BitZeroProb * (this.length >> ArithmeticBitModel.LengthShift);

        if (sym is 0)
        {
            this.length = x;
            model.IncrementBitZeroCount();
        }
        else
        {
            var initialBase = this.@base;
            this.@base += x;
            this.length -= x;
            if (initialBase > this.@base)
            {
                // overflow = carry
                this.PropagateCarry();
            }
        }

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormEncInterval();
        }

        if (model.DecrementBitsUntilUpdate() is 0)
        {
            // periodic model update
            model.Update();
        }
    }

    /// <summary>
    /// Encodes a symbol with modelling.
    /// </summary>
    /// <param name="model">The symbol model.</param>
    /// <param name="sym">The symbol index.</param>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void EncodeSymbol(ArithmeticSymbolModel model, uint sym)
    {
        // compute products
        var initialBase = this.@base;
        if (sym == model.LastSymbol)
        {
            var x = model.Distribution[sym] * (this.length >> ArithmeticSymbolModel.LengthShift);

            // update interval
            this.@base += x;

            // no product needed
            this.length -= x;
        }
        else
        {
            this.length >>= ArithmeticSymbolModel.LengthShift;
            var x = model.Distribution[sym] * this.length;

            // update interval
            this.@base += x;
            this.length = (model.Distribution[sym + 1] * this.length) - x;
        }

        if (initialBase > this.@base)
        {
            // overflow = carry
            this.PropagateCarry();
        }

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormEncInterval();
        }

        ++model.SymbolCount[sym];
        if (model.DecrementSymbolsUntilUpdate() is 0)
        {
            // periodic model update
            model.Update();
        }
    }

    /// <summary>
    /// Encodes the next bit without using a model.
    /// </summary>
    /// <param name="sym">The bit (0 or 1).</param>
    public void WriteBit(uint sym)
    {
        var initialBase = this.@base;

        // new interval base and length
        this.length >>= 1;
        this.@base += sym * this.length;

        if (initialBase > this.@base)
        {
            // overflow = carry
            this.PropagateCarry();
        }

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormEncInterval();
        }
    }

    /// <summary>
    /// Encodes the next <paramref name="bits"/> bits without using a model.
    /// </summary>
    /// <param name="bits">The number of bits to write (1-32).</param>
    /// <param name="sym">The value to encode.</param>
    public void WriteBits(int bits, uint sym)
    {
        if (bits > 19)
        {
            this.WriteUInt16((ushort)(sym & ushort.MaxValue));
            sym >>= 16;
            bits -= 16;
        }

        var initialBase = this.@base;

        // new interval base and length
        this.length >>= bits;
        this.@base += sym * this.length;

        if (initialBase > this.@base)
        {
            // overflow = carry
            this.PropagateCarry();
        }

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormEncInterval();
        }
    }

    /// <summary>
    /// Encodes the next byte without using a model.
    /// </summary>
    /// <param name="sym">The byte to encode.</param>
    public void WriteByte(byte sym)
    {
        var initialBase = this.@base;

        // new interval base and length
        this.length >>= 8;
        this.@base += sym * this.length;

        if (initialBase > this.@base)
        {
            // overflow = carry
            this.PropagateCarry();
        }

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormEncInterval();
        }
    }

    /// <summary>
    /// Encodes the next <see cref="ushort"/> without using a model.
    /// </summary>
    /// <param name="sym">The value to encode.</param>
    public void WriteUInt16(ushort sym)
    {
        var initialBase = this.@base;

        // new interval base and length
        this.length >>= 16;
        this.@base += sym * this.length;

        if (initialBase > this.@base)
        {
            // overflow = carry
            this.PropagateCarry();
        }

        if (this.length < MinLength)
        {
            // renormalization
            this.RenormEncInterval();
        }
    }

    /// <summary>
    /// Encodes the next <see cref="uint"/> without using a model.
    /// </summary>
    /// <param name="sym">The value to encode.</param>
    public void WriteUInt32(uint sym)
    {
        // lower 16 bits
        this.WriteUInt16((ushort)(sym & 0xFFFF));

        // UPPER 16 bits
        this.WriteUInt16((ushort)(sym >> 16));
    }

    /// <summary>
    /// Encodes the next <see cref="float"/> without using a model.
    /// </summary>
    /// <param name="sym">The value to encode.</param>
    public void WriteSingle(float sym) => this.WriteUInt32(BitConverter.SingleToUInt32Bits(sym));

    /// <summary>
    /// Encodes the next <see cref="ulong"/> without using a model.
    /// </summary>
    /// <param name="sym">The value to encode.</param>
    public void WriteUInt64(ulong sym)
    {
        // lower 32 bits
        this.WriteUInt32((uint)(sym & 0xFFFFFFFF));

        // UPPER 32 bits
        this.WriteUInt32((uint)(sym >> 32));
    }

    /// <summary>
    /// Encodes the next <see cref="double"/> without using a model.
    /// </summary>
    /// <param name="sym">The value to encode.</param>
    public void WriteDouble(double sym) => this.WriteUInt64(BitConverter.DoubleToUInt64Bits(sym));

    /// <summary>
    /// Gets the stream.
    /// </summary>
    /// <returns>The stream.</returns>
    /// <exception cref="CompressionNotInitializedException">The compression has not been initialized.</exception>
    public Stream GetStream() => this.outputStream ?? throw new CompressionNotInitializedException();

    private void PropagateCarry()
    {
        var current = this.outByte is 0
            ? Size - 1
            : this.outByte - 1;
        while (this.outBuffer[current] is byte.MaxValue)
        {
            this.outBuffer[current] = default;
            if (current is 0)
            {
                current = Size - 1;
            }
            else
            {
                current--;
            }
        }

        this.outBuffer[current]++;
    }

    private void RenormEncInterval()
    {
        // output and discard top byte
        do
        {
            this.outBuffer[this.outByte++] = (byte)(this.@base >> 24);
            if (this.outByte == this.endByte)
            {
                this.ManageOutBuffer();
            }

            this.@base <<= 8;
        }
        while ((this.length <<= 8) < MinLength); // length multiplied by 256
    }

    private void ManageOutBuffer()
    {
        if (this.outputStream is null)
        {
            throw new CompressionNotInitializedException();
        }

        if (this.outByte is Size)
        {
            this.outByte = default;
        }

        this.outputStream.Write(this.outBuffer, this.outByte, BufferSize);
        this.endByte = this.outByte + BufferSize;
    }
}