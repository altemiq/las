namespace Altemiq.IO.Las;

public class WellKnownTextValueTests
{
    private IEnumerable<object> Values =>
    [
        new WellKnownTextNode("ID", "1234"),
        1234.5678,
        "1234",
        new WellKnownTextLiteral("1234"),
    ];

    [Test]
    public async Task DefaultHasNoValue()
    {
        WellKnownTextValue value = default;
        await Assert.That(value)
            .Member(static v => v.HasValue, hasValue => hasValue.IsFalse()).And
            .Member(static v => v.Value, static value => value.IsNull());
    }

    [Test]
    [InstanceMethodDataSource(nameof(Values))]
    public async Task EqualsInput(object input)
    {
        await Assert.That(WellKnownTextValue.TryCreate(input, out WellKnownTextValue result)).IsTrue();
        await Assert.That(result).IsTypeOf<object>().And.IsEqualTo(result);
        await Assert.That(result).IsTypeOf<object>().And.IsEqualTo(input);
    }

    [Test]
    public async Task FromNode()
    {
        var node = new WellKnownTextNode("ID", "1234");
        WellKnownTextValue value = new(node);

        await Assert.That(value.Value).IsTypeOf<WellKnownTextNode>().And.IsEqualTo(node);
        await Assert.That(value).IsEqualTo(node);
        await Assert.That(value.TryGetValue(out WellKnownTextNode n)).IsTrue();
        await Assert.That(n).IsEqualTo(node);
    }

    [Test]
    public async Task FromDouble()
    {
        const double DoubleValue = 1234.5678;
        WellKnownTextValue value = new(DoubleValue);

        await Assert.That(value.Value).IsTypeOf<double>().And.IsEqualTo(DoubleValue);
        await Assert.That(value).IsEqualTo(DoubleValue);
        await Assert.That(value.TryGetValue(out double d)).IsTrue();
        await Assert.That(d).IsEqualTo(DoubleValue);
    }

    [Test]
    public async Task FromString()
    {
        const string StringValue = "1234";
        WellKnownTextValue value = new(StringValue);

        await Assert.That(value.Value).IsTypeOf<string>().And.IsEqualTo(StringValue);
        await Assert.That(value).IsEqualTo(StringValue);
        await Assert.That(value.TryGetValue(out string s)).IsTrue();
        await Assert.That(s).IsEqualTo(StringValue);
    }

    [Test]
    public async Task FromLiteral()
    {
        var literal = new WellKnownTextLiteral("1234");
        WellKnownTextValue value = new(literal);

        await Assert.That(value.Value).IsTypeOf<WellKnownTextLiteral>().And.IsEqualTo(literal);
        await Assert.That(value).IsEqualTo(literal);
        await Assert.That(value.TryGetValue(out WellKnownTextLiteral l)).IsTrue();
        await Assert.That(l).IsEqualTo(literal);
    }
}