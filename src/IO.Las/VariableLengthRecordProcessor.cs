// -----------------------------------------------------------------------
// <copyright file="VariableLengthRecordProcessor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#if LAS1_3_OR_GREATER
/// <summary>
/// The <see cref="VariableLengthRecord"/> and <see cref="ExtendedVariableLengthRecord"/> processor.
/// </summary>
#else
/// <summary>
/// The <see cref="VariableLengthRecord"/> processor.
/// </summary>
#endif
public sealed class VariableLengthRecordProcessor
{
    /// <summary>
    /// The instance.
    /// </summary>
    public static readonly VariableLengthRecordProcessor Instance = new();

    private readonly System.Collections.Concurrent.ConcurrentDictionary<Key, CreateVariableLengthRecord> processors = [];

#if LAS1_3_OR_GREATER
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Key, CreateExtendedVariableLengthRecord> extendedProcessors = [];
#endif

    private VariableLengthRecordProcessor()
    {
        this.Register(VariableLengthRecordHeader.ProjectionUserId, GeoKeyDirectoryTag.TagRecordId, static (header, data) => new GeoKeyDirectoryTag(header, data));
        this.Register(VariableLengthRecordHeader.ProjectionUserId, GeoDoubleParamsTag.TagRecordId, static (header, data) => new GeoDoubleParamsTag(header, data));
        this.Register(VariableLengthRecordHeader.ProjectionUserId, GeoAsciiParamsTag.TagRecordId, static (header, data) => new GeoAsciiParamsTag(header, data));
        this.Register(VariableLengthRecordHeader.SpecUserId, ClassificationLookup.TagRecordId, static (header, data) => new ClassificationLookup(header, data));
        this.Register(VariableLengthRecordHeader.SpecUserId, TextAreaDescription.TagRecordId, static (header, data) => new TextAreaDescription(header, data));
#if LAS1_3_OR_GREATER
        for (var i = WaveformPacketDescriptor.MinTagRecordId; i <= WaveformPacketDescriptor.MaxTagRecordId; i++)
        {
            this.Register(VariableLengthRecordHeader.SpecUserId, i, static (header, data) => new WaveformPacketDescriptor(header, data));
        }

        this.Register(VariableLengthRecordHeader.SpecUserId, WaveformDataPackets.TagRecordId, static (header, _, _, data) => new WaveformDataPackets(header, data));
#endif
#if LAS1_4_OR_GREATER
        this.Register(VariableLengthRecordHeader.ProjectionUserId, OgcMathTransformWkt.TagRecordId, static (header, data) => new OgcMathTransformWkt(header, data));
        this.Register(VariableLengthRecordHeader.ProjectionUserId, OgcCoordinateSystemWkt.TagRecordId, static (header, data) => new OgcCoordinateSystemWkt(header, data));
        this.Register(VariableLengthRecordHeader.SpecUserId, ExtraBytes.TagRecordId, static (header, bytes) => new ExtraBytes(header, bytes));
        this.Register(VariableLengthRecordHeader.SpecUserId, Superseded.TagRecordId, static (header, _) => new Superseded(header));
#endif
    }

    /// <summary>
    /// Creates a <see cref="VariableLengthRecord"/>.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    /// <returns>The <see cref="VariableLengthRecord"/>.</returns>
    public delegate VariableLengthRecord CreateVariableLengthRecord(VariableLengthRecordHeader header, ReadOnlySpan<byte> data);

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Creates an <see cref="ExtendedVariableLengthRecord"/>.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="variableLengthRecords">The variable length records.</param>
    /// <param name="position">The position of the <paramref name="header"/>.</param>
    /// <param name="data">The data.</param>
    /// <returns>The <see cref="ExtendedVariableLengthRecord"/>.</returns>
    public delegate ExtendedVariableLengthRecord CreateExtendedVariableLengthRecord(ExtendedVariableLengthRecordHeader header, IEnumerable<VariableLengthRecord> variableLengthRecords, long position, ReadOnlySpan<byte> data);
#endif

    /// <summary>
    /// Registers the processor.
    /// </summary>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    public void Register(ushort recordId, CreateVariableLengthRecord processor) =>
#if NETSTANDARD2_1_OR_GREATER || NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        this.processors.AddOrUpdate(
            new(recordId),
            static (_, processor) => processor,
            static (_, _, processor) => processor,
            processor);
#else
        this.processors.AddOrUpdate(
            new(recordId),
            _ => processor,
            (_, _) => processor);
#endif

    /// <summary>
    /// Registers the processor.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    public void Register(string userId, ushort recordId, CreateVariableLengthRecord processor) =>
#if NETSTANDARD2_1_OR_GREATER || NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        this.processors.AddOrUpdate(
            new(userId, recordId),
            static (_, processor) => processor,
            static (_, _, processor) => processor,
            processor);
