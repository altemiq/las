// -----------------------------------------------------------------------
// <copyright file="CopcHierarchy.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Cloud;

/// <summary>
/// The <c>COPC</c> hierarchy.
/// </summary>
public sealed record CopcHierarchy : ExtendedVariableLengthRecord
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 1000;

    /// <summary>
    /// The header size.
    /// </summary>
    public const int HeaderSize = 60;

    private const int EntrySize = 32;

    private readonly SortedDictionary<Entry, Page> pages = new(new EntryComparer());

    /// <summary>
    /// Initializes a new instance of the <see cref="CopcHierarchy"/> class.
    /// </summary>
    /// <param name="entries">The entries.</param>
    public CopcHierarchy(ICollection<Entry> entries)
        : base(new ExtendedVariableLengthRecordHeader
        {
            UserId = CopcConstants.UserId,
            RecordId = TagRecordId,
            RecordLengthAfterHeader = (ushort)(entries.Count * EntrySize),
            Description = "EPT hierarchy",
        }) => this.Root = new([.. entries]);

    /// <summary>
    /// Initializes a new instance of the <see cref="CopcHierarchy"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="info">The COPC information.</param>
    /// <param name="evlrStart">The start of the EVLR in bytes.</param>
    /// <param name="data">The data.</param>
    internal CopcHierarchy(ExtendedVariableLengthRecordHeader header, CopcInfo info, ulong evlrStart, ReadOnlySpan<byte> data)
        : base(header)
    {
        var temp = info.RootHierOffset - evlrStart;
        var rootOffset = (int)(temp - ExtendedVariableLengthRecordHeader.Size);
        this.Root = new(data[rootOffset..], (int)(info.RootHierSize / EntrySize));

        // go through reach entry in root
        foreach (var entry in this.Root)
        {
            ProcessPage(this.pages, entry, data, evlrStart);
        }

        static void ProcessPage(IDictionary<Entry, Page> pages, Entry entry, ReadOnlySpan<byte> data, ulong recordStart)
        {
            if (entry.PointCount is not -1)
            {
                return;
            }

            var offset = (int)(entry.Offset - recordStart) - ExtendedVariableLengthRecordHeader.Size;
            var page = new Page(data[offset..], entry.ByteSize / EntrySize);
            pages.Add(entry, page);

            foreach (var pageEntry in page)
            {
                ProcessPage(pages, pageEntry, data, recordStart);
            }
        }
    }

    /// <summary>
    /// Gets the count.
    /// </summary>
    public Page Root { get; }

    /// <summary>
    /// Gets the page.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <returns>The page.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="entry"/> does not represent a page link.</exception>
    public Page GetPage(in Entry entry) => this.pages.TryGetValue(entry, out var page) ? page : throw new ArgumentOutOfRangeException(nameof(entry));

    /// <summary>
    /// Tries to get the page.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="page">The page.</param>
    /// <returns><see langword="true"/> if the page was found successfully; otherwise <see langword="false"/>.</returns>
    public bool TryGetPage(in Entry entry, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Page? page) => this.pages.TryGetValue(entry, out page);

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        this.Header.CopyTo(destination);
        int bytesWritten = ExtendedVariableLengthRecordHeader.Size;
        bytesWritten += WritePage(destination[bytesWritten..], this.Root);

        foreach (var page in this.pages)
        {
            bytesWritten += WritePage(destination[bytesWritten..], page.Value);
        }

        return bytesWritten;

        static int WritePage(Span<byte> destination, Page page)
        {
            var bytesWritten = 0;
            foreach (var entry in page)
            {
                entry.CopyTo(destination);
                bytesWritten += EntrySize;
                destination = destination[EntrySize..];
            }

            return bytesWritten;
        }
    }

    /// <summary>
    /// An entry corresponds to a single key/value pair in an EPT hierarchy, but contains additional information to allow direct access and decoding of the corresponding point data.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public readonly struct Entry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Entry"/> struct.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="byteSize">The byte size.</param>
        /// <param name="pointCount">The point count.</param>
        public Entry(in VoxelKey key, ulong offset, int byteSize, int pointCount) => (this.Key, this.Offset, this.ByteSize, this.PointCount) = (key, offset, byteSize, pointCount);

        /// <summary>
        /// Initializes a new instance of the <see cref="Entry"/> struct.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public Entry(ReadOnlySpan<byte> bytes)
        {
            this.Key = new(bytes);
            bytes = bytes[(4 * sizeof(int))..];
            this.Offset = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(bytes);
            bytes = bytes[sizeof(ulong)..];
            this.ByteSize = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes);
            bytes = bytes[sizeof(int)..];
            this.PointCount = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes);
        }

        /// <summary>
        /// Gets the EPT key of the data to which this entry corresponds.
        /// </summary>
        public VoxelKey Key { get; }

        /// <summary>
        /// Gets the offset.
        /// <list type="bullet">
        ///   <item>Absolute offset to the data chunk if the pointCount > 0.</item>
        ///   <item>Absolute offset to a child hierarchy page if the pointCount is -1.</item>
        ///   <item>0 if the pointCount is 0.</item>
        /// </list>
        /// </summary>
        public ulong Offset { get; }

        /// <summary>
        /// Gets the byte size.
        /// <list type="bullet">
        ///   <item>Size of the data chunk in bytes (compressed size) if the pointCount > 0.</item>
        ///   <item>Size of the hierarchy page if the pointCount is -1.</item>
        ///   <item>0 if the pointCount is 0.</item>
        /// </list>
        /// </summary>
        public int ByteSize { get; }

        /// <summary>
        /// Gets the point count.
        /// <list type="bullet">
        ///   <item>If > 0, represents the number of points in the data chunk.</item>
        ///   <item>If -1, indicates the information for this octree node is found in another hierarchy page.</item>
        ///   <item>If 0, no point data exists for this key, though may exist for child entries.</item>
        /// </list>
        /// </summary>
        public int PointCount { get; }

        /// <summary>
        /// Implements the equals operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Entry left, Entry right) => left.Equals(right);

        /// <summary>
        /// Implements the not-equals operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Entry left, Entry right) => !(left == right);

        /// <inheritdoc/>
        public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is Entry entry
                                                                                                        && this.Key == entry.Key
                                                                                                        && this.Offset == entry.Offset
                                                                                                        && this.ByteSize == entry.ByteSize
                                                                                                        && this.PointCount == entry.PointCount;

        /// <inheritdoc/>
        public override int GetHashCode() =>
