// -----------------------------------------------------------------------
// <copyright file="HeaderBlockBuilder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The builder for <see cref="HeaderBlock"/> instances.
/// </summary>
public class HeaderBlockBuilder
{
#if LAS1_5_OR_GREATER
    private const double DefaultMinGps = double.MaxValue;
    private const double DefaultMaxGps = double.MinValue;
#endif
    private static readonly Vector3D DefaultScaleFactor = new(0.001);
    private static readonly Vector3D DefaultOffset = Vector3D.Zero;
    private static readonly Vector3D DefaultMin = new(double.MaxValue);
    private static readonly Vector3D DefaultMax = new(double.MinValue);

    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderBlockBuilder"/> class.
    /// </summary>
    public HeaderBlockBuilder()
        : this(HeaderBlock.DefaultVersion)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderBlockBuilder"/> class with the specified version.
    /// </summary>
    /// <param name="version">The version.</param>
    public HeaderBlockBuilder(Version version)
        : this(GetPointDataFormatId(version)) => this.Version = version;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderBlockBuilder"/> class with the specified point format ID.
    /// </summary>
    /// <param name="pointFormatId">The point format ID.</param>
    public HeaderBlockBuilder(byte pointFormatId)
    {
        this.PointDataFormatId = pointFormatId;
#if LAS1_4_OR_GREATER
        if (pointFormatId >= 6)
        {
            this.GlobalEncoding |= GlobalEncoding.Wkt;
        }
#endif
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderBlockBuilder"/> class.
    /// </summary>
    /// <param name="header">The header block.</param>
    public HeaderBlockBuilder(HeaderBlock header) => this.PopulateFromHeaderBlock(ref header);

    /// <summary>
    /// Gets or sets the file source id.
    /// </summary>
    /// <inheritdoc cref="HeaderBlock.FileSourceId" />
    public ushort FileSourceId { get; set; }

#if LAS1_2_OR_GREATER
    /// <summary>
    /// Gets or sets the global encoding.
    /// </summary>
    /// <inheritdoc cref="HeaderBlock.GlobalEncoding" />
    public GlobalEncoding GlobalEncoding { get; set; } = GlobalEncoding.StandardGpsTime;
#endif

    /// <summary>
    /// Gets or sets the project id.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the version that indicates the format number of the current specification itself.
    /// </summary>
    public Version Version { get; set; } = HeaderBlock.DefaultVersion;

    /// <summary>
    /// Gets or sets the system identifier.
    /// </summary>
    public string? SystemIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the generating software as ASCII data describing the generating software itself.
    /// </summary>
    public string? GeneratingSoftware { get; set; }

    /// <summary>
    /// Gets or sets the file creation.
    /// </summary>
    public DateTime? FileCreation { get; set; }

    /// <summary>
    /// Gets or sets the point data format id.
    /// </summary>
    /// <inheritdoc cref="HeaderBlock.PointDataFormatId" />
    public byte PointDataFormatId
    {
        get => (byte)(this.PointDataFormat & 0x3F);
        set => this.PointDataFormat = (byte)((this.PointDataFormat & ~0x3F) | (value & 0x3F));
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets or sets the legacy total number of point records within the file.
    /// </summary>
    public uint LegacyNumberOfPointRecords { get; set; }

    /// <inheritdoc cref="HeaderBlock.LegacyNumberOfPointsByReturn" />
    public uint[] LegacyNumberOfPointsByReturn { get; } = new uint[5];
#else
    /// <summary>
    /// Gets or sets the total number of point records within the file.
    /// </summary>
    public uint NumberOfPointRecords { get; set; }

    /// <inheritdoc cref="HeaderBlock.NumberOfPointsByReturn" />
    public uint[] NumberOfPointsByReturn { get; } = new uint[5];
#endif

    /// <summary>
    /// Gets or sets the scale factor.
    /// </summary>
    /// <inheritdoc cref="HeaderBlock.ScaleFactor" />
    public Vector3D ScaleFactor { get; set; } = DefaultScaleFactor;

    /// <summary>
    /// Gets or sets the offset.
    /// </summary>
    /// <inheritdoc cref="HeaderBlock.Offset" />
    public Vector3D Offset { get; set; } = DefaultOffset;

    /// <summary>
    /// Gets or sets the min un-scaled extents of the LAS point file data, specified in the coordinate system of the LAS data.
    /// </summary>
    public Vector3D Min { get; set; } = DefaultMin;

    /// <summary>
    /// Gets or sets the max un-scaled extents of the LAS point file data, specified in the coordinate system of the LAS data.
    /// </summary>
    public Vector3D Max { get; set; } = DefaultMax;

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets or sets the total number of point records within the file.
    /// </summary>
    public ulong NumberOfPointRecords { get; set; }

    /// <inheritdoc cref="HeaderBlock.NumberOfPointsByReturn" />
    public ulong[] NumberOfPointsByReturn { get; } = new ulong[15];
#endif

#if LAS1_5_OR_GREATER
    /// <summary>
    /// Gets or sets the minimum GPS time.
    /// </summary>
    public double MinGpsTime { get; set; } = DefaultMinGps;

    /// <summary>
    /// Gets or sets the maximum GPS time.
    /// </summary>
    public double MaxGpsTime { get; set; } = DefaultMaxGps;

    /// <summary>
    /// Gets or sets the time offset.
    /// </summary>
    public ushort TimeOffset { get; set; }
#endif

    /// <summary>
    /// Gets the header block.
    /// </summary>
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    public HeaderBlock HeaderBlock
    {
        get
        {
            var header = this.CreateHeaderBlock();
            this.PopulateFromHeaderBlock(ref header, keepMinMax: true);
            return header;
        }
    }

    /// <summary>
    /// Gets or sets the point data format.
    /// </summary>
    internal byte PointDataFormat { get; set; }

#if LAS1_5_OR_GREATER
    /// <summary>
    /// Gets the suggested time offset for the current date.
    /// </summary>
    /// <returns>The suggested time offset.</returns>
    public static ushort GetSuggestedTimeOffset() => GpsTime.GetTimeOffset();

    /// <summary>
    /// Gets the suggested time offset for the specified date.
    /// </summary>
    /// <param name="dateTime">The date.</param>
    /// <returns>The suggested time offset.</returns>
    public static ushort GetSuggestedTimeOffset(DateTime dateTime) => GpsTime.GetTimeOffset(dateTime.Year);

    /// <summary>
    /// Gets the suggested time offset for the specified year.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <returns>The suggested time offset.</returns>
    public static ushort GetSuggestedTimeOffset(int year) => GpsTime.GetTimeOffset(year);
#endif

    /// <summary>
    /// Creates a <see cref="HeaderBlockBuilder"/> from the specified typeof <see cref="PointDataFormat"/>.
    /// </summary>
    /// <typeparam name="T">The type of point data record.</typeparam>
    /// <returns>The header block builder.</returns>
    /// <exception cref="InvalidOperationException">Invalid point data type.</exception>
#if NET7_0_OR_GREATER
    public static HeaderBlockBuilder FromPointType<T>()
#else
    public static HeaderBlockBuilder FromPointType<[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicFields)] T>()
#endif
        where T : IBasePointDataRecord =>
#if NET7_0_OR_GREATER
        new(T.Id);
#else
        System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(T)).GetDeclaredField(nameof(PointDataRecord.Id))?.GetValue(null) is byte pointTypeId
            ? new(pointTypeId)
            : throw new InvalidOperationException();
#endif

