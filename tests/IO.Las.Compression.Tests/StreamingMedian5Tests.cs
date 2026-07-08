// -----------------------------------------------------------------------
// <copyright file="StreamingMedian5Tests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// Tests locking in the behavior of <see cref="StreamingMedian5"/> across the
/// class-to-struct refactor. Expected medians were captured from the baseline
/// (sealed class) implementation and must hold after the refactor.
/// </summary>
public class StreamingMedian5Tests
{
    [Test]
    public async Task NewInstanceReturnsZeroMedian()
    {
        _ = await Assert.That(default(StreamingMedian5).Get()).IsEqualTo(0);
    }

    [Test]
    [Arguments("ascending", new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 }, new[] { 0, 0, 10, 20, 30, 40, 50, 60, 70, 80 })]
    [Arguments("descending", new[] { -10, -20, -30, -40, -50, -60, -70, -80, -90, -100 }, new[] { 0, 0, -10, -20, -30, -40, -50, -60, -70, -80 })]
    [Arguments("alternating", new[] { 5, -5, 10, -10, 15, -15, 20, -20 }, new[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments("mixed", new[] { 100, 50, 200, 25, 300, 75, 150, 10, 500, 0 }, new[] { 0, 0, 50, 50, 50, 75, 100, 100, 100, 100 })]
    [Arguments("zeros", new[] { 0, 0, 0, 0, 0, 0, 0 }, new[] { 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments("constant", new[] { 42, 42, 42, 42, 42, 42, 42 }, new[] { 0, 0, 42, 42, 42, 42, 42 })]
    [Arguments("extremes", new[] { int.MaxValue, int.MinValue, 0, 1000, -1000, 500 }, new[] { 0, 0, 0, 0, 0, 0 })]
    public async Task SequencePreservesMedianPath(string name, int[] inputs, int[] expected)
    {
        _ = name;

        StreamingMedian5 median = default;
        for (var i = 0; i < inputs.Length; i++)
        {
            median.Add(inputs[i]);
            _ = await Assert.That(median.Get())
                .IsEqualTo(expected[i])
                .Because($"[{name}] median after inserting inputs[0..={i}] = [{string.Join(", ", inputs.Take(i + 1))}]");
        }
    }

    [Test]
    public async Task MedianArrayElementsAreIndependent()
    {
        // Critical for the struct refactor: each array element must be mutable in-place.
        // With struct semantics, array[i].Add(v) must mutate the element directly, not a copy.
        // C# arrays yield ref-modifiable locations for value types via indexers, so this is safe;
        // List<T>, Dictionary<K,V> indexers are NOT.
        var medians = new StreamingMedian5[3];

        medians[0].Add(100);
        medians[0].Add(200);
        medians[0].Add(300);
        medians[0].Add(400);

        medians[1].Add(-100);
        medians[1].Add(-200);
        medians[1].Add(-300);
        medians[1].Add(-400);

        // medians[2] untouched.

        // Expected values match the same baseline class behavior: four ascending adds
        // produce a median of 200; four descending produce -200.
        _ = await Assert.That(medians[0].Get()).IsEqualTo(200);
        _ = await Assert.That(medians[1].Get()).IsEqualTo(-200);
        _ = await Assert.That(medians[2].Get()).IsEqualTo(0);
    }
}