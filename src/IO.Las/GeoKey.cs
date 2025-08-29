// -----------------------------------------------------------------------
// <copyright file="GeoKey.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents the Geo key.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "This lines up with the specification. When it is removed from that, we will remove from here.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "InconsistentNaming", Justification = "This lines up with the specification.")]
public enum GeoKey
{
    /* 6.2.1 GeoTIFF Configuration Keys */

    /* Section 6.3.1.1 Codes */

    /// <summary>
    /// GT Model Type.
    /// </summary>
    GTModelTypeGeoKey = 1024,

    /* Section 6.3.1.2 Codes */

    /// <summary>
    /// GT Raster Type.
    /// </summary>
    GTRasterTypeGeoKey = 1025,

    /// <summary>
    /// Citation documentation.
    /// </summary>
    GTCitationGeoKey = 1026,

    /* 6.2.2 Geographic CS Parameter Keys */

    /* Section 6.3.2.1 Codes */

    /// <summary>
    /// The geodetic coordinate reference system.
    /// </summary>
    GeodeticCRSGeoKey = 2048,

    /// <inheritdoc cref="GeodeticCRSGeoKey" />
    [Obsolete($"Use {nameof(GeodeticCRSGeoKey)} instead")]
    GeographicTypeGeoKey = GeodeticCRSGeoKey,

    /// <summary>
    /// The geodetic citation.
    /// </summary>
    GeodeticCitationGeoKey = 2049,

    /// <inheritdoc cref="GeodeticCitationGeoKey"/>
    [Obsolete($"Use {nameof(GeodeticCitationGeoKey)} instead")]
    GeogCitationGeoKey = GeodeticCitationGeoKey,

    /* Section 6.3.2.2 Codes */

    /// <summary>
    /// The geodetic datum.
    /// </summary>
    GeodeticDatumGeoKey = 2050,

    /// <inheritdoc cref="GeodeticDatumGeoKey" />
    [Obsolete($"Use {nameof(GeodeticDatumGeoKey)} instead")]
    GeogGeodeticDatumGeoKey = GeodeticDatumGeoKey,

    /* Section 6.3.2.4 Codes */

    /// <summary>
    /// The prime meridian.
    /// </summary>
    PrimeMeridianGeoKey = 2051,

    /// <inheritdoc cref="PrimeMeridianGeoKey" />
    [Obsolete($"Use {nameof(PrimeMeridianGeoKey)} instead")]
    GeogPrimeMeridianGeoKey = PrimeMeridianGeoKey,

    /* Section 6.3.1.3 Codes */

    /// <summary>
    /// The linear units.
    /// </summary>
    GeogLinearUnitsGeoKey = 2052,

    /// <summary>
    /// Meters units.
    /// </summary>
    GeogLinearUnitSizeGeoKey = 2053,

    /* Section 6.3.1.4 Codes */

    /// <summary>
    /// The angular units.
    /// </summary>
    GeogAngularUnitsGeoKey = 2054,

    /// <summary>
    /// Radians units.
    /// </summary>
    GeogAngularUnitSizeGeoKey = 2055,

    /* Section 6.3.2.3 Codes */

    /// <summary>
    /// The ellipsoid.
    /// </summary>
    EllipsoidGeoKey = 2056,

    /// <inheritdoc cref="EllipsoidGeoKey" />
    [Obsolete($"Use {nameof(EllipsoidGeoKey)} instead")]
    GeogEllipsoidGeoKey = EllipsoidGeoKey,

    /// <summary>
    /// The ellipsoid semi-major axis.
    /// </summary>
    EllipsoidSemiMajorAxisGeoKey = 2057,

    /// <inheritdoc cref="EllipsoidSemiMajorAxisGeoKey" />
    [Obsolete($"Use {nameof(EllipsoidSemiMajorAxisGeoKey)} instead")]
    GeogSemiMajorAxisGeoKey = EllipsoidSemiMajorAxisGeoKey,