    /// <summary>
    /// Sets the offset using the specified coordinates and precision.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <param name="horizontalPrecision">The horizontal position.</param>
    /// <param name="verticalPrecision">The vertical position.</param>
    public void SetOffset(double x, double y, double z, double horizontalPrecision = 1000, double verticalPrecision = 100) =>
        this.Offset = new(
            Math.Truncate(x / horizontalPrecision) * horizontalPrecision,
            Math.Truncate(y / horizontalPrecision) * horizontalPrecision,
            Math.Truncate(z / verticalPrecision) * verticalPrecision);

    /// <summary>
    /// Adds a point to the header block.
    /// </summary>
    /// <param name="point">The point.</param>
    public void Add(IBasePointDataRecord point)
    {
        var (x, y, z) = PointDataRecordQuantizer.Get(point, this.ScaleFactor, this.Offset);
#if LAS1_5_OR_GREATER
        if (point is IGpsPointDataRecord gpsPointDataRecord)
        {
            this.Add(x, y, z, gpsPointDataRecord.GpsTime, point.ReturnNumber);
        }
        else
        {
            this.Add(x, y, z, point.ReturnNumber);
        }
#else
        this.Add(x, y, z, point.ReturnNumber);
#endif
    }

    /// <summary>
    /// Adds the points to the header block.
    /// </summary>
    /// <typeparam name="T">The type of point.</typeparam>
    /// <param name="points">The points.</param>
    public void Add<T>(IEnumerable<T> points)
        where T : IBasePointDataRecord
    {
        foreach (var point in points)
        {
            this.Add(point);
        }
    }

#if LAS1_5_OR_GREATER
    /// <summary>
    /// Adds a point to the header block.
    /// </summary>
    /// <param name="point">The point.</param>
    public void Add(IGpsPointDataRecord point)
    {
        var (x, y, z) = PointDataRecordQuantizer.Get(point, this.ScaleFactor, this.Offset);
        this.Add(x, y, z, point.GpsTime, point.ReturnNumber);
    }
#endif

