// -----------------------------------------------------------------------
// <copyright file="LasZip.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The LAS ZIP class.
/// </summary>
internal sealed class LasZip
{
    /// <summary>
    /// The variable chunk size.
    /// </summary>
    public const uint VariableChunkSize = uint.MaxValue;

    /// <summary>
    /// Get the current version.
    /// </summary>
    public static readonly Version Version = new(3, 4, 0, 3);

    private const int GpsItemSize = sizeof(double);

#if LAS1_2_OR_GREATER
    private const int ColorItemSize = 3 * sizeof(ushort);
#endif

#if LAS1_3_OR_GREATER
    private const int WavePacketItemSize = sizeof(byte) + sizeof(ulong) + sizeof(uint) + (4 * sizeof(float));
#endif

#if LAS1_4_OR_GREATER
    private const int ColorNearInfraredItemSize = ColorItemSize + sizeof(ushort);
#endif

    private const ushort DefaultChunkSize = 50000;

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Initializes a new instance of the <see cref="LasZip"/> class.
    /// </summary>
    /// <param name="pointDataFormatId">The point data format ID.</param>
    /// <param name="extraByteCount">The number of extra bytes.</param>
    /// <param name="compressor">The compressor.</param>
    /// <param name="requestedVersion">The requested version.</param>
    public LasZip(byte pointDataFormatId, ushort extraByteCount, Compressor compressor, ushort requestedVersion = 2)
    {
        var items = GetItems(pointDataFormatId, requestedVersion).ToList();
        var point14 = items[0].IsType(LasItemType.Point14);

        // add the extra bytes
        if (extraByteCount is not 0)
        {
            items.Add(new()
            {
                Type = point14 ? LasItemType.Byte14 : LasItemType.Byte,
                Size = extraByteCount,
                Version = point14 ? (ushort)3 : requestedVersion,
            });
        }

        this.Compressor = (compressor, point14) switch
        {
            (not Compressor.None and not Compressor.LayeredChunked, true) => throw new ArgumentException(Compression.Properties.v1_4.Resources.MustUseLayeredChunkedCompression, nameof(compressor)),
            (Compressor.LayeredChunked, false) => Compressor.PointWiseChunked,
            var (c, _) => c,
        };

        if (this.Compressor is not Compressor.PointWise)
        {
            this.ChunkSize = DefaultChunkSize;
        }

        this.Items = items;
    }
#else
    /// <summary>
    /// Initializes a new instance of the <see cref="LasZip"/> class.
    /// </summary>
    /// <param name="pointDataFormatId">The point data format ID.</param>
    /// <param name="compressor">The compressor.</param>
    /// <param name="requestedVersion">The requested version.</param>
    public LasZip(byte pointDataFormatId, Compressor compressor, ushort requestedVersion = 2)
    {
        var items = GetItems(pointDataFormatId, requestedVersion).ToList();

        this.Compressor = compressor;
        if (compressor is not Compressor.PointWise)
        {
            this.ChunkSize = DefaultChunkSize;
        }

        this.Items = items;
    }
#endif

    private LasZip()
    {
    }

    /// <summary>
    /// Gets the compressor.
    /// </summary>
    public Compressor Compressor { get; private init; }

    /// <summary>
    /// Gets the coder.
    /// </summary>
    public Coder Coder { get; private init; }

    /// <summary>
    /// Gets the options.
    /// </summary>
    public uint Options { get; private set; }

    /// <summary>
    /// Gets the chunk size.
    /// </summary>
    public uint ChunkSize { get; private set; }

    /// <summary>
    /// Gets the items.
    /// </summary>
    public IReadOnlyCollection<LasItem> Items { get; private init; } = [];

