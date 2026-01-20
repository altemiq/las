// -----------------------------------------------------------------------
// <copyright file="VariableLengthRecordHeader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a variable length record.
/// </summary>
[Serializable]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = Size)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2292:Trivial properties should be auto-implemented", Justification = "This needs sequential layout")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1085:Use auto-implemented property", Justification = "This needs sequential layout")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required for automated cleanup")]
public readonly record struct VariableLengthRecordHeader
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
                               + sizeof(ushort)
                               + (sizeof(sbyte) * DescriptionSize);

    private const int UserIdSize = 16;
    private const int DescriptionSize = 32;

    private readonly ushort reserved;

    [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = UserIdSize)]
    private readonly string userId;

    private readonly ushort recordId;

    private readonly ushort recordLengthAfterHeader;

    [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = DescriptionSize)]
    private readonly string description;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    /// Initializes a new instance of the <see cref="VariableLengthRecordHeader"/> struct.
    /// </summary>
    /// <param name="data">The input data.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public VariableLengthRecordHeader(byte[] data)
        : this(new ReadOnlySpan<byte>(data))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableLengthRecordHeader"/> struct.
    /// </summary>
    /// <param name="data">The input data.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public VariableLengthRecordHeader(ReadOnlySpan<byte> data)
    {
        this.reserved = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[..Constants.VariableLengthRecord.UserIdFieldOffset]);
        this.userId = System.Text.Encoding.UTF8.GetNullTerminatedString(data[Constants.VariableLengthRecord.UserIdFieldOffset..Constants.VariableLengthRecord.RecordIdFieldOffset]);
        this.recordId = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.VariableLengthRecord.RecordIdFieldOffset..Constants.VariableLengthRecord.RecordLengthAfterHeaderFieldOffset]);
        this.recordLengthAfterHeader = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.VariableLengthRecord.RecordLengthAfterHeaderFieldOffset..Constants.VariableLengthRecord.DescriptionFieldOffset]);
        this.description = System.Text.Encoding.UTF8.GetNullTerminatedString(data[Constants.VariableLengthRecord.DescriptionFieldOffset..Size]);
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Gets the summary.
    /// </summary>
    public required string UserId { get => this.userId; init => this.userId = value; }

    /// <summary>
    /// Gets the record ID.
    /// </summary>
    public required ushort RecordId { get => this.recordId; init => this.recordId = value; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get => this.description; init => this.description = value; }

    /// <summary>
    /// Gets the record length after the header.
    /// </summary>
    public ushort RecordLengthAfterHeader { get => this.recordLengthAfterHeader; init => this.recordLengthAfterHeader = value; }

    /// <summary>
    /// Creates an instance of <see cref="VariableLengthRecordHeader"/> from the source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The instance of <see cref="VariableLengthRecordHeader"/>.</returns>
    public static VariableLengthRecordHeader Create(ReadOnlySpan<byte> source) => BitConverter.IsLittleEndian
        ? System.Runtime.InteropServices.Marshal.SpanToStructure<VariableLengthRecordHeader>(source)
        : new(source);

    /// <summary>
    /// Copies the contents of this instance into a destination <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
    public void CopyTo(Span<byte> destination)
    {
        if (BitConverter.IsLittleEndian)
        {
            System.Runtime.InteropServices.Marshal.StructureToSpan(this, destination);
        }
        else
        {
            this.WriteLittleEndian(destination);
        }
    }

    /// <summary>
    /// Writes this instance into a span of bytes, as little endian.
    /// </summary>
    /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
    public void WriteLittleEndian(Span<byte> destination)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[..Constants.VariableLengthRecord.UserIdFieldOffset], this.reserved);
        System.Text.Encoding.UTF8.GetNullTerminatedBytes(this.userId, destination[Constants.VariableLengthRecord.UserIdFieldOffset..Constants.VariableLengthRecord.RecordIdFieldOffset]);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.VariableLengthRecord.RecordIdFieldOffset..Constants.VariableLengthRecord.RecordLengthAfterHeaderFieldOffset], this.recordId);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.VariableLengthRecord.RecordLengthAfterHeaderFieldOffset..Constants.VariableLengthRecord.DescriptionFieldOffset], this.recordLengthAfterHeader);
        System.Text.Encoding.UTF8.GetNullTerminatedBytes(this.description, destination[Constants.VariableLengthRecord.DescriptionFieldOffset..Size]);
    }
}