#if NETSTANDARD2_0_OR_GREATER || NET461_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            HashCode.Combine(this.Key, this.Offset, this.ByteSize, this.PointCount);
#else
            (this.Key, this.Offset, this.ByteSize, this.PointCount).GetHashCode();
#endif

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the value of this instance.</returns>
        public override string ToString() =>
            $$"""
            {
              {{nameof(this.Key)}}: {{this.Key}},
              {{nameof(this.Offset)}}: {{this.Offset}},
              {{nameof(this.ByteSize)}}: {{this.ByteSize}},
              {{nameof(this.PointCount)}}: {{this.PointCount}}
            }
            """;

        /// <summary>
        /// Copies the contents of this instance into a destination <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
        internal void CopyTo(Span<byte> destination)
        {
            var index = 0;
            this.Key.CopyTo(destination);
            index += 4 * sizeof(int);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination[index..], this.Offset);
            index += sizeof(ulong);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[index..], this.ByteSize);
            index += sizeof(int);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[index..], this.PointCount);
        }
    }

    /// <summary>
    /// The VoxelKey corresponds to the naming of EPT data files.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public readonly struct VoxelKey : IEquatable<VoxelKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoxelKey"/> struct.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public VoxelKey(int level, int x, int y, int z) => (this.Level, this.X, this.Y, this.Z) = (level, x, y, z);

        /// <summary>
        /// Initializes a new instance of the <see cref="VoxelKey"/> struct.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public VoxelKey(ReadOnlySpan<byte> bytes)
        {
            this.Level = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes);
            this.X = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes[sizeof(int)..]);
            this.Y = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes[(2 * sizeof(int))..]);
            this.Z = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes[(3 * sizeof(int))..]);
        }

        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <remarks>A value &lt; 0 indicates an invalid <see cref="VoxelKey"/>.</remarks>
        public int Level { get; }

        /// <summary>
        /// Gets the X.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets the Z.
        /// </summary>
        public int Z { get; }

        /// <summary>
        /// Implements the equals operator.
        /// </summary>
        /// <param name="a">The first key.</param>
        /// <param name="b">The second key.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(in VoxelKey a, in VoxelKey b) => Equals(in a, in b);

        /// <summary>
        /// Implements the not-equals operator.
        /// </summary>
        /// <param name="a">The first key.</param>
        /// <param name="b">The second key.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(in VoxelKey a, in VoxelKey b) => a.Level != b.Level || a.X != b.X || a.Y != b.Y || a.Z == b.Z;

        /// <inheritdoc/>
        public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is VoxelKey key && this.Equals(key);

        /// <inheritdoc/>
        public bool Equals(VoxelKey other) => this.Equals(in other);

        /// <inheritdoc cref="Equals(VoxelKey)"/>
        public bool Equals(in VoxelKey other) => Equals(in this, in other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(this.Level, this.X, this.Y, this.Z);

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the value of this instance.</returns>
        public override string ToString() =>
            $$"""
            {
              {{nameof(this.Level)}}: {{this.Level}},
              {{nameof(this.X)}}: {{this.X}},
              {{nameof(this.Y)}}: {{this.Y}},
              {{nameof(this.Z)}}: {{this.Z}}
            }
            """;

        /// <summary>
        /// Copies this instance into the destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        internal void CopyTo(Span<byte> destination)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination, this.Level);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[sizeof(int)..], this.X);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[(2 * sizeof(int))..], this.Y);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[(3 * sizeof(int))..], this.Z);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static bool Equals(in VoxelKey a, in VoxelKey b) => a.Level == b.Level && a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    }

    /// <summary>
    /// Gets the page.
    /// </summary>
    public sealed class Page : IReadOnlyList<Entry>
    {
        private readonly IReadOnlyList<Entry> entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class.
        /// </summary>
        /// <param name="entries">The entries.</param>
        internal Page(IReadOnlyList<Entry> entries) => this.entries = entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="count">The count.</param>
        internal Page(ReadOnlySpan<byte> data, int count)
        {
            var array = new Entry[count];
            for (int i = 0; i < count; i++)
            {
                var start = i * EntrySize;
                array[i] = new(data[start..]);
            }

            this.entries = array;
        }

        /// <inheritdoc />
        public int Count => this.entries.Count;

        /// <inheritdoc />
        public Entry this[int index] => this.entries[index];

        /// <inheritdoc/>
        public IEnumerator<Entry> GetEnumerator() => this.entries.GetEnumerator();

        /// <inheritdoc/>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <inheritdoc/>
        public override string ToString() => this.ToString(default);

#if NETSTANDARD1_1
        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="string" /> using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="string" /> instance equivalent to the value of this instance.</returns>
#else
        /// <inheritdoc cref="IConvertible.ToString(IFormatProvider)" />
#endif
        public string ToString(IFormatProvider? provider)
#if NETCOREAPP6_0_OR_GREATER
            => string.Create(formatProvider, $"Count: {this.Count}");
#elif NETCOREAPP1_0_OR_GREATER || NET46_OR_GREATER || NETSTANDARD1_3_OR_GREATER
            => ((FormattableString)$"Count: {this.Count}").ToString(provider);
#else
            => string.Format(provider, "Count: {0}", this.Count);
#endif
    }

    private sealed class EntryComparer : IComparer<Entry>
    {
        public int Compare(Entry x, Entry y) => x.Offset.CompareTo(y.Offset);
    }
}