    /// <summary>
    /// Creates a new instance of <see cref="LasZip"/> from a <see cref="CompressedTag"/>.
    /// </summary>
    /// <param name="tag">The compressed tag.</param>
    /// <returns>The new instance of <see cref="LasZip"/>.</returns>
    public static LasZip From(CompressedTag tag)
    {
        var items = new LasItem[tag.Count];
        for (var i = 0; i < items.Length; i++)
        {
            items[i] = tag[i];
        }

        return new()
        {
            Compressor = tag.Compressor,
            Coder = tag.Coder,
            Options = tag.Options,
            ChunkSize = (uint)tag.ChunkSize,
            Items = items,
        };
    }

    /// <summary>
    /// Get valid version for the specified point data format ID.
    /// </summary>
    /// <param name="pointDataFormatId">The point data format ID.</param>
    /// <returns>The valid version.</returns>
    public static ushort GetValidVersion(byte pointDataFormatId) => pointDataFormatId switch
    {
        >= 6 => 3,
        _ => 2,
    };

    /// <summary>
    /// Checks this instance.
    /// </summary>
    /// <param name="pointSize">The point size.</param>
    /// <exception cref="InvalidOperationException">The compressor is invalid.</exception>
    /// <exception cref="InvalidOperationException">The coder is invalid.</exception>
    /// <exception cref="InvalidOperationException">The point size is invalid.</exception>
    public void Validate(ushort pointSize)
    {
        if (!IsDefined(this.Compressor))
        {
            throw new InvalidOperationException($"Invalid {nameof(Las.Compressor)} value: {this.Compressor}");
        }

        if (!IsDefined(this.Coder))
        {
            throw new InvalidOperationException($"Invalid {nameof(Las.Coder)} value: {this.Coder}");
        }

        ValidateItems(this.Items, pointSize);

        static bool IsDefined<TEnum>(TEnum value)
            where TEnum : struct, Enum
        {
#if NET5_0_OR_GREATER
            return Enum.IsDefined<TEnum>(value);
#else
            return Enum.IsDefined(typeof(TEnum), value);
#endif
        }

        static void ValidateItems(IEnumerable<LasItem> items, ushort pointSize)
        {
            var size = default(ushort);
            foreach (var item in items)
            {
                ValidateItem(item);
                size += item.Size;
            }

            if (pointSize is not 0 && (pointSize != size))
            {
                throw new InvalidOperationException($"point has size of {pointSize} but items only add up to {size} bytes");
            }

            static void ValidateItem(LasItem item)
            {
                switch (item)
                {
                    case { Type: LasItemType.Point10, Size: not PointDataRecord.Size }:
                        throw new InvalidOperationException($"{item.Name} has size != {PointDataRecord.Size}");
                }

                switch (item)
                {
                    case { Type: LasItemType.Point10, Size: not PointDataRecord.Size }: throw new InvalidOperationException($"{item.Name} has size != {PointDataRecord.Size}");
                    case { Type: LasItemType.Point10, Version: < 2 }: throw new InvalidOperationException($"{item.Name} has version < 2");
                    case { Type: LasItemType.Point10, Version: > 2 }: throw new InvalidOperationException($"{item.Name} has version > 2");
                    case { Type: LasItemType.Point10 }: break;
                    case { Type: LasItemType.GpsTime11, Size: not GpsItemSize }: throw new InvalidOperationException($"{item.Name} has size != {GpsItemSize}");
                    case { Type: LasItemType.GpsTime11, Version: < 2 }: throw new InvalidOperationException($"{item.Name} has version < 2");
                    case { Type: LasItemType.GpsTime11, Version: > 2 }: throw new InvalidOperationException($"{item.Name} has version > 2");
                    case { Type: LasItemType.GpsTime11 }: break;
#if LAS1_2_OR_GREATER
                    case { Type: LasItemType.Rgb12, Size: not ColorItemSize }: throw new InvalidOperationException($"{item.Name} has size != {ColorItemSize}");
                    case { Type: LasItemType.Rgb12, Version: < 2 }: throw new InvalidOperationException($"{item.Name} has version < 2");
                    case { Type: LasItemType.Rgb12, Version: > 2 }: throw new InvalidOperationException($"{item.Name} has version > 2");
                    case { Type: LasItemType.Rgb12 }: break;
#endif
                    case { Type: LasItemType.Byte, Size: < sizeof(byte) }: throw new InvalidOperationException($"{item.Name} has size < {sizeof(byte)}");
                    case { Type: LasItemType.Byte, Version: < 2 }: throw new InvalidOperationException($"{item.Name} has version < 2");
                    case { Type: LasItemType.Byte, Version: > 2 }: throw new InvalidOperationException($"{item.Name} has version > 2");
                    case { Type: LasItemType.Byte }: break;
#if LAS1_3_OR_GREATER
                    case { Type: LasItemType.WavePacket13, Size: not WavePacketItemSize }: throw new InvalidOperationException($"{item.Name} has size != {WavePacketItemSize}");
                    case { Type: LasItemType.WavePacket13, Version: > 1 }: throw new InvalidOperationException($"{item.Name} has version > 1");
                    case { Type: LasItemType.WavePacket13 }: break;
#endif
#if LAS1_4_OR_GREATER
                    case { Type: LasItemType.Point14, Size: not ExtendedGpsPointDataRecord.Size }: throw new InvalidOperationException($"{item.Name} has size != {ExtendedGpsPointDataRecord.Size}");
                    case { Type: LasItemType.Point14, Version: not 0 and not 2 and not 3 and not 4 }: throw new InvalidOperationException($"{item.Name} has version != 0 and != 2 and != 3 and != 4");
                    case { Type: LasItemType.Point14 }: break;
                    case { Type: LasItemType.Rgb14, Size: not ColorItemSize }: throw new InvalidOperationException($"{item.Name} has size != {ColorItemSize}");
                    case { Type: LasItemType.Rgb14, Version: not 0 and not 2 and not 3 and not 4 }: throw new InvalidOperationException($"{item.Name} has version != 0 and != 2 and != 3 and != 4");
                    case { Type: LasItemType.Rgb14 }: break;
                    case { Type: LasItemType.RgbNir14, Size: not ColorNearInfraredItemSize }: throw new InvalidOperationException($"{item.Name} has size != {ColorNearInfraredItemSize}");
                    case { Type: LasItemType.RgbNir14, Version: not 0 and not 2 and not 3 and not 4 }: throw new InvalidOperationException($"{item.Name} has version != 0 and != 2 and != 3 and != 4");
                    case { Type: LasItemType.RgbNir14 }: break;
                    case { Type: LasItemType.Byte14, Size: < sizeof(byte) }: throw new InvalidOperationException($"{item.Name} has size < {sizeof(byte)}");
                    case { Type: LasItemType.Byte14, Version: not 0 and not 2 and not 3 and not 4 }: throw new InvalidOperationException($"{item.Name} has version != 0 and != 2 and != 3 and != 4");
                    case { Type: LasItemType.Byte14 }: break;
                    case { Type: LasItemType.WavePacket14, Size: not WavePacketItemSize }: throw new InvalidOperationException($"{item.Name}  has size != {WavePacketItemSize}");
                    case { Type: LasItemType.WavePacket14, Version: not 0 and not 3 and not 4 }: throw new InvalidOperationException($"{item.Name}  has version != 0 and != 3 and != 4");
                    case { Type: LasItemType.WavePacket14 }: break;
#endif
                    default: throw new InvalidOperationException($"item unknown ({item.Type},{item.Size},{item.Version}");
                }
            }
        }
    }

