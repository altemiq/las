// -----------------------------------------------------------------------
// <copyright file="Classification.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The ASPRS reserved classifications.
/// </summary>
public enum Classification
{
    /// <summary>
    /// Created, never classified.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Point is unclassified.
    /// </summary>
    Unclassified = 1,

    /// <summary>
    /// Ground classification.
    /// </summary>
    Ground = 2,

    /// <summary>
    /// Low Vegetation classification.
    /// </summary>
    LowVegetation = 3,

    /// <summary>
    /// Medium Vegetation classification.
    /// </summary>
    MediumVegetation = 4,

    /// <summary>
    /// High Vegetation classification.
    /// </summary>
    HighVegetation = 5,

    /// <summary>
    /// Building classification.
    /// </summary>
    Building = 6,

    /// <summary>
    /// Low Point (noise) classification.
    /// </summary>
    LowPoint = 7,

    /// <summary>
    /// Model Key-point (mass point) classification.
    /// </summary>
    ModelKeyPoint = 8,

    /// <summary>
    /// Water classification.
    /// </summary>
    Water = 9,

    /// <summary>
    /// Overlap Points classification.
    /// </summary>
    OverlapPoints = 12,
}