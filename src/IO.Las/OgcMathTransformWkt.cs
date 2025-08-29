// -----------------------------------------------------------------------
// <copyright file="OgcMathTransformWkt.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// This record contains the textual data representing a Math Transform WKT as defined in section 7 of the Coordinate Transformation Service Spec.
/// </summary>
public sealed record OgcMathTransformWkt : VariableLengthRecord
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 2111;

    /// <summary>
    /// Initializes a new instance of the <see cref="OgcMathTransformWkt"/> class.
    /// </summary>
    /// <param name="wkt">The well-known text.</param>
    public OgcMathTransformWkt(string wkt)
        : base(
            new VariableLengthRecordHeader
            {
                UserId = VariableLengthRecordHeader.ProjectionUserId,
                RecordId = 2112,
                RecordLengthAfterHeader = (ushort)(System.Text.Encoding.UTF8.GetByteCount(wkt) + 1),
                Description = "OGC MATH TRANSFORMATION WKT",
            }) => this.Wkt = wkt;

    /// <summary>
    /// Initializes a new instance of the <see cref="OgcMathTransformWkt"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal OgcMathTransformWkt(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : base(header) => this.Wkt =
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            System.Text.Encoding.UTF8.GetString(data[..^1]);
#else
            System.Text.Encoding.UTF8.GetString(data[..^1].ToArray());
#endif

    /// <summary>
    /// Gets the well-known text.
    /// </summary>
    public string Wkt { get; }

    /// <inheritdoc />
    public override int Write(Span<byte> destination)
    {
        this.Header.Write(destination);
        int bytesWritten = VariableLengthRecordHeader.Size;
        var d = destination[bytesWritten..];

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        bytesWritten += System.Text.Encoding.UTF8.GetBytes(this.Wkt, d);
#else
        var bytes = System.Text.Encoding.UTF8.GetBytes(this.Wkt);
        bytes.AsSpan().CopyTo(d);
        bytesWritten += bytes.Length;
#endif
        d[bytesWritten] = 0;

        return bytesWritten + 1;
    }
}