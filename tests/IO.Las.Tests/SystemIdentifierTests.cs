// -----------------------------------------------------------------------
// <copyright file="SystemIdentifierTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

public class SystemIdentifierTests
{
    [Test]
    public async Task NullParse()
    {
        _ = await Assert.That(() => SystemIdentifier.Parse(null!)).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task TooShort()
    {
        _ = await Assert.That(() => SystemIdentifier.Parse("1")).ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task TooLong()
    {
        _ = await Assert.That(() => SystemIdentifier.Parse("123456")).ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IncorrectPlatform()
    {
        _ = await Assert.That(() => SystemIdentifier.Parse("12345")).ThrowsExactly<KeyNotFoundException>();
    }

    [Test]
    public async Task IncorrectModel()
    {
        _ = await Assert.That(() => SystemIdentifier.Parse("A2345")).ThrowsExactly<KeyNotFoundException>();
    }

    [Test]
    public async Task RieglTripod()
    {
        var si = SystemIdentifier.Parse("TRTA0");
        _ = await Assert.That(si.Platform).IsEqualTo(Platforms.StaticTripod);
        _ = await Assert.That(si.Model).IsEqualTo(Models.Riegl.VZ1000);
    }

    [Test]
    public async Task TeledyneOptechTruck()
    {
        var si = SystemIdentifier.Parse("MO000");
        _ = await Assert.That(si.Platform).IsEqualTo(Platforms.CrewedTruckVanVehicle);
        _ = await Assert.That(si.Model).IsEqualTo(Models.TeledyneOptech.GenericOptech);
    }

    [Test]
    public async Task LineOfSightTripod()
    {
        var si = SystemIdentifier.Parse("T000L");
        _ = await Assert.That(si.Platform).IsEqualTo(Platforms.StaticTripod);
        _ = await Assert.That(si.Model).IsEqualTo(Models.Generic.AnyTofLidar);
    }

    [Test]
    public async Task TwoSensors()
    {
        List<SystemIdentifier> identifiers = [.. SystemIdentifier.ParseMultiple("AR15J ALTM1")];
        _ = await Assert.That(identifiers).Count().IsEqualTo(2)
            .And.IsEquivalentTo([
                new(Platforms.CrewedFixedWing, Models.Riegl.VQ1560ii),
                new SystemIdentifier(Platforms.CrewedFixedWing, Models.Leica.TerrainMapper)
            ]);
    }

    [Test]
    public async Task TwoSensorsDifferentPlatforms()
    {
        List<SystemIdentifier> identifiers = [.. SystemIdentifier.ParseMultiple("RRX2H ARX2H")];
        _ = await Assert.That(identifiers).Count().IsEqualTo(2)
            .And.IsEquivalentTo([
                new(Platforms.CrewedHelicopterRotary, Models.Riegl.Vux240),
                new SystemIdentifier(Platforms.CrewedFixedWing, Models.Riegl.Vux240)
            ]);
    }

#if LAS1_2_OR_GREATER
    [Test]
    public async Task RieglTripodFromLas()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(SystemIdentifierTests).Assembly.GetManifestResourceStream(typeof(SystemIdentifierTests), "systemid_examples.1sensor.las")
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        HeaderBlockReader header = new(stream);

        var headerBlock = header.GetHeaderBlock();

        _ = await Assert.That(headerBlock.SystemIdentifier).IsNotNull();

        var identifier = SystemIdentifier.Parse(headerBlock.SystemIdentifier!);
        _ = await Assert.That(identifier).IsEqualTo(new SystemIdentifier(Platforms.StaticTripod, Models.Riegl.VZ1000));
    }

    [Test]
    public async Task LineOfSightTripodFromLas()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(SystemIdentifierTests).Assembly.GetManifestResourceStream(typeof(SystemIdentifierTests), "systemid_examples.1sensor_generic.las")
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        HeaderBlockReader header = new(stream);

        var headerBlock = header.GetHeaderBlock();

        _ = await Assert.That(headerBlock.SystemIdentifier).IsNotNull();

        var si = SystemIdentifier.Parse(headerBlock.SystemIdentifier!);
        _ = await Assert.That(si.Platform).IsEqualTo(Platforms.StaticTripod);
        _ = await Assert.That(si.Model).IsEqualTo(Models.Generic.AnyTofLidar);
    }

    [Test]
    public async Task FiveSensorsFromLas()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(SystemIdentifierTests).Assembly.GetManifestResourceStream(typeof(SystemIdentifierTests), "systemid_examples.5sensor.las")
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        HeaderBlockReader header = new(stream);

        var headerBlock = header.GetHeaderBlock();

        _ = await Assert.That(headerBlock.SystemIdentifier).IsNotNull();

        var identifiers = SystemIdentifier.ParseMultiple(headerBlock.SystemIdentifier!);
        _ = await Assert.That(identifiers).IsEquivalentTo(
            [
                new(Platforms.CrewedHelicopterRotary, Models.Riegl.Vux240),
                new(Platforms.CrewedFixedWing, Models.Leica.TerrainMapper),
                new(Platforms.CrewedFixedWing, Models.L3HarrisTechnologies.L3HarrisGeigerModeLiDAR),
                new(Platforms.UasUavDroneCopter, Models.Dji.ZenmuseX5),
                new SystemIdentifier(Platforms.CrewedWatercraft, Models.Norbit.IWBMSStxMultibeam),
            ]);
    }
#endif
}