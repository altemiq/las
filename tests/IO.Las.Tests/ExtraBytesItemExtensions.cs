namespace Altemiq.IO.Las;

public static class ExtraBytesValueExtensions
{
    public static ExtraBytesValueTypeOfAssertion<TExpected> ValueIsTypeOf<TExpected>(this TUnit.Assertions.Core.IAssertionSource<ExtraBytesValue> source)
    {
        source.Context.ExpressionBuilder.Append($".Value.IsTypeOf<{typeof(TExpected).Name}>()");
        return new(source.Context);
    }
}

public class ExtraBytesValueTypeOfAssertion<TTo>(TUnit.Assertions.Core.AssertionContext<ExtraBytesValue> parentContext)
    : TUnit.Assertions.Core.Assertion<TTo>(parentContext.Map(value => value.Value is TTo casted ? casted : throw new InvalidCastException($"Value is of type {value.Value?.GetType().Name ?? "null"}, not {typeof(TTo).Name}")))
{
    private readonly Type expectedType = typeof(TTo);

    protected override Task<TUnit.Assertions.Core.AssertionResult> CheckAsync(TUnit.Assertions.Core.EvaluationMetadata<TTo> metadata) => Task.FromResult(
        metadata.Exception is { Message: var message }
            ? TUnit.Assertions.Core.AssertionResult.Failed(message)
            : TUnit.Assertions.Core.AssertionResult.Passed);

    protected override string GetExpectation() => $"to be of type {this.expectedType.Name}";
}