// -----------------------------------------------------------------------
// <copyright file="ArithmeticEncoder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The arithmetic encoder.
/// </summary>
internal sealed class ArithmeticEncoder : ArithmeticCoder, IEntropyEncoder
{
    private readonly byte[] outBuffer;
    private readonly int endBuffer;
    private BinaryWriter? binaryWriter;
    private int outByte;
    private int endByte;

    private uint @base;
    private uint length;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArithmeticEncoder"/> class.
    /// </summary>
    public ArithmeticEncoder()
    {
        this.outBuffer = new byte[sizeof(byte) * 2 * BufferSize];
        this.endBuffer = this.outBuffer.Length;
    }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(binaryWriter))]
    public bool Initialize(BinaryWriter? writer)
    {
        if (writer is null)
        {
            return false;
        }

        this.binaryWriter = writer;
        this.@base = default;
        this.length = MaxLength;
        this.outByte = default;
        this.endByte = this.endBuffer;

        return true;
    }

    /// <inheritdoc/>
    public override void Done()
    {
        if (this.binaryWriter is null)
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

        if (this.endByte != this.endBuffer)
        {
            this.binaryWriter.Write(this.outBuffer, BufferSize, BufferSize);
        }

        var bufferSize = this.outByte;
        if (bufferSize is not 0)
        {
            this.binaryWriter.Write(this.outBuffer, 0, bufferSize);
        }

        // write two or three zero bytes to be in sync with the decoder's byte reads
        this.binaryWriter.Write(byte.MinValue);
        this.binaryWriter.Write(byte.MinValue);
        if (anotherByte)
        {
            this.binaryWriter.Write(byte.MinValue);
        }

        this.binaryWriter = null;
    }

    /// <inheritdoc/>
    public void EncodeBit(IBitModel model, uint sym)
    {
        // product l x p0 update interval
        var x = model.BitZeroProb * (this.length >> (int)ArithmeticBitModel.LengthShift);

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

    /// <inheritdoc/>
    public void EncodeSymbol(ISymbolModel model, uint sym)
    {
        // compute products
        var initialBase = this.@base;
        if (sym == model.LastSymbol)
        {
            var x = model.Distribution[sym] * (this.length >> (int)ArithmeticSymbolModel.LengthShift);

            // update interval
            this.@base += x;

            // no product needed
            this.length -= x;
        }
        else
        {
            this.length >>= (int)ArithmeticSymbolModel.LengthShift;
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void WriteBits(uint bits, uint sym)
    {
        if (bits > 19)
        {
            this.WriteShort((ushort)(sym & ushort.MaxValue));
            sym >>= 16;
            bits -= 16;
        }

        var initialBase = this.@base;

        // new interval base and length
        this.length >>= (int)bits;
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void WriteShort(ushort sym)
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

    /// <inheritdoc/>
    public void WriteInt(uint sym)
    {
        // lower 16 bits
        this.WriteShort((ushort)(sym & 0xFFFF));

        // UPPER 16 bits
        this.WriteShort((ushort)(sym >> 16));
    }

    /// <inheritdoc/>
    public void WriteFloat(float sym) => this.WriteInt(ExtendedBitConverter.SingleToUInt32Bits(sym));

    /// <inheritdoc/>
    public void WriteInt64(ulong sym)
    {
        // lower 32 bits
        this.WriteInt((uint)(sym & 0xFFFFFFFF));

        // UPPER 32 bits
        this.WriteInt((uint)(sym >> 32));
    }

    /// <inheritdoc/>
    public void WriteDouble(double sym) => this.WriteInt64(ExtendedBitConverter.DoubleToUInt64Bits(sym));

    /// <summary>
    /// Gets the binary writer.
    /// </summary>
    /// <returns>The binary writer.</returns>
    public BinaryWriter GetBinaryWriter() => this.binaryWriter ?? throw new CompressionNotInitializedException();

    private void PropagateCarry()
    {
        var p = this.outByte is 0
            ? this.endBuffer - 1
            : this.outByte - 1;
        while (this.outBuffer[p] is byte.MaxValue)
        {
            this.outBuffer[p] = default;
            if (p is 0)
            {
                p = this.endBuffer - 1;
            }
            else
            {
                p--;
            }
        }

        this.outBuffer[p]++;
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
        if (this.binaryWriter is null)
        {
            throw new CompressionNotInitializedException();
        }

        if (this.outByte == this.endBuffer)
        {
            this.outByte = default;
        }

        this.binaryWriter.Write(this.outBuffer, this.outByte, BufferSize);
        this.endByte = this.outByte + BufferSize;
    }
}