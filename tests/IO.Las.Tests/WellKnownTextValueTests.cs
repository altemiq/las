namespace Altemiq.IO.Las;

public class WellKnownTextValueTests
{
    [Test]
    public async Task DefaultHasNoValue()
    {
        WellKnownTextValue value = default;
        _ = await Assert.That(value)
            .Member(static v => v.HasValue, static hasValue => hasValue.IsFalse()).And
            .Member(static v => v.Value, static value => value.IsNull());
    }

#if NET6_0_OR_GREATER
    [Test]
    public async Task SwitchOnNoValue()
    {
        _ = await Assert.That(default(WellKnownTextValue) switch
        {
            WellKnownTextNode or double or string or WellKnownTextLiteral => true,
            _ => false,
        }).IsFalse();

    }

    [Test]
    [InstanceMethodDataSource(nameof(Values))]
    public async Task EqualsInput(object input)
    {
        _ = await Assert.That(WellKnownTextValue.TryCreate(input, out var result)).IsTrue();
        _ = await Assert.That(result).IsTypeOf<object>().And.IsEqualTo(result);
        _ = await Assert.That(result).IsTypeOf<object>().And.IsEqualTo(input);
    }
#endif

    [Test]
    public async Task FromNode()
    {
        var node = new WellKnownTextNode("ID", "1234");
        WellKnownTextValue value = new(node);

        _ = await Assert.That(value.Value).IsTypeOf<WellKnownTextNode>().And.IsEqualTo(node);
        _ = await Assert.That(value).IsEqualTo(node);
        _ = await Assert.That(value.TryGetValue(out WellKnownTextNode n)).IsTrue();
        _ = await Assert.That(n).IsEqualTo(node);
    }

    [Test]
    public async Task FromDouble()
    {
        const double DoubleValue = 1234.5678;
        WellKnownTextValue value = new(DoubleValue);

        _ = await Assert.That(value.Value).IsTypeOf<double>().And.IsEqualTo(DoubleValue);
        _ = await Assert.That(value).IsEqualTo(DoubleValue);
        _ = await Assert.That(value.TryGetValue(out double d)).IsTrue();
        _ = await Assert.That(d).IsEqualTo(DoubleValue);
    }

    [Test]
    public async Task FromString()
    {
        const string StringValue = "1234";
        WellKnownTextValue value = new(StringValue);

        _ = await Assert.That(value.Value).IsTypeOf<string>().And.IsEqualTo(StringValue);
        _ = await Assert.That(value).IsEqualTo(StringValue);
        _ = await Assert.That(value.TryGetValue(out string s)).IsTrue();
        _ = await Assert.That(s).IsEqualTo(StringValue);
    }

    [Test]
    public async Task FromLiteral()
    {
        var literal = new WellKnownTextLiteral("1234");
        WellKnownTextValue value = new(literal);

        _ = await Assert.That(value.Value).IsTypeOf<WellKnownTextLiteral>().And.IsEqualTo(literal);
        _ = await Assert.That(value).IsEqualTo(literal);
        _ = await Assert.That(value.TryGetValue(out WellKnownTextLiteral l)).IsTrue();
        _ = await Assert.That(l).IsEqualTo(literal);
    }

#if NET6_0_OR_GREATER
    private IEnumerable<Func<object>> Values()
    {
        yield return static () => new WellKnownTextNode("ID", "1234");
        yield return static () => 1234.5678;
        yield return static () => "1234";
        yield return static () => new WellKnownTextLiteral("1234");
    }
#endif
}