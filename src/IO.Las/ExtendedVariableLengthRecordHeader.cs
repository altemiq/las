// -----------------------------------------------------------------------
// <copyright file="ExtendedVariableLengthRecordHeader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a variable length record.
/// </summary>
[Serializable]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = Size, Pack = 2)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2292:Trivial properties should be auto-implemented", Justification = "This needs sequential layout")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1085:Use auto-implemented property", Justification = "This needs sequential layout")]
public readonly record struct ExtendedVariableLengthRecordHeader
{
    /// <summary>
    /// The projection user ID.
    /// </summary>
    public const string ProjectionUserId = "LASF_Projection";

    /// <summary>
    /// The spec user ID.
    /// </summary>
    public const string SpecUserId = "LASF_Spec";

    /// <summary>
    /// The base size.
    /// </summary>
    public const ushort Size = sizeof(ushort)
                               + (sizeof(sbyte) * UserIdSize)
                               + sizeof(ushort)
                               + sizeof(ulong)
                               + (sizeof(sbyte) * DescriptionSize);

    private const int UserIdSize = 16;
    private const int DescriptionSize = 32;

    private readonly ushort reserved;

    [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = UserIdSize)]
    private readonly string userId;

    private readonly ushort recordId;

    private readonly ulong recordLengthAfterHeader;

    [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = DescriptionSize)]
    private readonly string description;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedVariableLengthRecordHeader"/> struct.
    /// </summary>
    /// <param name="data">The input data.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public ExtendedVariableLengthRecordHeader(byte[] data)
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        : this(new ReadOnlySpan<byte>(data))
    {
    }
#else
    {
        this.reserved = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(Constants.ExtendedVariableLengthRecord.ReservedFieldOffset, sizeof(ushort)));
        this.userId = System.Text.Encoding.UTF8.GetString(data, Constants.ExtendedVariableLengthRecord.UserIdFieldOffset, GetNullChar(data, Constants.ExtendedVariableLengthRecord.UserIdFieldOffset) - Constants.ExtendedVariableLengthRecord.UserIdFieldOffset);
        this.recordId = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(Constants.ExtendedVariableLengthRecord.RecordIdFieldOffset, sizeof(ushort)));
        this.recordLengthAfterHeader = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(Constants.ExtendedVariableLengthRecord.RecordLengthAfterHeaderFieldOffset, sizeof(ulong)));
        this.description = System.Text.Encoding.UTF8.GetString(data, Constants.ExtendedVariableLengthRecord.DescriptionFieldOffset, GetNullChar(data, Constants.ExtendedVariableLengthRecord.DescriptionFieldOffset) - Constants.ExtendedVariableLengthRecord.DescriptionFieldOffset);

        static int GetNullChar(byte[] source, int startIndex = 0)
        {
            for (var i = startIndex; i < source.Length; i++)
            {
                if (source[i] is 0)
                {
                    return i;
                }
            }

            return source.Length;
        }
    }
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedVariableLengthRecordHeader"/> struct.
    /// </summary>
    /// <param name="data">The input data.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public ExtendedVariableLengthRecordHeader(ReadOnlySpan<byte> data)
    {
        this.reserved = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[..Constants.ExtendedVariableLengthRecord.UserIdFieldOffset]);
        this.userId = System.Text.Encoding.UTF8.GetNullTerminatedString(data[Constants.ExtendedVariableLengthRecord.UserIdFieldOffset..Constants.ExtendedVariableLengthRecord.RecordIdFieldOffset]);
        this.recordId = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.ExtendedVariableLengthRecord.RecordIdFieldOffset..Constants.ExtendedVariableLengthRecord.RecordLengthAfterHeaderFieldOffset]);
        this.recordLengthAfterHeader = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(data[Constants.ExtendedVariableLengthRecord.RecordLengthAfterHeaderFieldOffset..Constants.ExtendedVariableLengthRecord.DescriptionFieldOffset]);
        this.description = System.Text.Encoding.UTF8.GetNullTerminatedString(data[Constants.ExtendedVariableLengthRecord.DescriptionFieldOffset..Size]);
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Gets the summary.
    /// </summary>
    public required string UserId
    {
        get => this.userId;
        init => this.userId = value;
    }

    /// <summary>
    /// Gets the record ID.
    /// </summary>
    public required ushort RecordId
    {
        get => this.recordId;
        init => this.recordId = value;
    }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description
    {
        get => this.description;
        init => this.description = value;
    }

    /// <summary>
    /// Gets the record length after the header.
    /// </summary>
    public ulong RecordLengthAfterHeader
    {
        get => this.recordLengthAfterHeader;
        init => this.recordLengthAfterHeader = value;
    }

    /// <summary>
    /// Reads an instance of <see cref="ExtendedVariableLengthRecordHeader"/> from the source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The instance of <see cref="ExtendedVariableLengthRecordHeader"/>.</returns>
    public static ExtendedVariableLengthRecordHeader Read(ReadOnlySpan<byte> source)
    {
        if (BitConverter.IsLittleEndian)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStructure<ExtendedVariableLengthRecordHeader>(GetIntPtrFromSpan(source));

            static unsafe IntPtr GetIntPtrFromSpan<T>(ReadOnlySpan<T> span)
            {
                // Cast the reference to an IntPtr
                return (IntPtr)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(span));
            }
        }

        return new(source);
    }

    /// <summary>
    /// Writes this instance to the destination.
    /// </summary>
    /// <param name="destination">The destination.</param>
    public void CopyTo(Span<byte> destination)
    {
        if (BitConverter.IsLittleEndian)
        {
            System.Runtime.InteropServices.Marshal.StructureToPtr(this, GetIntPtrFromSpan(destination), fDeleteOld: false);
            return;

            static unsafe IntPtr GetIntPtrFromSpan<T>(Span<T> span)
            {
                // Cast the reference to an IntPtr
                return (IntPtr)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(span));
            }
        }

        this.WriteLittleEndian(destination);
    }

    /// <summary>
    /// Writes this instance into a span of bytes, as little endian.
    /// </summary>
    /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
    public void WriteLittleEndian(Span<byte> destination)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[..Constants.ExtendedVariableLengthRecord.UserIdFieldOffset], this.reserved);
        WriteString(destination[Constants.ExtendedVariableLengthRecord.UserIdFieldOffset..Constants.ExtendedVariableLengthRecord.RecordIdFieldOffset], this.userId);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.ExtendedVariableLengthRecord.RecordIdFieldOffset..Constants.ExtendedVariableLengthRecord.RecordLengthAfterHeaderFieldOffset], this.recordId);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination[Constants.ExtendedVariableLengthRecord.RecordLengthAfterHeaderFieldOffset..Constants.ExtendedVariableLengthRecord.DescriptionFieldOffset], this.recordLengthAfterHeader);
        WriteString(destination[Constants.ExtendedVariableLengthRecord.DescriptionFieldOffset..Size], this.description);

        static void WriteString(Span<byte> destination, string input)
        {
            var length = Math.Min(destination.Length, input.Length);
            int current;
            for (current = 0; current < length; current++)
            {
                destination[current] = (byte)input[current];
            }

            while (current < destination.Length)
            {
                destination[current] = 0;
                current++;
            }
        }
    }
}