    private static IEnumerable<LasItem> GetItems(byte pointType, ushort requestedVersion) => (pointType, requestedVersion) switch
    {
        (PointDataRecord.Id, 2) =>
        [
            new() { Type = LasItemType.Point10, Size = PointDataRecord.Size, Version = requestedVersion },
        ],
        (GpsPointDataRecord.Id, 2) =>
        [
            new() { Type = LasItemType.Point10, Size = PointDataRecord.Size, Version = requestedVersion },
            new() { Type = LasItemType.GpsTime11, Size = GpsItemSize, Version = requestedVersion },
        ],
#if LAS1_2_OR_GREATER
        (ColorPointDataRecord.Id, 2) =>
        [
            new() { Type = LasItemType.Point10, Size = PointDataRecord.Size, Version = requestedVersion },
            new() { Type = LasItemType.Rgb12, Size = ColorItemSize, Version = requestedVersion },
        ],
        (GpsColorPointDataRecord.Id, 2) =>
        [
            new() { Type = LasItemType.Point10, Size = PointDataRecord.Size, Version = requestedVersion },
            new() { Type = LasItemType.GpsTime11, Size = GpsItemSize, Version = requestedVersion },
            new() { Type = LasItemType.Rgb12, Size = ColorItemSize, Version = requestedVersion },
        ],
#endif
#if LAS1_3_OR_GREATER
        (GpsWaveformPointDataRecord.Id, 2) =>
        [
            new() { Type = LasItemType.Point10, Size = PointDataRecord.Size, Version = requestedVersion },
            new() { Type = LasItemType.GpsTime11, Size = GpsItemSize, Version = requestedVersion },
            new() { Type = LasItemType.WavePacket13, Size = WavePacketItemSize, Version = 1 },
        ],
        (GpsColorWaveformPointDataRecord.Id, 2) =>
        [
            new() { Type = LasItemType.Point10, Size = PointDataRecord.Size, Version = requestedVersion },
            new() { Type = LasItemType.GpsTime11, Size = GpsItemSize, Version = requestedVersion },
            new() { Type = LasItemType.Rgb12, Size = ColorItemSize, Version = requestedVersion },
            new() { Type = LasItemType.WavePacket13, Size = WavePacketItemSize, Version = 1 },
        ],
#endif
#if LAS1_4_OR_GREATER
        (ExtendedGpsPointDataRecord.Id, 0 or 3 or 4) =>
        [
            new() { Type = LasItemType.Point14, Size = ExtendedGpsPointDataRecord.Size, Version = 3 },
        ],
        (ExtendedGpsColorPointDataRecord.Id, 0 or 3 or 4) =>
        [
            new() { Type = LasItemType.Point14, Size = ExtendedGpsPointDataRecord.Size, Version = 3 },
            new() { Type = LasItemType.Rgb14, Size = ColorItemSize, Version = 3 },
        ],
        (ExtendedGpsColorNearInfraredPointDataRecord.Id, 0 or 3 or 4) =>
        [
            new() { Type = LasItemType.Point14, Size = ExtendedGpsPointDataRecord.Size, Version = 3 },
            new() { Type = LasItemType.RgbNir14, Size = ColorNearInfraredItemSize, Version = 3 },
        ],
        (ExtendedGpsWaveformPointDataRecord.Id, 0 or 3 or 4) =>
        [
            new() { Type = LasItemType.Point14, Size = ExtendedGpsPointDataRecord.Size, Version = 3 },
            new() { Type = LasItemType.WavePacket14, Size = WavePacketItemSize, Version = 3 },
        ],
        (ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id, 0 or 3 or 4) =>
        [
            new() { Type = LasItemType.Point14, Size = ExtendedGpsPointDataRecord.Size, Version = 3 },
            new() { Type = LasItemType.RgbNir14, Size = ColorNearInfraredItemSize, Version = 3 },
            new() { Type = LasItemType.WavePacket14, Size = WavePacketItemSize, Version = 3 },
        ],
#endif
#if LAS1_4_OR_GREATER
        _ => throw new NotSupportedException(Properties.v1_4.Resources.OnlyDataPointsAreAllowed),
#elif LAS1_3_OR_GREATER
        _ => throw new NotSupportedException(Properties.v1_3.Resources.OnlyDataPointsAreAllowed),
#elif LAS1_2_OR_GREATER
        _ => throw new NotSupportedException(Properties.v1_2.Resources.OnlyDataPointsAreAllowed),
#else
        _ => throw new NotSupportedException(Properties.v1_1.Resources.OnlyDataPointsAreAllowed),
#endif

    };
}