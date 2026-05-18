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
        await Assert.That(() => SystemIdentifier.Parse(null!)).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task TooShort()
    {
        await Assert.That(() => SystemIdentifier.Parse("1")).ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task TooLong()
    {
        await Assert.That(() => SystemIdentifier.Parse("123456")).ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task IncorrectPlatform()
    {
        await Assert.That(() => SystemIdentifier.Parse("12345")).ThrowsExactly<KeyNotFoundException>();
    }

    [Test]
    public async Task IncorrectModel()
    {
        await Assert.That(() => SystemIdentifier.Parse("A2345")).ThrowsExactly<KeyNotFoundException>();
    }

    [Test]
    public async Task RieglTripod()
    {
        var si = SystemIdentifier.Parse("TRTA0");
        await Assert.That(si.Platform).IsEqualTo(Platforms.StaticTripod);
        await Assert.That(si.Model).IsEqualTo(Models.Riegl.VZ1000);
    }

    [Test]
    public async Task TeledyneOptechTruck()
    {
        var si = SystemIdentifier.Parse("MO000");
        await Assert.That(si.Platform).IsEqualTo(Platforms.CrewedTruckVanVehicle);
        await Assert.That(si.Model).IsEqualTo(Models.TeledyneOptech.GenericOptech);
    }

    [Test]
    public async Task LineOfSightTripod()
    {
        var si = SystemIdentifier.Parse("T000L");
        await Assert.That(si.Platform).IsEqualTo(Platforms.StaticTripod);
        await Assert.That(si.Model).IsEqualTo(Models.Generic.AnyTofLidar);
    }

    [Test]
    public async Task TwoSensors()
    {
        List<SystemIdentifier> identifiers = [.. SystemIdentifier.ParseMultiple("AR15J ALTM1")];
        await Assert.That(identifiers).Count().IsEqualTo(2)
            .And.IsEquivalentTo([
                new(Platforms.CrewedFixedWing, Models.Riegl.VQ1560ii),
                new SystemIdentifier(Platforms.CrewedFixedWing, Models.Leica.TerrainMapper)
            ]);
    }

    [Test]
    public async Task TwoSensorsDifferentPlatforms()
    {
        List<SystemIdentifier> identifiers = [.. SystemIdentifier.ParseMultiple("RRX2H ARX2H")];
        await Assert.That(identifiers).Count().IsEqualTo(2)
            .And.IsEquivalentTo([
                new(Platforms.CrewedHelicopterRotary, Models.Riegl.Vux240),
                new SystemIdentifier(Platforms.CrewedFixedWing, Models.Riegl.Vux240)
            ]);
    }

#if LAS1_2_OR_GREATER
    [Test]
    public async Task RieglTripodFromLas()
    {
        var stream = typeof(SystemIdentifierTests).Assembly.GetManifestResourceStream(typeof(SystemIdentifierTests), "systemid_examples.1sensor.las")
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        HeaderBlockReader header = new(stream);

        var headerBlock = await header.GetHeaderBlockAsync();

        await Assert.That(headerBlock.SystemIdentifier).IsNotNull();

        var identifier = SystemIdentifier.Parse(headerBlock.SystemIdentifier!);
        await Assert.That(identifier).IsEqualTo(new(Platforms.StaticTripod, Models.Riegl.VZ1000));

        await stream.DisposeAsync();
    }

    [Test]
    public async Task LineOfSightTripodFromLas()
    {
        var stream = typeof(SystemIdentifierTests).Assembly.GetManifestResourceStream(typeof(SystemIdentifierTests), "systemid_examples.1sensor_generic.las")
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        HeaderBlockReader header = new(stream);

        var headerBlock = await header.GetHeaderBlockAsync();

        await Assert.That(headerBlock.SystemIdentifier).IsNotNull();

        var si = SystemIdentifier.Parse(headerBlock.SystemIdentifier!);
        await Assert.That(si.Platform).IsEqualTo(Platforms.StaticTripod);
        await Assert.That(si.Model).IsEqualTo(Models.Generic.AnyTofLidar);

        await stream.DisposeAsync();
    }

    [Test]
    public async Task FiveSensorsFromLas()
    {
        var stream = typeof(SystemIdentifierTests).Assembly.GetManifestResourceStream(typeof(SystemIdentifierTests), "systemid_examples.5sensor.las")
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        HeaderBlockReader header = new(stream);

        var headerBlock = await header.GetHeaderBlockAsync();

        await Assert.That(headerBlock.SystemIdentifier).IsNotNull();

        var identifiers = SystemIdentifier.ParseMultiple(headerBlock.SystemIdentifier!);
        await Assert.That(identifiers).IsEquivalentTo(
            [
                new(Platforms.CrewedHelicopterRotary, Models.Riegl.Vux240),
                new(Platforms.CrewedFixedWing, Models.Leica.TerrainMapper),
                new(Platforms.CrewedFixedWing, Models.L3HarrisTechnologies.L3HarrisGeigerModeLiDAR),
                new(Platforms.UasUavDroneCopter, Models.Dji.ZenmuseX5),
                new SystemIdentifier(Platforms.CrewedWatercraft, Models.Norbit.IWBMSStxMultibeam),
            ]);

        await stream.DisposeAsync();
    }
#endif
}