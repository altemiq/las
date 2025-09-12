// -----------------------------------------------------------------------
// <copyright file="ClassificationLookup.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Classification lookup.
/// </summary>
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ClassificationLookup), nameof(Create))]
public sealed record ClassificationLookup : VariableLengthRecord, IReadOnlyList<ClassificationLookupItem>
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = default;

    private const int ValueSize = 16;

    private const int ValueCount = 256;

    private const int TotalSize = ValueCount * ValueSize;

    private readonly IReadOnlyList<ClassificationLookupItem> values;

    /// <summary>
    /// Initialises a new instance of the <see cref="ClassificationLookup"/> class.
    /// </summary>
    /// <param name="values">The values.</param>
    public ClassificationLookup(params IReadOnlyList<ClassificationLookupItem> values)
        : this(
            new()
            {
                UserId = VariableLengthRecordHeader.SpecUserId,
                RecordId = TagRecordId,
                RecordLengthAfterHeader = TotalSize,
            },
            values)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ClassificationLookup"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal ClassificationLookup(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : this(header, GetEntries(data))
    {
    }

    private ClassificationLookup(VariableLengthRecordHeader header, IReadOnlyList<ClassificationLookupItem> values)
        : base(header) => this.values = values;

    /// <inheritdoc />
    public int Count => this.values.Count;

    /// <inheritdoc />
    public ClassificationLookupItem this[int index] => this.values[index];

    /// <summary>
    /// Creates an instance of <see cref="ClassificationLookup"/>.
    /// </summary>
    /// <param name="items">The values.</param>
    /// <returns>The <see cref="ClassificationLookup"/>.</returns>
    public static ClassificationLookup Create(ReadOnlySpan<ClassificationLookupItem> items) => new(items.ToReadOnlyList());

    /// <inheritdoc />
    public IEnumerator<ClassificationLookupItem> GetEnumerator() => this.values.GetEnumerator();

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        const int End = VariableLengthRecordHeader.Size + TotalSize;
        this.Header.CopyTo(destination);
        int bytesWritten = VariableLengthRecordHeader.Size;
        var d = destination[bytesWritten..];

        foreach (var value in this.values)
        {
            if (BitConverter.IsLittleEndian)
            {
                System.Runtime.InteropServices.Marshal.StructureToPtr(value, GetIntPtrFromSpan(d), fDeleteOld: false);

                static unsafe IntPtr GetIntPtrFromSpan<T>(Span<T> span)
                {
                    // Cast the reference to an IntPtr
                    return (IntPtr)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(span));
                }
            }
            else
            {
                d[0] = value.ClassNumber;
                _ = System.Text.Encoding.UTF8.GetBytes(value.Description, d[1..]);
            }

            bytesWritten += ValueSize;
            d = destination[bytesWritten..];
        }

        // ensure we bank out the rest
        destination[bytesWritten..End].Clear();

        return End;
    }

    private static System.Collections.ObjectModel.ReadOnlyCollection<ClassificationLookupItem> GetEntries(ReadOnlySpan<byte> source)
    {
        var builder = new System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<ClassificationLookupItem>(ValueCount);

        for (var i = 0; i < ValueCount; i++)
        {
            var index = i * 16;
            var s = source[index..];
            var entry = BitConverter.IsLittleEndian
                ? Read(s)
                : new()
                {
                    ClassNumber = s[0],
                    Description = System.Text.Encoding.UTF8.GetNullTerminatedString(s[1..]),
                };

            builder.Add(entry);
        }

        return builder.ToReadOnlyCollection();

        static ClassificationLookupItem Read(ReadOnlySpan<byte> source)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStructure<ClassificationLookupItem>(GetIntPtrFromSpan(source));

            static unsafe IntPtr GetIntPtrFromSpan<T>(ReadOnlySpan<T> span)
            {
                // Cast the reference to an IntPtr
                return (IntPtr)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(span));
            }
        }
    }
}