    /// <summary>
    /// The ellipsoid semi-minor axis.
    /// </summary>
    EllipsoidSemiMinorAxisGeoKey = 2058,

    /// <inheritdoc cref="EllipsoidSemiMinorAxisGeoKey" />
    [Obsolete($"Use {nameof(EllipsoidSemiMinorAxisGeoKey)} instead")]
    GeogSemiMinorAxisGeoKey = EllipsoidSemiMinorAxisGeoKey,

    /// <summary>
    /// The ellipsoid inverse flattening.
    /// </summary>
    EllipsoidInvFlatteningGeoKey = 2059,

    /// <inheritdoc cref="EllipsoidInvFlatteningGeoKey" />
    [Obsolete($"Use {nameof(EllipsoidInvFlatteningGeoKey)} instead")]
    GeogInvFlatteningGeoKey = EllipsoidInvFlatteningGeoKey,

    /* Section 6.3.1.4 Codes */

    /// <summary>
    /// The azimuth units.
    /// </summary>
    GeogAzimuthUnitsGeoKey = 2060,

    /// <summary>
    /// The prime meridian longitude.
    /// </summary>
    PrimeMeridianLongitudeGeoKey = 2061,

    /// <inheritdoc cref="PrimeMeridianLongitudeGeoKey"/>
    [Obsolete($"Use {nameof(PrimeMeridianLongitudeGeoKey)} instead")]
    GeogPrimeMeridianLongGeoKey = PrimeMeridianLongitudeGeoKey,

    /// <summary>
    /// 2011 - proposed addition.
    /// </summary>
    GeogToWgs84GeoKey = 2062,

    /* 6.2.3 Projected CS Parameter Keys */

    /* Section 6.3.3.1 Codes */

    /// <summary>
    /// Projected coordinate reference system.
    /// </summary>
    ProjectedCRSGeoKey = 3072,

    /// <inheritdoc cref="ProjectedCRSGeoKey" />
    [Obsolete($"Use {nameof(ProjectedCRSGeoKey)} instead")]
    ProjectedCSTypeGeoKey = ProjectedCRSGeoKey,

    /// <summary>
    /// Citation documentation.
    /// </summary>
    ProjectedCitationGeoKey = 3073,

    /// <inheritdoc cref="ProjectedCitationGeoKey" />
    [Obsolete($"Use {nameof(ProjectedCitationGeoKey)} instead")]
    PCSCitationGeoKey = ProjectedCitationGeoKey,

    /* Section 6.3.3.2 codes */

    /// <summary>
    /// The projection.
    /// </summary>
    ProjectionGeoKey = 3074,

    /* Section 6.3.3.3 codes */

    /// <summary>
    /// Projection method.
    /// </summary>
    ProjMethodGeoKey = 3075,

    /// <inheritdoc cref="ProjMethodGeoKey" />
    [Obsolete($"Use {nameof(ProjMethodGeoKey)} instead")]
    ProjCoordTransGeoKey = ProjMethodGeoKey,

    /* Section 6.3.1.3 codes */

    /// <summary>
    /// The projection linear units.
    /// </summary>
    ProjLinearUnitsGeoKey = 3076,

    /// <summary>
    /// Meters units.
    /// </summary>
    ProjLinearUnitSizeGeoKey = 3077,

    /// <summary>
    /// First Standard Parallel.
    /// </summary>
    ProjStdParallel1GeoKey = 3078,

    /// <inheritdoc cref="ProjStdParallel1GeoKey"/>
    ProjStdParallelGeoKey = ProjStdParallel1GeoKey,

    /// <summary>
    /// Second Standard Parallel.
    /// </summary>
    ProjStdParallel2GeoKey = 3079,

    /// <summary>
    /// Natural Origin Longitude.
    /// </summary>
    ProjNatOriginLongGeoKey = 3080,

    /// <inheritdoc cref="ProjNatOriginLongGeoKey"/>
    ProjOriginLongGeoKey = ProjNatOriginLongGeoKey,

