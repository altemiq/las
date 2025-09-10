// -----------------------------------------------------------------------
// <copyright file="LaxTag.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// The <see cref="LasIndex"/> <see cref="ExtendedVariableLengthRecord"/>.
/// </summary>
public sealed record LaxTag : ExtendedVariableLengthRecord
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 30;

    private const string DefaultUserId = "LAStools";

    private readonly ReadOnlyMemory<byte> data;

    /// <summary>
    /// Initializes a new instance of the <see cref="LaxTag"/> class from the specified <see cref="LasIndex"/>.
    /// </summary>
    /// <param name="index">The LAS index.</param>
    public LaxTag(LasIndex index)
    : this(GetBytes(index))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LaxTag"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal LaxTag(ExtendedVariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : base(header) => this.data = data.ToArray();

    private LaxTag(ReadOnlyMemory<byte> data)
        : base(new ExtendedVariableLengthRecordHeader
        {
            UserId = DefaultUserId,
            RecordId = TagRecordId,
            Description = "LAX spatial indexing (LASindex)",
            RecordLengthAfterHeader = (ulong)data.Length,
        }) => this.data = data;

    /// <summary>
    /// Reads the index from this instance.
    /// </summary>
    /// <returns>The index.</returns>
    public LasIndex GetIndex() => LasIndex.ReadFrom(this.data.Span);

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        this.Header.CopyTo(destination);
        this.data.Span.CopyTo(destination[ExtendedVariableLengthRecordHeader.Size..]);
        return ExtendedVariableLengthRecordHeader.Size + this.data.Length;
    }

    private static byte[] GetBytes(LasIndex index)
    {
        using var memoryStream = new MemoryStream();

        using (var binaryWriter = new BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            index.WriteTo(binaryWriter);
        }

        return memoryStream.ToArray();
    }
}