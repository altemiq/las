// -----------------------------------------------------------------------
// <copyright file="HeaderBlock.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents the LAS Header Block.
/// </summary>
public readonly struct HeaderBlock : IEquatable<HeaderBlock>
{
    /// <summary>
    /// Gets the default version.
    /// </summary>
    public static readonly Version DefaultVersion = new(1, MaxMinorVersion);

    /// <summary>
    /// Gets the default header block.
    /// </summary>
    public static readonly HeaderBlock Default = new(systemIdentifier: null, generatingSoftware: null);

    /// <summary>
    /// The size of the version 1.0 header block.
    /// </summary>
    internal const ushort Size10 = (sizeof(sbyte) * 4) // File Signature (“LASF”)
        + sizeof(ushort) // File Source ID
        + sizeof(ushort) // Global Encoding
        + sizeof(uint) // Project ID - GUID data 1
        + sizeof(ushort) // Project ID - GUID data 2
        + sizeof(ushort) // Project ID - GUID data 3
        + (sizeof(byte) * 8) // Project ID - GUID data 4
        + sizeof(byte) // Version Major
        + sizeof(byte) // Version Minor
        + (sizeof(sbyte) * 32) // System Identifier
        + (sizeof(sbyte) * 32) // Generating Software
        + sizeof(ushort) // File Creation Day of Year
        + sizeof(ushort) // File Creation Year
        + sizeof(ushort) // Header Size
        + sizeof(uint) // Offset to point data
        + sizeof(uint) // Number of Variable Length Records
        + sizeof(byte) // Point Data Format ID (0-99 for spec)
        + sizeof(ushort) // Legacy point Data Record Length
        + sizeof(uint) // Legacy number of point records
        + (sizeof(uint) * 5) // Number of points by return
        + sizeof(double) // X scale factor
        + sizeof(double) // Y scale factor
        + sizeof(double) // Z scale factor
        + sizeof(double) // X offset
        + sizeof(double) // Y offset
        + sizeof(double) // Z offset
        + sizeof(double) // Max X
        + sizeof(double) // Min X
        + sizeof(double) // Max Y
        + sizeof(double) // Min Y
        + sizeof(double) // Max Z
        + sizeof(double); // Min Z

#if LAS1_3_OR_GREATER
    /// <summary>
    /// The size of the version 1.3 header block.
    /// </summary>
    internal const ushort Size13 = Size10
        + sizeof(ulong); // Start of wavelength data
#endif

#if LAS1_4_OR_GREATER
    /// <summary>
    /// The size of the version 1.4 header block.
    /// </summary>
    internal const ushort Size14 = Size13
        + sizeof(ulong) // Start of first extended variable length record
        + sizeof(uint) // Number of extended variable length records
        + sizeof(ulong) // Number of point records
        + (sizeof(ulong) * 15); // Number of points by return
#endif

#if LAS1_5_OR_GREATER
    /// <summary>
    /// The size of the version 1.5 header block.
    /// </summary>
    internal const ushort Size15 = Size14
        + sizeof(double) // Max GPS time
        + sizeof(double) // Min GPS time
        + sizeof(ushort); // Time offset.
#endif

    /// <summary>
    /// The maximum minor version.
    /// </summary>
    internal const int MaxMinorVersion
#if LAS1_5_OR_GREATER
        = 5;
#elif LAS1_4_OR_GREATER
        = 4;
#elif LAS1_3_OR_GREATER
        = 3;
#elif LAS1_2_OR_GREATER
        = 2;
#else
        = 1;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderBlock"/> struct.
    /// </summary>
    /// <param name="systemIdentifier">The system identifier.</param>
    /// <param name="generatingSoftware">The generating software.</param>
    public HeaderBlock(string? systemIdentifier, string? generatingSoftware)
    {
        this.FileSignature = "LASF";
        this.Version = DefaultVersion;
        this.SystemIdentifier = systemIdentifier;
        this.GeneratingSoftware = generatingSoftware;
#if LAS1_5_OR_GREATER
        this.PointDataFormat = ExtendedGpsPointDataRecord.Id;
#else
        this.PointDataFormat = PointDataRecord.Id;
#endif

#if LAS1_4_OR_GREATER
        this.LegacyNumberOfPointsByReturn = new uint[5];
#else
        this.NumberOfPointsByReturn = new uint[5];
#endif
        this.ScaleFactor = new Vector3D(0.01, 0.01, 0.01);
        this.Offset = Vector3D.Zero;
        this.Min = Vector3D.Zero;
        this.Max = Vector3D.Zero;
#if LAS1_4_OR_GREATER
        this.RawNumberOfPointsByReturn = new ulong[15];
#endif
    }

#pragma warning disable SA1642
#if LAS1_5_OR_GREATER
    /// <include file="Properties/v1.5/Documentation.xml" path="doc/members/member[@name='M:Altemiq.IO.Las.HeaderBlock.Constructor']/*" />
#elif LAS1_4_OR_GREATER
    /// <include file="Properties/v1.4/Documentation.xml" path="doc/members/member[@name='M:Altemiq.IO.Las.HeaderBlock.Constructor']/*" />
#elif LAS1_2_OR_GREATER
    /// <include file="Properties/v1.2/Documentation.xml" path="doc/members/member[@name='M:Altemiq.IO.Las.HeaderBlock.Constructor']/*" />
#else
    /// <include file="Properties/v1.1/Documentation.xml" path="doc/members/member[@name='M:Altemiq.IO.Las.HeaderBlock.Constructor']/*" />
#endif
#pragma warning restore SA1642
    internal HeaderBlock(
        ushort fileSourceId,
#if LAS1_2_OR_GREATER
        GlobalEncoding globalEncoding,
#endif
        Guid? projectId,
        Version version,
        string? systemIdentifier,
        string? generatingSoftware,
        DateTime? fileCreation,
        byte pointDataFormat,
#if LAS1_4_OR_GREATER
        uint legacyPointCount,
        uint[] legacyPointsByReturn,
#else
        uint pointCount,
        uint[] pointsByReturn,
#endif
        Vector3D scaleFactor,
        Vector3D offset,
#if LAS1_4_OR_GREATER
        ulong pointCount,
        ulong[] pointsByReturn,
#endif
#if LAS1_5_OR_GREATER
        double maxGpsTime,
        double minGpsTime,
        ushort timeOffset,
#endif
        Vector3D min,
        Vector3D max)
    {
        this.FileSignature = "LASF";
        this.FileSourceId = fileSourceId;
        this.ProjectId = projectId ?? Guid.Empty;
#if LAS1_2_OR_GREATER
        this.GlobalEncoding = globalEncoding;
#endif
        if (version is not { Major: 1, Minor: >= 0 and <= MaxMinorVersion })
        {
            // invalid version.
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        this.Version = version;
        this.SystemIdentifier = systemIdentifier;
        this.GeneratingSoftware = generatingSoftware;
        this.FileCreation = fileCreation;
        this.PointDataFormat = pointDataFormat;
#if LAS1_4_OR_GREATER
        this.LegacyNumberOfPointRecords = legacyPointCount;
        if (legacyPointsByReturn is not { Length: 5 })
        {
            // invalid legacy array.
            throw new ArgumentOutOfRangeException(nameof(legacyPointsByReturn));
        }

        this.LegacyNumberOfPointsByReturn = legacyPointsByReturn;
#else
        this.NumberOfPointRecords = pointCount;
        if (pointsByReturn is not { Length: 5 })
        {
            // invalid legacy array.
            throw new ArgumentOutOfRangeException(nameof(pointsByReturn));
        }

        this.NumberOfPointsByReturn = pointsByReturn;
#endif
        this.ScaleFactor = scaleFactor;
        this.Offset = offset;
        this.Min = min;
        this.Max = max;
#if LAS1_4_OR_GREATER
        this.RawNumberOfPointRecords = pointCount;
        if (pointsByReturn is not { Length: 15 })
        {
            // invalid array.
            throw new ArgumentOutOfRangeException(nameof(pointsByReturn));
        }

        this.RawNumberOfPointsByReturn = pointsByReturn;
#endif

#if LAS1_5_OR_GREATER
        this.MaxGpsTime = maxGpsTime;
        this.MinGpsTime = minGpsTime;
        this.TimeOffset = timeOffset;
#endif
    }

    /// <summary>
    /// Gets the file signature. The file signature must contain the four characters “LASF”, and it is required by the LAS specification. These four characters can be checked by user software as a quick look initial determination of file type.
    /// </summary>
    public string FileSignature { get; }

    /// <summary>
    /// Gets the file source id.
    /// </summary>
    /// <value>The file source id.</value>
    /// <remarks>
    /// <para>
    /// This field should be set to a value from 0 to 65,535.
    /// A value of zero is interpreted to mean that an ID has not been assigned, which is the norm for a LAS file resulting from an aggregation of multiple independent sources (e.g., a tile merged from multiple swaths).</para>
    /// <para>
    /// Note that this scheme allows a project to contain up to 65,535 unique sources.
    /// Example sources can include a data repository ID or an original collection of temporally consistent data such as a flight line or sortie number for airborne systems, a route number for mobile systems, or a setup identifier for static systems.
    /// </para>
    /// </remarks>
    public ushort FileSourceId { get; }

#if LAS1_5_OR_GREATER
    /// <include file="Properties/v1.5/Documentation.xml" path="doc/members/member[@name='P:Altemiq.IO.Las.HeaderBlock.GlobalEncoding']/*" />
    public GlobalEncoding GlobalEncoding { get; }
#elif LAS1_4_OR_GREATER
    /// <include file="Properties/v1.4/Documentation.xml" path="doc/members/member[@name='P:Altemiq.IO.Las.HeaderBlock.GlobalEncoding']/*" />
    public GlobalEncoding GlobalEncoding { get; }
#elif LAS1_3_OR_GREATER
    /// <include file="Properties/v1.3/Documentation.xml" path="doc/members/member[@name='P:Altemiq.IO.Las.HeaderBlock.GlobalEncoding']/*" />
    public GlobalEncoding GlobalEncoding { get; }
#elif LAS1_2_OR_GREATER
    /// <include file="Properties/v1.2/Documentation.xml" path="doc/members/member[@name='P:Altemiq.IO.Las.HeaderBlock.GlobalEncoding']/*" />
    public GlobalEncoding GlobalEncoding { get; }
#endif

    /// <summary>
    /// Gets the project id.
    /// </summary>
    public Guid ProjectId { get; }

    /// <summary>
    /// Gets the version that indicates the format number of the current specification itself.
    /// </summary>
    public Version Version { get; }

    /// <summary>
    /// Gets the system identifier.
    /// </summary>
    public string? SystemIdentifier { get; }

    /// <summary>
    /// Gets the generating software as ASCII data describing the generating software itself.
    /// </summary>
    public string? GeneratingSoftware { get; }

    /// <summary>
    /// Gets the file creation.
    /// </summary>
    public DateTime? FileCreation { get; }

    /// <summary>
    /// Gets the point data format id.
    /// </summary>
    /// <remarks>
    /// <para>The point data format ID corresponds to the point data record format type.</para>
    /// <list type="bullet">
    /// <item>LAS 1.0 defines types 0 and 1</item>
    /// <item>LAS 1.2 defines types 2 and 3</item>
    /// <item>LAS 1.3 defines types 4 and 5</item>
    /// <item>LAS 1.4 defines types 6, 7, 8, 9, and 10</item>
    /// </list>
    /// </remarks>
    public byte PointDataFormatId => (byte)(this.PointDataFormat & 0x3f);

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets the legacy total number of point records within the file.
    /// </summary>
    public uint LegacyNumberOfPointRecords { get; }

    /// <summary>
    /// Gets the legacy number of points by return.
    /// </summary>
    /// <remarks>The first unsigned long value will be the total number of records from the first return, and the second contains the total number for return two, and so forth up to five returns.</remarks>
    public IReadOnlyList<uint> LegacyNumberOfPointsByReturn { get; }
#else
    /// <summary>
    /// Gets the total number of point records within the file.
    /// </summary>
    public uint NumberOfPointRecords { get; }

    /// <summary>
    /// Gets the number of points by return.
    /// </summary>
    /// <remarks>The first unsigned long value will be the total number of records from the first return, and the second contains the total number for return two, and so forth up to five returns.</remarks>
    public IReadOnlyList<uint> NumberOfPointsByReturn { get; }
#endif

    /// <summary>
    /// Gets the scale factor.
    /// </summary>
    /// <remarks>The scale factor fields contain a double floating point value that is used to scale the corresponding X, Y, and Z long values within the point records. The corresponding X, Y, and Z scale factor must be multiplied by the X, Y, or Z point record value to get the actual X, Y, or Z coordinate. For example, if the X, Y, and Z coordinates are intended to have two decimal point values, then each scale factor will contain the number 0.01.</remarks>
    public Vector3D ScaleFactor { get; }

    /// <summary>
    /// Gets the offset.
    /// </summary>
    /// <remarks>The offset fields should be used to set the overall offset for the point records. In general these numbers will be zero, but for certain cases the resolution of the point data may not be large enough for a given projection system. However, it should always be assumed that these numbers are used. So to scale a given X from the point record, take the point record X multiplied by the X scale factor, and then add the X offset.</remarks>
    public Vector3D Offset { get; }

    /// <summary>
    /// Gets the min un-scaled extents of the LAS point file data, specified in the coordinate system of the LAS data.
    /// </summary>
    public Vector3D Min { get; }

    /// <summary>
    /// Gets the max un-scaled extents of the LAS point file data, specified in the coordinate system of the LAS data.
    /// </summary>
    public Vector3D Max { get; }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets the total number of point records within the file.
    /// </summary>
    public ulong NumberOfPointRecords => this.Version switch
    {
        { Major: 1, Minor: >= 4 } => this.RawNumberOfPointRecords,
        _ => this.LegacyNumberOfPointRecords,
    };

    /// <summary>
    /// Gets the number of points by return.
    /// </summary>
    /// <remarks>The first unsigned long value will be the total number of records from the first return, and the second contains the total number for return two, and so forth up to five returns.</remarks>
    public IReadOnlyList<ulong> NumberOfPointsByReturn => this.Version switch
    {
        { Major: 1, Minor: >= 4 } => this.RawNumberOfPointsByReturn,
        _ => new ReadOnlyLegacy(this.LegacyNumberOfPointsByReturn),
    };
#endif

#if LAS1_5_OR_GREATER
    /// <summary>
    /// Gets the maximum GPS time.
    /// </summary>
    public readonly double MaxGpsTime { get; }

    /// <summary>
    /// Gets the minimum GPS time.
    /// </summary>
    public readonly double MinGpsTime { get; }

    /// <summary>
    /// Gets the time offset.
    /// </summary>
    /// <remarks>The Time Offset field can be used to optimize GPS Time precision for a desired time period. Offset GPS Time for a point record is equal to standard GPS Time, minus 106 * Time Offset.</remarks>
    public readonly ushort TimeOffset { get; }
#endif

    /// <summary>
    /// Gets the point data format.
    /// </summary>
    internal byte PointDataFormat { get; }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets the raw total number of point records within the file.
    /// </summary>
    internal ulong RawNumberOfPointRecords { get; }

    /// <summary>
    /// Gets the raw number of points by return.
    /// </summary>
    internal IReadOnlyList<ulong> RawNumberOfPointsByReturn { get; }
#endif

    /// <summary>
    /// Gets the result of the equal operator.
    /// </summary>
    /// <param name="left">The left hand side.</param>
    /// <param name="right">The right hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(in HeaderBlock left, in HeaderBlock right) => left.Equals(in right);

    /// <summary>
    /// Gets the result of the not-equal operator.
    /// </summary>
    /// <param name="left">The left hand side.</param>
    /// <param name="right">The right hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(in HeaderBlock left, in HeaderBlock right) => !(left == right);

    /// <inheritdoc/>
    public bool Equals(HeaderBlock other) => this.Equals(in other);

    /// <inheritdoc cref="Equals(HeaderBlock)"/>
    public bool Equals(in HeaderBlock other) => StringComparer.Ordinal.Equals(this.FileSignature, other.FileSignature)
                                                && this.FileSourceId == other.FileSourceId
#if LAS1_2_OR_GREATER
                                                && this.GlobalEncoding == other.GlobalEncoding
#endif
                                                && this.ProjectId == other.ProjectId
                                                && this.Version == other.Version
                                                && StringComparer.Ordinal.Equals(this.SystemIdentifier, other.SystemIdentifier)
                                                && StringComparer.Ordinal.Equals(this.GeneratingSoftware, other.GeneratingSoftware)
                                                && this.FileCreation == other.FileCreation
                                                && this.PointDataFormat == other.PointDataFormat
                                                && this.ScaleFactor == other.ScaleFactor
                                                && this.Offset == other.Offset
                                                && this.Min == other.Min
                                                && this.Max == other.Max
                                                && this.NumberOfPointRecords == other.NumberOfPointRecords
#if LAS1_5_OR_GREATER
                                                && this.NumberOfPointsByReturn.SequenceEqual(other.NumberOfPointsByReturn)
                                                && this.MaxGpsTime.Equals(other.MaxGpsTime)
                                                && this.MinGpsTime.Equals(other.MinGpsTime)
                                                && this.TimeOffset.Equals(other.TimeOffset);
#else
                                                && this.NumberOfPointsByReturn.SequenceEqual(other.NumberOfPointsByReturn);
#endif

    /// <inheritdoc/>
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is HeaderBlock headerBlock ? this.Equals(in headerBlock) : base.Equals(obj);

    /// <inheritdoc/>
    public override int GetHashCode()
#if NETSTANDARD2_0_OR_GREATER || NET461_OR_GREATER || NET
    {
        var hashCode = default(HashCode);
        hashCode.Add(this.FileSignature, StringComparer.Ordinal);
        hashCode.Add(this.FileSourceId);
#if LAS1_2_OR_GREATER
        hashCode.Add(this.GlobalEncoding);
#endif
        hashCode.Add(this.ProjectId);
        hashCode.Add(this.Version);
        if (this.SystemIdentifier is { } systemIdentifier)
        {
            hashCode.Add(systemIdentifier, StringComparer.Ordinal);
        }

        if (this.GeneratingSoftware is { } generatingSoftware)
        {
            hashCode.Add(generatingSoftware, StringComparer.Ordinal);
        }

        hashCode.Add(this.FileCreation);
        hashCode.Add(this.PointDataFormat);
        hashCode.Add(this.ScaleFactor);
        hashCode.Add(this.Offset);
        hashCode.Add(this.Min);
        hashCode.Add(this.Max);
        hashCode.Add(this.NumberOfPointRecords);
        hashCode.Add(this.NumberOfPointsByReturn);
#if LAS1_5_OR_GREATER
        hashCode.Add(this.MaxGpsTime);
        hashCode.Add(this.MinGpsTime);
        hashCode.Add(this.TimeOffset);
#endif
        return hashCode.ToHashCode();
    }
#else
        => StringComparer.Ordinal.GetHashCode(this.FileSignature)
            ^ this.FileSourceId.GetHashCode()
#if LAS1_2_OR_GREATER
            ^ this.GlobalEncoding.GetHashCode()
#endif
            ^ this.ProjectId.GetHashCode()
            ^ this.Version.GetHashCode()
            ^ (this.SystemIdentifier is { } systemIdentifier ? StringComparer.Ordinal.GetHashCode(systemIdentifier) : 0)
            ^ (this.GeneratingSoftware is { } generatingSoftware ? StringComparer.Ordinal.GetHashCode(generatingSoftware) : 0)
            ^ this.FileCreation.GetHashCode()
            ^ this.PointDataFormat.GetHashCode()
            ^ this.ScaleFactor.GetHashCode()
            ^ this.Offset.GetHashCode()
            ^ this.Min.GetHashCode()
            ^ this.Max.GetHashCode()
            ^ this.NumberOfPointRecords.GetHashCode()
#if LAS1_5_OR_GREATER
            ^ this.NumberOfPointsByReturn.GetHashCode()
            ^ this.MaxGpsTime.GetHashCode()
            ^ this.MinGpsTime.GetHashCode()
            ^ this.TimeOffset.GetHashCode();
#else
            ^ this.NumberOfPointsByReturn.GetHashCode();
#endif
#endif

#if LAS1_5_OR_GREATER
    /// <summary>
    /// Converts the value of this instance to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the value of this instance.</returns>>
    public override string ToString() =>
        $$"""
        {
          {{nameof(this.FileSignature)}}: 'LASF',
          {{nameof(this.FileSourceId)}}: {{this.FileSourceId}},
          {{nameof(this.GlobalEncoding)}}: {{(int)this.GlobalEncoding}},
          {{nameof(this.ProjectId)}}: '{{this.ProjectId}}',
          {{nameof(this.Version)}}: {{this.Version}},
          {{nameof(this.SystemIdentifier)}}: '{{this.SystemIdentifier}}',
          {{nameof(this.FileSourceId)}}: {{this.FileSourceId}},
          {{nameof(this.FileCreation)}}: {{this.FileCreation}},
          {{nameof(this.PointDataFormat)}}: {{this.PointDataFormat}},
          {{nameof(this.NumberOfPointRecords)}}: {{nameof(this.NumberOfPointRecords)}}
          {{nameof(this.NumberOfPointsByReturn)}}: [ {{string.Join(", ", this.NumberOfPointsByReturn)}} ],
          {{nameof(this.ScaleFactor)}}, [ {{this.ScaleFactor.X}}, {{this.ScaleFactor.Y}}, {{this.ScaleFactor.Z}} ],
          {{nameof(this.Offset)}}, [ {{this.Offset.X}}, {{this.Offset.Y}}, {{this.Offset.Z}} ],
          {{nameof(this.Min)}}, [ {{this.Min.X}}, {{this.Min.Y}}, {{this.Min.Z}} ],
          {{nameof(this.Max)}}, [ {{this.Max.X}}, {{this.Max.Y}}, {{this.Max.Z}} ],
          {{nameof(this.MaxGpsTime)}}, {{this.MaxGpsTime}}
          {{nameof(this.MinGpsTime)}}, {{this.MinGpsTime}}
          {{nameof(this.TimeOffset)}}, {{this.TimeOffset}}
        }
        """;
#elif LAS1_4_OR_GREATER
    /// <summary>
    /// Converts the value of this instance to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the value of this instance.</returns>>
    public override string ToString() =>
        $$"""
        {
          {{nameof(this.FileSignature)}}: 'LASF',
          {{nameof(this.FileSourceId)}}: {{this.FileSourceId}},
          {{nameof(this.GlobalEncoding)}}: {{(int)this.GlobalEncoding}},
          {{nameof(this.ProjectId)}}: '{{this.ProjectId}}',
          {{nameof(this.Version)}}: {{this.Version}},
          {{nameof(this.SystemIdentifier)}}: '{{this.SystemIdentifier}}',
          {{nameof(this.FileSourceId)}}: {{this.FileSourceId}},
          {{nameof(this.FileCreation)}}: {{this.FileCreation}},
          {{nameof(this.PointDataFormat)}}: {{this.PointDataFormat}},
          {{nameof(this.NumberOfPointRecords)}}: {{nameof(this.NumberOfPointRecords)}}
          {{nameof(this.NumberOfPointsByReturn)}}: [ {{string.Join(", ", this.NumberOfPointsByReturn)}} ],
          {{nameof(this.ScaleFactor)}}, [ {{this.ScaleFactor.X}}, {{this.ScaleFactor.Y}}, {{this.ScaleFactor.Z}} ],
          {{nameof(this.Offset)}}, [ {{this.Offset.X}}, {{this.Offset.Y}}, {{this.Offset.Z}} ],
          {{nameof(this.Min)}}, [ {{this.Min.X}}, {{this.Min.Y}}, {{this.Min.Z}} ],
          {{nameof(this.Max)}}, [ {{this.Max.X}}, {{this.Max.Y}}, {{this.Max.Z}} ]
        }
        """;
#else
    /// <summary>
    /// Converts the value of this instance to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the value of this instance.</returns>>
    public override string ToString() =>
        $$"""
        {
          {{nameof(this.FileSignature)}}: 'LASF',
          {{nameof(this.FileSourceId)}}: {{this.FileSourceId}},
          {{nameof(this.ProjectId)}}: '{{this.ProjectId}}',
          {{nameof(this.Version)}}: {{this.Version}},
          {{nameof(this.SystemIdentifier)}}: '{{this.SystemIdentifier}}',
          {{nameof(this.FileSourceId)}}: {{this.FileSourceId}},
          {{nameof(this.FileCreation)}}: {{this.FileCreation}},
          {{nameof(this.PointDataFormat)}}: {{this.PointDataFormat}},
          {{nameof(this.NumberOfPointRecords)}}: {{nameof(this.NumberOfPointRecords)}}
          {{nameof(this.NumberOfPointsByReturn)}}: [ {{string.Join(", ", this.NumberOfPointsByReturn)}} ],
          {{nameof(this.ScaleFactor)}}, [ {{this.ScaleFactor.X}}, {{this.ScaleFactor.Y}}, {{this.ScaleFactor.Z}} ],
          {{nameof(this.Offset)}}, [ {{this.Offset.X}}, {{this.Offset.Y}}, {{this.Offset.Z}} ],
          {{nameof(this.Min)}}, [ {{this.Min.X}}, {{this.Min.Y}}, {{this.Min.Z}} ],
          {{nameof(this.Max)}}, [ {{this.Max.X}}, {{this.Max.Y}}, {{this.Max.Z}} ]
        }
        """;
#endif

    private readonly struct ReadOnlyLegacy(IReadOnlyList<uint> source) : IReadOnlyList<ulong>, IEquatable<IReadOnlyList<ulong>>, IEquatable<IReadOnlyList<uint>>
    {
        private readonly IReadOnlyList<uint> source = source;

        int IReadOnlyCollection<ulong>.Count => this.source.Count;

        ulong IReadOnlyList<ulong>.this[int index] => this.source[index];

        IEnumerator<ulong> IEnumerable<ulong>.GetEnumerator() => new Enumerator(this.source);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new Enumerator(this.source);

        bool IEquatable<IReadOnlyList<ulong>>.Equals(IReadOnlyList<ulong>? other) => other switch
        {
            ReadOnlyLegacy rol => this.source.Equals(rol.source),
            { } rol => this.source.Equals(rol),
            _ => false,
        };

        bool IEquatable<IReadOnlyList<uint>>.Equals(IReadOnlyList<uint>? other) => other switch
        {
            { } rol => this.source.Equals(rol),
            _ => false,
        };

        public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj switch
        {
            IReadOnlyList<uint> rol => this.Equals(rol),
            IReadOnlyList<ulong> rol => this.Equals(rol),
            _ => false,
        };

        public override int GetHashCode() => this.source.GetHashCode();

        public override string? ToString() => this.source.ToString();

        private readonly struct Enumerator(IEnumerable<uint> source) : IEnumerator<ulong>
        {
            private readonly IEnumerator<uint> source = source.GetEnumerator();

            ulong IEnumerator<ulong>.Current => this.source.Current;

            object System.Collections.IEnumerator.Current => this.source.Current;

            void IDisposable.Dispose() => this.source.Dispose();

            bool System.Collections.IEnumerator.MoveNext() => this.source.MoveNext();

            void System.Collections.IEnumerator.Reset() => this.source.Reset();

            public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => this.source.Equals(obj);

            public override int GetHashCode() => this.source.GetHashCode();

            public override string? ToString() => this.source.ToString();
        }
    }
}