    /// <summary>
    /// Natural Original Latitude.
    /// </summary>
    ProjNatOriginLatGeoKey = 3081,

    /// <inheritdoc cref="ProjNatOriginLatGeoKey"/>
    ProjOriginLatGeoKey = ProjNatOriginLatGeoKey,

    /// <summary>
    /// False Easting.
    /// </summary>
    ProjFalseEastingGeoKey = 3082,

    /// <summary>
    /// False Northing.
    /// </summary>
    ProjFalseNorthingGeoKey = 3083,

    /// <summary>
    /// False Origin in Longitude.
    /// </summary>
    ProjFalseOriginLongGeoKey = 3084,

    /// <summary>
    /// False Origin in Latitude.
    /// </summary>
    ProjFalseOriginLatGeoKey = 3085,

    /// <summary>
    /// False Origin in Easting.
    /// </summary>
    ProjFalseOriginEastingGeoKey = 3086,

    /// <summary>
    /// False Origin in Northing.
    /// </summary>
    ProjFalseOriginNorthingGeoKey = 3087,

    /// <summary>
    /// Centre Longitude.
    /// </summary>
    ProjCenterLongGeoKey = 3088,

    /// <summary>
    /// Centre Latitude.
    /// </summary>
    ProjCenterLatGeoKey = 3089,

    /// <summary>
    /// Centre Easting.
    /// </summary>
    ProjCenterEastingGeoKey = 3090,

    /// <summary>
    /// Centre Northing.
    /// </summary>
    ProjCenterNorthingGeoKey = 3091,

    /// <summary>
    /// Scale at Natural Origin.
    /// </summary>
    ProjScaleAtNatOriginGeoKey = 3092,

    /// <inheritdoc cref="ProjScaleAtNatOriginGeoKey"/>
    ProjScaleAtOriginGeoKey = ProjScaleAtNatOriginGeoKey,

    /// <summary>
    /// Scale at centre.
    /// </summary>
    ProjScaleAtCenterGeoKey = 3093,

    /// <summary>
    /// Azimuth angle.
    /// </summary>
    ProjAzimuthAngleGeoKey = 3094,

    /// <summary>
    /// Straight Vertical Pole Longitude.
    /// </summary>
    ProjStraightVertPoleLongGeoKey = 3095,

    /// <summary>
    /// Rectified Grid Angle.
    /// </summary>
    ProjRectifiedGridAngleGeoKey = 3096,

    /* 6.2.4 Vertical CS Keys */

    /// <summary>
    /// Vertical coordinate reference system.
    /// </summary>
    VerticalGeoKey = 4096,

    /// <inheritdoc cref="VerticalGeoKey"/>
    [Obsolete($"Use {nameof(VerticalGeoKey)} instead")]
    VerticalCSTypeGeoKey = VerticalGeoKey,

    /// <summary>
    /// Vertical citation.
    /// </summary>
    VerticalCitationGeoKey = 4097,

    /* Section 6.3.4.2 codes */

    /// <summary>
    /// Vertical datum.
    /// </summary>
    VerticalDatumGeoKey = 4098,

    /* Section 6.3.1 (.x, codes */

    /// <summary>
    /// Vertical units.
    /// </summary>
    VerticalUnitsGeoKey = 4099,

    /// <summary>
    /// End of the reserved <see cref="GeoKey"/> entries.
    /// </summary>
    ReservedEndGeoKey = 32767,

    /// <summary>
    /// Base of private or internal <see cref="GeoKey"/> entries.
    /// </summary>
    PrivateBaseGeoKey = 32768,

    /// <summary>
    /// End of private or internal <see cref="GeoKey"/> entries.
    /// </summary>
    PrivateEndGeoKey = ushort.MaxValue,

    /// <summary>
    /// Largest possible <see cref="GeoKey"/> ID.
    /// </summary>
    EndGeoKey = PrivateEndGeoKey,
}