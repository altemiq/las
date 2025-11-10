namespace Altemiq.IO.Las.Compression;

using TUnit.Assertions.Core;

public static class TypeExtensions
{
    public static TypeOfAssertion<TValue> IsTypeOf<TValue>(this TUnit.Assertions.Sources.ValueAssertion<TValue> assertion, Type expectedType)
    {
        assertion.Context.ExpressionBuilder.Append($".IsTypeOf<{expectedType.Name}>()");
        return new(assertion.Context, expectedType);
    }
}

public class TypeOfAssertion<TValue>(
    AssertionContext<TValue> context,
    Type expectedType) : Assertion<TValue>(context)
{
    private readonly Type expectedType = expectedType;

    protected override Task<AssertionResult> CheckAsync(EvaluationMetadata<TValue> metadata)
    {
        var value = metadata.Value;
        var exception = metadata.Exception;

        object objectToCheck;

        // If we have an exception (from Throws/ThrowsExactly), check that
        if (exception is not null)
        {
            objectToCheck = exception;
        }

        // Otherwise check the value
        else if (value is not null)
        {
            objectToCheck = value;
        }
        else
        {
            return Task.FromResult(AssertionResult.Failed("value was null"));
        }

        var actualType = objectToCheck.GetType();

        return Task.FromResult(this.expectedType.IsAssignableFrom(actualType)
            ? AssertionResult.Passed
            : AssertionResult.Failed($"type {actualType.Name} is not assignable to {this.expectedType.Name}"));
    }

    protected override string GetExpectation() => $"to be of type {this.expectedType.Name}";
}