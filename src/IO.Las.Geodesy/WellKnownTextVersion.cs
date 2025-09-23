// -----------------------------------------------------------------------
// <copyright file="WellKnownTextVersion.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Geodesy;

/// <summary>
/// The well-known text version.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "InconsistentNaming", Justification = "This is required.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "This is required.")]
public enum WellKnownTextVersion
{
    /// <summary>
    /// Well-known Text version 1.
    /// </summary>
    Wkt1 = 1,

    /// <summary>
    /// Well-known Text version 2:2015.
    /// </summary>
    Wkt2_2015 = 2,

    /// <summary>
    /// Well-known Text version 2:2019.
    /// </summary>
    Wkt2_2019 = 3,
}