#else
        this.processors.AddOrUpdate(
            new(userId, recordId),
            _ => processor,
            (_, _) => processor);
#endif

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Registers the processor.
    /// </summary>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    public void Register(ushort recordId, CreateExtendedVariableLengthRecord processor) =>
#if NETSTANDARD2_1_OR_GREATER || NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        this.extendedProcessors.AddOrUpdate(
            new(recordId),
            static (_, processor) => processor,
            static (_, _, processor) => processor,
            processor);
#else
        this.extendedProcessors.AddOrUpdate(
            new(recordId),
            _ => processor,
            (_, _) => processor);
#endif

    /// <summary>
    /// Registers the processor.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    public void Register(string userId, ushort recordId, CreateExtendedVariableLengthRecord processor) =>
#if NETSTANDARD2_1_OR_GREATER || NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        this.extendedProcessors.AddOrUpdate(
            new(userId, recordId),
            static (_, processor) => processor,
            static (_, _, processor) => processor,
            processor);
#else
        this.extendedProcessors.AddOrUpdate(
            new(userId, recordId),
            _ => processor,
            (_, _) => processor);
#endif
#endif

    /// <summary>
    /// Tries to register the processor.
    /// </summary>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    /// <returns><see langword="true" /> when the processor is successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processor, in which case nothing gets added.</returns>
    public bool TryRegister(ushort recordId, CreateVariableLengthRecord processor) => this.processors.TryAdd(new(recordId), processor);

    /// <summary>
    /// Tries to register the processor.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    /// <returns><see langword="true" /> when the processor is successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processor, in which case nothing gets added.</returns>
    public bool TryRegister(string userId, ushort recordId, CreateVariableLengthRecord processor) => this.processors.TryAdd(new(userId, recordId), processor);

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Tries to register the processor.
    /// </summary>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    /// <returns><see langword="true" /> when the processor is successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processor, in which case nothing gets added.</returns>
    public bool TryRegister(ushort recordId, CreateExtendedVariableLengthRecord processor) => this.extendedProcessors.TryAdd(new(recordId), processor);

    /// <summary>
    /// Tries to register the processor.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    /// <returns><see langword="true" /> when the processor is successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processor, in which case nothing gets added.</returns>
    public bool TryRegister(string userId, ushort recordId, CreateExtendedVariableLengthRecord processor) => this.extendedProcessors.TryAdd(new(userId, recordId), processor);
#endif

    /// <summary>
    /// Processes the <see cref="VariableLengthRecord"/>.
    /// </summary>
    /// <param name="header">The header to process.</param>
    /// <param name="data">The data.</param>
    /// <returns>The processed record.</returns>
    public VariableLengthRecord Process(VariableLengthRecordHeader header, ReadOnlySpan<byte> data) => GetProcessor(this.processors, header.UserId, header.RecordId) is { } func
        ? func(header, data)
        : new UnknownVariableLengthRecord(header, data.ToArray());

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Processes the <see cref="ExtendedVariableLengthRecord"/>.
    /// </summary>
    /// <param name="header">The header to process.</param>
    /// <param name="records">The <see cref="VariableLengthRecord"/> instances.</param>
    /// <param name="position">The location of the record.</param>
    /// <param name="data">The data.</param>
    /// <returns>The processed record.</returns>
    public ExtendedVariableLengthRecord Process(ExtendedVariableLengthRecordHeader header, IEnumerable<VariableLengthRecord> records, long position, ReadOnlySpan<byte> data) => GetProcessor(this.extendedProcessors, header.UserId, header.RecordId) is { } func
        ? func(header, records, position, data)
        : new UnknownExtendedVariableLengthRecord(header, data.ToArray());
#endif

    private static T? GetProcessor<T>(System.Collections.Concurrent.ConcurrentDictionary<Key, T> dictionary, string userId, ushort recordId) =>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        dictionary.GetValueOrDefault(new(userId, recordId));
#else
        dictionary.TryGetValue(new(userId, recordId),  out var value) ? value : default;
#endif

    private readonly struct Key(string? userId, ushort recordId) : IEquatable<Key>, IEqualityComparer<Key>
    {
        public Key(ushort recordId)
            : this(default, recordId)
        {
        }

        private string? UserId { get; } = userId;

        private ushort RecordId { get; } = recordId;

        public override bool Equals(object? obj) => obj is Key key && this.Equals(key);

        public bool Equals(Key other) => (this.UserId is null || string.Equals(this.UserId, other.UserId, StringComparison.Ordinal))
                                         && this.RecordId == other.RecordId;

        public bool Equals(Key x, Key y) => (x.UserId is null || string.Equals(x.UserId, y.UserId, StringComparison.Ordinal))
                                            && x.RecordId == y.RecordId;

        public override int GetHashCode() => this.GetHashCode(this);

        public int GetHashCode(Key obj) => obj.RecordId.GetHashCode();
    }
}