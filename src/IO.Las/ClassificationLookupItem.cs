// -----------------------------------------------------------------------
// <copyright file="ClassificationLookupItem.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The classification lookup item.
/// </summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1, Size = 16)]
public readonly record struct ClassificationLookupItem
{
    /// <summary>
    /// Gets the class number.
    /// </summary>
    public byte ClassNumber { get; init; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    [field: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 15)]
    public string Description { get; init; }
}