    /// <summary>
    /// Adds a point to the header block.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <param name="returnNumber">The return number.</param>
    public void Add(double x, double y, double z, int returnNumber = 1) => this.Add(this.Truncate(x, y, z), returnNumber);

#if LAS1_5_OR_GREATER
    /// <summary>
    /// Adds a point to the header block.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <param name="gpsTime">The gps time.</param>
    /// <param name="returnNumber">The return number.</param>
    public void Add(double x, double y, double z, double gpsTime, int returnNumber = 1) => this.Add(this.Truncate(x, y, z), gpsTime, returnNumber);
#endif

    /// <summary>
    /// Adds a point to the header block.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="returnNumber">The return number.</param>
    public void Add(Vector3D point, int returnNumber = 1)
    {
        this.Min = Vector3D.Min(this.Min, point);
        this.Max = Vector3D.Max(this.Max, point);

#if LAS1_4_OR_GREATER
        if (this.PointDataFormat >= 6)
        {
            this.NumberOfPointRecords++;
            if (returnNumber >= 1)
            {
                this.NumberOfPointsByReturn[returnNumber - 1]++;
            }
        }
        else
        {
            this.LegacyNumberOfPointRecords++;
            if (returnNumber >= 1)
            {
                this.LegacyNumberOfPointsByReturn[returnNumber - 1]++;
            }
        }
#else
        this.NumberOfPointRecords++;
        if (returnNumber >= 1)
        {
            this.NumberOfPointsByReturn[returnNumber - 1]++;
        }
#endif
    }

#if LAS1_5_OR_GREATER
    /// <summary>
    /// Adds a point to the header block.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="gpsTime">The GPS time.</param>
    /// <param name="returnNumber">The return number.</param>
    public void Add(Vector3D point, DateTime gpsTime, int returnNumber = 1) => this.Add(point, this.GetGpsTime(gpsTime), returnNumber);
#endif

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        this.Offset = DefaultOffset;
        this.ScaleFactor = DefaultScaleFactor;
        this.Min = DefaultMin;
        this.Max = DefaultMax;
#if LAS1_5_OR_GREATER
        this.MinGpsTime = DefaultMinGps;
        this.MaxGpsTime = DefaultMaxGps;
#endif
#if LAS1_4_OR_GREATER
        this.LegacyNumberOfPointRecords = default;
        for (var i = 0; i < this.LegacyNumberOfPointsByReturn.Length; i++)
        {
            this.LegacyNumberOfPointsByReturn[i] = default;
        }
#endif

