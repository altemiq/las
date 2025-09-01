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

    private readonly Dictionary<(string? UserId, ushort RecordId), CreateVariableLengthRecord> processors = [];

#if LAS1_3_OR_GREATER
    private readonly Dictionary<(string? UserId, ushort RecordId), CreateExtendedVariableLengthRecord> extendedProcessors = [];
#endif

    private VariableLengthRecordProcessor()
    {
        this.Register(VariableLengthRecordHeader.ProjectionUserId, GeoKeyDirectoryTag.TagRecordId, (header, data) => new GeoKeyDirectoryTag(header, data));
        this.Register(VariableLengthRecordHeader.ProjectionUserId, GeoDoubleParamsTag.TagRecordId, (header, data) => new GeoDoubleParamsTag(header, data));
        this.Register(VariableLengthRecordHeader.ProjectionUserId, GeoAsciiParamsTag.TagRecordId, (header, data) => new GeoAsciiParamsTag(header, data));
        this.Register(VariableLengthRecordHeader.SpecUserId, ClassificationLookup.TagRecordId, (header, data) => new ClassificationLookup(header, data));
        this.Register(VariableLengthRecordHeader.SpecUserId, TextAreaDescription.TagRecordId, (header, data) => new TextAreaDescription(header, data));
#if LAS1_3_OR_GREATER
        for (var i = WaveformPacketDescriptor.MinTagRecordId; i <= WaveformPacketDescriptor.MaxTagRecordId; i++)
        {
            this.Register(VariableLengthRecordHeader.SpecUserId, i, (header, data) => new WaveformPacketDescriptor(header, data));
        }

        this.Register(VariableLengthRecordHeader.SpecUserId, WaveformDataPackets.TagRecordId, (header, _, _, data) => new WaveformDataPackets(header, data));
#endif
#if LAS1_4_OR_GREATER
        this.Register(VariableLengthRecordHeader.ProjectionUserId, OgcMathTransformWkt.TagRecordId, (header, data) => new OgcMathTransformWkt(header, data));
        this.Register(VariableLengthRecordHeader.ProjectionUserId, OgcCoordinateSystemWkt.TagRecordId, (header, data) => new OgcCoordinateSystemWkt(header, data));
        this.Register(VariableLengthRecordHeader.SpecUserId, ExtraBytes.TagRecordId, (header, bytes) => new ExtraBytes(header, bytes));
        this.Register(VariableLengthRecordHeader.SpecUserId, Superseded.TagRecordId, (header, _) => new Superseded(header));
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
    public void Register(ushort recordId, CreateVariableLengthRecord processor) => this.processors.Add((default, recordId), processor);

    /// <summary>
    /// Registers the processor.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    public void Register(string userId, ushort recordId, CreateVariableLengthRecord processor) => this.processors.Add((userId, recordId), processor);

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Registers the processor.
    /// </summary>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    public void Register(ushort recordId, CreateExtendedVariableLengthRecord processor) => this.extendedProcessors.Add((default, recordId), processor);

    /// <summary>
    /// Registers the processor.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    public void Register(string userId, ushort recordId, CreateExtendedVariableLengthRecord processor) => this.extendedProcessors.Add((userId, recordId), processor);
#endif

    /// <summary>
    /// Tries to register the processor.
    /// </summary>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    /// <returns><see langword="true" /> when the processor is successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processor, in which case nothing gets added.</returns>
    public bool TryRegister(ushort recordId, CreateVariableLengthRecord processor) => TryAdd(this.processors, (default, recordId), processor);

    /// <summary>
    /// Tries to register the processor.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    /// <returns><see langword="true" /> when the processor is successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processor, in which case nothing gets added.</returns>
    public bool TryRegister(string userId, ushort recordId, CreateVariableLengthRecord processor) => TryAdd(this.processors, (userId, recordId), processor);

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Tries to register the processor.
    /// </summary>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    /// <returns><see langword="true" /> when the processor is successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processor, in which case nothing gets added.</returns>
    public bool TryRegister(ushort recordId, CreateExtendedVariableLengthRecord processor) => TryAdd(this.extendedProcessors, (default, recordId), processor);

    /// <summary>
    /// Tries to register the processor.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="recordId">The record ID.</param>
    /// <param name="processor">The processor.</param>
    /// <returns><see langword="true" /> when the processor is successfully added to the dictionary; <see langword="false" /> when the dictionary already contains the processor, in which case nothing gets added.</returns>
    public bool TryRegister(string userId, ushort recordId, CreateExtendedVariableLengthRecord processor) => TryAdd(this.extendedProcessors, (userId, recordId), processor);
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

    private static T? GetProcessor<T>(IDictionary<(string? UserId, ushort RecordId), T> dictionary, string userId, ushort recordId)
    {
        return dictionary.FirstOrDefault(IsValid).Value;

        bool IsValid(KeyValuePair<(string? UserId, ushort RecordId), T> kvp)
        {
            return IsValidCore(kvp.Key, userId, recordId);

            static bool IsValidCore((string? UserId, ushort RecordId) key, string userId, ushort recordId)
            {
                return (key.UserId is null || string.Equals(key.UserId, userId, StringComparison.Ordinal))
                    && key.RecordId == recordId;
            }
        }
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
    private static bool TryAdd<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull => dictionary.TryAdd(key, value);
#else
    private static bool TryAdd<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
        {
            return false;
        }

        dictionary.Add(key, value);
        return true;
    }
#endif
}