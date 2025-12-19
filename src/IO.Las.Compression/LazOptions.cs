// -----------------------------------------------------------------------
// <copyright file="LazOptions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="LasZip"/> options.
/// </summary>
[Flags]
public enum LazOptions : uint
{
    /// <summary>
    /// Default value.
    /// </summary>
    None = 0,

    /// <summary>
    /// Uses LAS1.4 compatibility mode.
    /// </summary>
    Las14Compatibility = 1 << 0,
}