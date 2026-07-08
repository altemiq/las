// -----------------------------------------------------------------------
// <copyright file="ExtensionTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

using Writers.Compressed;

public class ExtensionTests
{
    [Test]
    public async Task PositiveLongIsInInt32Range()
    {
        _ = await Assert.That(12345L.IsInt32()).IsTrue();
    }

    [Test]
    public async Task NegativeLongIsInInt32Range()
    {
        _ = await Assert.That((-12345L).IsInt32()).IsTrue();
    }

    [Test]
    public async Task PositiveLongIsNotInInt32Range()
    {
        _ = await Assert.That(2150000000L.IsInt32()).IsFalse();
    }

    [Test]
    public async Task NegativeLongIsNotInInt32Range()
    {
        _ = await Assert.That((-2150000000L).IsInt32()).IsFalse();
    }
}