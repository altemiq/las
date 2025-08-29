// -----------------------------------------------------------------------
// <copyright file="GeoKeyEntry.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents the <see cref="GeoKey"/> entry.
/// </summary>
[Serializable]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 8, Pack = 2)]
public readonly record struct GeoKeyEntry
{
    /// <summary>
    /// Gets the Key ID.
    /// </summary>
    /// <remarks>Defined key ID for each piece of GeoTIFF data. IDs contained in the GeoTIFF specification.</remarks>
    [field: System.Runtime.InteropServices.FieldOffset(0)]
    public GeoKey KeyId { get; init; }

    /// <summary>
    /// Gets the TIFF tag location.
    /// </summary>
    /// <remarks>
    /// <para>Indicates where the data for this key is located:</para>
    /// <list type="bullet">
    ///   <item>
    ///     <term>0</term>
    ///     <description>means data is in the <see cref="ValueOffset"/> field as an unsigned short.</description>
    ///   </item>
    ///   <item>
    ///     <term>34736</term>
    ///     <description>means the data is located at index <see cref="ValueOffset"/> of the <see cref="GeoDoubleParamsTag"/> record.</description>
    ///   </item>
    ///   <item>
    ///     <term>34737</term>
    ///     <description>means the data is located at index <see cref="ValueOffset"/> of the <see cref="GeoAsciiParamsTag"/> record.</description>
    ///   </item>
    /// </list>
    /// </remarks>
    [field: System.Runtime.InteropServices.FieldOffset(2)]
    public ushort TiffTagLocation { get; init; }

    /// <summary>
    /// Gets the number of characters in string for values of <see cref="GeoAsciiParamsTag"/>, otherwise is 1.
    /// </summary>
    [field: System.Runtime.InteropServices.FieldOffset(4)]
    public ushort Count { get; init; }

    /// <summary>
    /// Gets the value offset.
    /// </summary>
    /// <remarks>Contents vary depending on value for <see cref="TiffTagLocation"/>.</remarks>
    [field: System.Runtime.InteropServices.FieldOffset(6)]
    public ushort ValueOffset { get; init; }
}