        this.NumberOfPointRecords = default;
        for (var i = 0; i < this.NumberOfPointsByReturn.Length; i++)
        {
            this.NumberOfPointsByReturn[i] = default;
        }
    }

    private static byte GetPointDataFormatId(Version version) => version switch
    {
#if LAS1_4_OR_GREATER
        { Major: 1, Minor: >= 5 } => 6, // ExtendedGpsPointDataRecord.Id,
#endif
        { Major: 1 } => PointDataRecord.Id,
        _ => throw new InvalidOperationException(),
    };

#if LAS1_5_OR_GREATER
    private void Add(Vector3D point, double gpsTime, int returnNumber = 1)
    {
        this.Add(point, returnNumber);
        this.MaxGpsTime = Math.Max(this.MaxGpsTime, gpsTime);
        this.MinGpsTime = Math.Min(this.MinGpsTime, gpsTime);
    }
#endif

    private Vector3D Truncate(double x, double y, double z)
    {
        return new(TruncateToScale(x, this.ScaleFactor.X), TruncateToScale(y, this.ScaleFactor.Y), TruncateToScale(z, this.ScaleFactor.Y));

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static double TruncateToScale(double value, double scale)
        {
            return Math.Truncate(value / scale) * scale;
        }
    }

#if LAS1_5_OR_GREATER
    private double GetGpsTime(DateTime gpsTime)
    {
        return HasFlag(this.GlobalEncoding, GlobalEncoding.StandardGpsTime)
            ? GetAdjusted(gpsTime)
            : GpsTime.DateTimeToGpsWeekTime(gpsTime, 0);

        double GetAdjusted(DateTime value)
        {
            return HasFlag(this.GlobalEncoding, GlobalEncoding.TimeOffsetFlag)
                ? GpsTime.DateTimeToTimeOffsetGpsTime(value, this.TimeOffset)
                : GpsTime.DateTimeToAdjustedStandardGpsTime(value);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static bool HasFlag(GlobalEncoding globalEncoding, GlobalEncoding flag)
        {
            return (globalEncoding & flag) == flag;
        }
    }
#endif

    private HeaderBlock CreateHeaderBlock()
    {
#if LAS1_4_OR_GREATER
        var legacyNumberOfPointRecords = this.LegacyNumberOfPointRecords;
        var legacyNumberOfPointsByReturn = this.LegacyNumberOfPointsByReturn;

        if (this.PointDataFormatId >= 6)
        {
            // set these to zeros
            legacyNumberOfPointRecords = default;
            legacyNumberOfPointsByReturn = new uint[5];
        }
        else if (legacyNumberOfPointRecords is 0)
        {
            legacyNumberOfPointRecords = (uint)this.NumberOfPointRecords;
            legacyNumberOfPointsByReturn = new uint[5];
            for (var i = 0; i < 5; i++)
            {
                legacyNumberOfPointsByReturn[i] = (uint)this.NumberOfPointsByReturn[i];
            }
        }

        var numberOfPointRecords = this.NumberOfPointRecords;
        var numberOfPointsByReturn = this.NumberOfPointsByReturn;
        if (numberOfPointRecords is 0)
        {
            numberOfPointRecords = this.LegacyNumberOfPointRecords;
            numberOfPointsByReturn = new ulong[15];
            for (var i = 0; i < 5; i++)
            {
                numberOfPointsByReturn[i] = this.LegacyNumberOfPointsByReturn[i];
            }
        }
#else
        var numberOfPointRecords = this.NumberOfPointRecords;
        var numberOfPointsByReturn = this.NumberOfPointsByReturn;
#endif

        var min = this.Min == DefaultMin ? Vector3D.Zero : this.Min;
        var max = this.Max == DefaultMax ? Vector3D.Zero : this.Max;
#if LAS1_5_OR_GREATER
        var maxGpsTime = this.MaxGpsTime.Equals(double.MinValue) ? default : this.MaxGpsTime;
        var minGpsTime = this.MinGpsTime.Equals(double.MaxValue) ? default : this.MinGpsTime;
#endif

        return new(
            this.FileSourceId,
#if LAS1_5_OR_GREATER
            (GlobalEncoding)BitManipulation.Get((byte)this.GlobalEncoding, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3 | Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5 | Constants.BitMasks.Mask6),
#elif LAS1_4_OR_GREATER
            (GlobalEncoding)BitManipulation.Get((byte)this.GlobalEncoding, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3 | Constants.BitMasks.Mask4),
#elif LAS1_3_OR_GREATER
            (GlobalEncoding)BitManipulation.Get((byte)this.GlobalEncoding, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3),
#elif LAS1_2_OR_GREATER
            (GlobalEncoding)BitManipulation.Get((byte)this.GlobalEncoding, Constants.BitMasks.Mask0),
#endif
            this.ProjectId,
            this.Version,
            this.SystemIdentifier,
            this.GeneratingSoftware,
            this.FileCreation,
            this.PointDataFormat,
#if LAS1_4_OR_GREATER
            legacyNumberOfPointRecords,
            legacyNumberOfPointsByReturn,
#else
            numberOfPointRecords,
            numberOfPointsByReturn,
#endif
            this.ScaleFactor,
            this.Offset,
#if LAS1_4_OR_GREATER
            numberOfPointRecords,
            numberOfPointsByReturn,
#endif
#if LAS1_5_OR_GREATER
            maxGpsTime,
            minGpsTime,
            this.TimeOffset,
#endif
            min,
            max);
    }

    private void PopulateFromHeaderBlock(ref HeaderBlock header, bool keepMinMax = false)
    {
        this.FileSourceId = header.FileSourceId;
#if LAS1_2_OR_GREATER
        this.GlobalEncoding = header.GlobalEncoding;
#endif
        this.ProjectId = header.ProjectId;
        this.Version = header.Version;
        this.SystemIdentifier = header.SystemIdentifier;
        this.GeneratingSoftware = header.GeneratingSoftware;
        this.FileCreation = header.FileCreation;
        this.PointDataFormat = header.PointDataFormat;
#if LAS1_4_OR_GREATER
        this.LegacyNumberOfPointRecords = header.LegacyNumberOfPointRecords;
        for (var i = 0; i < 5; i++)
        {
            this.LegacyNumberOfPointsByReturn[i] = header.LegacyNumberOfPointsByReturn[i];
        }
#else
        this.NumberOfPointRecords = header.NumberOfPointRecords;
        for (var i = 0; i < 5; i++)
        {
            this.NumberOfPointsByReturn[i] = header.NumberOfPointsByReturn[i];
        }
#endif

        this.ScaleFactor = header.ScaleFactor;
        this.Offset = header.Offset;
        if (!keepMinMax)
        {
            this.Min = header.Min;
            this.Max = header.Max;
        }

#if LAS1_4_OR_GREATER
        this.NumberOfPointRecords = header.RawNumberOfPointRecords;
        for (var i = 0; i < 15; i++)
        {
            this.NumberOfPointsByReturn[i] = header.RawNumberOfPointsByReturn[i];
        }
#endif

#if LAS1_5_OR_GREATER
        if (!keepMinMax)
        {
            this.MaxGpsTime = header.MaxGpsTime;
            this.MinGpsTime = header.MinGpsTime;
        }

        this.TimeOffset = header.TimeOffset;
#endif
    }
}