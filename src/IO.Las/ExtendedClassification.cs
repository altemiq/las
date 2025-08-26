// -----------------------------------------------------------------------
// <copyright file="ExtendedClassification.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents the LAS point classification.
/// </summary>
public enum ExtendedClassification : byte
{
    /// <summary>
    /// Created, Never Classified.
    /// </summary>
    Created = Classification.Created,

    /// <summary>
    /// Point is Unclassified.
    /// </summary>
    Unclassified = Classification.Unclassified,

    /// <summary>
    /// Ground classification.
    /// </summary>
    Ground = Classification.Ground,

    /// <summary>
    /// Low Vegetation classification.
    /// </summary>
    LowVegetation = Classification.LowVegetation,

    /// <summary>
    /// Medium Vegetation classification.
    /// </summary>
    MediumVegetation = Classification.MediumVegetation,

    /// <summary>
    /// High Vegetation classification.
    /// </summary>
    HighVegetation = Classification.HighVegetation,

    /// <summary>
    /// Building classification.
    /// </summary>
    Building = Classification.Building,

    /// <summary>
    /// Low Point (Noise) classification.
    /// </summary>
    LowPoint = Classification.LowPoint,

    /// <summary>
    /// Water classification.
    /// </summary>
    Water = Classification.Water,

    /// <summary>
    /// Rail classification.
    /// </summary>
    Rail = 10,

    /// <summary>
    /// Road Surface classification.
    /// </summary>
    RoadSurface = 11,

    /// <summary>
    /// Wire – Guard (Shield) classification.
    /// </summary>
    WireGuard = 13,

    /// <summary>
    /// Wire – Conductor (Phase) classification.
    /// </summary>
    WireConductor = 14,

    /// <summary>
    /// Transmission Tower classification.
    /// </summary>
    TransmissionTower = 15,

    /// <summary>
    /// Wire-structure Connector classification.
    /// </summary>
    /// <remarks>e.g., insulators.</remarks>
    WireStructureConnector = 16,

    /// <summary>
    /// Bridge Deck classification.
    /// </summary>
    BridgeDeck = 17,

    /// <summary>
    /// High Noise classification.
    /// </summary>
    HighNoise = 18,

    /// <summary>
    /// Overhead Structure classification.
    /// </summary>
    /// <remarks>e.g.,conveyors,mining equipment,trafﬁc lights.</remarks>
    OverheadStructure = 19,

    /// <summary>
    /// Ignored Ground classification.
    /// </summary>
    /// <remarks>e.g., break line proximity.</remarks>
    IgnoredGround = 20,

    /// <summary>
    /// Snow classification.
    /// </summary>
    Snow = 21,

    /// <summary>
    /// Temporal Exclusion classification.
    /// </summary>
    /// <remarks>Features excluded due to changes over time between data sources – e.g., water levels, landslides, permafrost.</remarks>
    TemporalExclusion = 22,
}