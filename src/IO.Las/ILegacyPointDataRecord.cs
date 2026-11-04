// -----------------------------------------------------------------------
// <copyright file="ILegacyPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable S1133

/// <summary>
/// The legacy point data record.
/// </summary>
[Obsolete("Use properties from LAS 1.1+ instead.")]
[System.ComponentModel.Browsable(false)]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
internal interface ILegacyPointDataRecord
{
    /// <summary>
    /// Gets the file marker.
    /// </summary>
    /// <remarks>The file marker is an optional field that should be used in conjunction with the
    /// variable length records.The file marker allows for the LAS flight - line based files to be combined
    /// into single files with more than one flight - line.The actual original flight - line should be tracked in
    /// the variable length records, so that individual flight lines can be separated from a given combined
    /// file.</remarks>
    [Obsolete($"Use {nameof(IBasePointDataRecord.UserData)} instead")]
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    byte FileMarker { get; init; }

    /// <summary>
    /// Gets the bit field that is to be used at the users discretion.
    /// </summary>
    [Obsolete($"Use {nameof(IBasePointDataRecord.PointSourceId)} instead")]
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    ushort UserBitField { get; init; }
}