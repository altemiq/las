// -----------------------------------------------------------------------
// <copyright file="OgcCoordinateSystemWkt.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// This record contains the textual data representing a Coordinate System WKT as defined in section 7 of the Coordinate Transformation Service Spec.
/// </summary>
public sealed record OgcCoordinateSystemWkt : VariableLengthRecord
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 2112;

    /// <summary>
    /// Initializes a new instance of the <see cref="OgcCoordinateSystemWkt"/> class.
    /// </summary>
    /// <param name="wkt">The well-known text.</param>
    public OgcCoordinateSystemWkt(string wkt)
        : this(WellKnownTextNode.Parse(wkt))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OgcCoordinateSystemWkt"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal OgcCoordinateSystemWkt(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : this(header, WellKnownTextNode.Parse(data))
    {
    }

    private OgcCoordinateSystemWkt(WellKnownTextNode wkt)
        : base(new VariableLengthRecordHeader
        {
            UserId = VariableLengthRecordHeader.ProjectionUserId,
            RecordId = TagRecordId,
            RecordLengthAfterHeader = (ushort)(wkt.GetByteCount() + 1),
            Description = "OGC COORDINATE SYSTEM WKT",
        }) => this.Wkt = wkt;

    private OgcCoordinateSystemWkt(VariableLengthRecordHeader header, WellKnownTextNode wkt)
        : base(header with { RecordLengthAfterHeader = (ushort)(wkt.GetByteCount() + 1) }) => this.Wkt = wkt;

    /// <summary>
    /// Gets the well-known text.
    /// </summary>
    public WellKnownTextNode Wkt { get; }

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        this.Header.CopyTo(destination);
        int bytesWritten = VariableLengthRecordHeader.Size;
        var d = destination[bytesWritten..];

        bytesWritten += this.Wkt.CopyTo(d);
        d[bytesWritten] = 0;

        return bytesWritten + 1;
    }
}