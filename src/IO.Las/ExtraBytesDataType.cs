// -----------------------------------------------------------------------
// <copyright file="ExtraBytesDataType.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The values for the <see cref="ExtraBytesItem.DataType"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "These represent those types.")]
public enum ExtraBytesDataType : byte
{
    /// <summary>
    /// Value specified in the <see cref="ExtraBytesItem.Options"/> field.
    /// </summary>
    Undocumented = default,

    /// <summary>
    /// 1 byte, unsigned char.
    /// </summary>
    UnsignedChar = 1,

    /// <summary>
    /// 1 byte, char.
    /// </summary>
    Char = 2,

    /// <summary>
    /// 2 bytes, unsigned short.
    /// </summary>
    UnsignedShort = 3,

    /// <summary>
    /// 2 bytes, short.
    /// </summary>
    Short = 4,

    /// <summary>
    /// 4 bytes, unsigned long.
    /// </summary>
    UnsignedLong = 5,

    /// <summary>
    /// 4 bytes, long.
    /// </summary>
    Long = 6,

    /// <summary>
    /// 8 bytes, unsigned long, long.
    /// </summary>
    UnsignedLongLong = 7,

    /// <summary>
    /// 8 bytes, long, long.
    /// </summary>
    LongLong = 8,

    /// <summary>
    /// 4 bytes, float.
    /// </summary>
    Float = 9,

    /// <summary>
    /// 8 bytes, float.
    /// </summary>
    Double = 10,
}