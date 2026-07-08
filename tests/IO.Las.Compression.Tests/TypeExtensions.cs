namespace Altemiq.IO.Las.Compression;

using TUnit.Assertions.Core;

public static class TypeExtensions
{
    public static TypeOfAssertion<TValue> IsTypeOf<TValue>(this TUnit.Assertions.Sources.ValueAssertion<TValue> assertion, Type expectedType)
    {
        _ =
#if NET6_0_OR_GREATER
            assertion.Context.ExpressionBuilder.Append(System.Globalization.CultureInfo.InvariantCulture, $".IsTypeOf<{expectedType.Name}>()");
#else
            assertion.Context.ExpressionBuilder.Append($".IsTypeOf<{expectedType.Name}>()");
#endif
        return new(assertion.Context, expectedType);
    }
}

public class TypeOfAssertion<TValue>(
    AssertionContext<TValue> context,
    Type expectedType) : Assertion<TValue>(context)
{
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

        return Task.FromResult(expectedType.IsAssignableFrom(actualType)
            ? AssertionResult.Passed
            : AssertionResult.Failed($"type {actualType.Name} is not assignable to {expectedType.Name}"));
    }

    protected override string GetExpectation()
    {
        return $"to be of type {expectedType.Name}";
    }
}