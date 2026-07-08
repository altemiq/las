namespace Altemiq.IO.Las;

public static class ExtraBytesValueExtensions
{
    public static ExtraBytesValueTypeOfAssertion<TExpected> ValueIsTypeOf<TExpected>(this TUnit.Assertions.Core.IAssertionSource<ExtraBytesValue> source)
    {
        _ =
#if NET6_0_OR_GREATER
            source.Context.ExpressionBuilder.Append(System.Globalization.CultureInfo.InvariantCulture, $".Value.IsTypeOf<{typeof(TExpected).Name}>()");
#else
            source.Context.ExpressionBuilder.Append($".Value.IsTypeOf<{typeof(TExpected).Name}>()");
#endif
        return new(source.Context);
    }
}

public class ExtraBytesValueTypeOfAssertion<TTo>(TUnit.Assertions.Core.AssertionContext<ExtraBytesValue> parentContext)
    : TUnit.Assertions.Core.Assertion<TTo>(parentContext.Map(static value => value.Value is TTo casted ? casted : throw new InvalidCastException($"Value is of type {value.Value?.GetType().Name ?? "null"}, not {typeof(TTo).Name}")))
{
    private readonly Type expectedType = typeof(TTo);

    protected override Task<TUnit.Assertions.Core.AssertionResult> CheckAsync(TUnit.Assertions.Core.EvaluationMetadata<TTo> metadata)
    {
        return Task.FromResult(
            metadata.Exception is { Message: var message }
                ? TUnit.Assertions.Core.AssertionResult.Failed(message)
                : TUnit.Assertions.Core.AssertionResult.Passed);
    }

    protected override string GetExpectation()
    {
        return $"to be of type {expectedType.Name}";
    }
}