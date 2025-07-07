using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

public sealed class TypeHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        // Validate that the keyword value is a string or an array of strings
        if (value.ValueKind != JsonValueKind.String && value.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException($"Invalid 'type' keyword value: expected string or array, found {value.ValueKind}");

        if (value.ValueKind == JsonValueKind.Array && value.EnumerateArray().Any(v => v.ValueKind != JsonValueKind.String))
            throw new JsonSchemaException("Invalid 'type' keyword value: all elements in the array must be strings");

        var expectedTypes = value.ValueKind == JsonValueKind.String
            ? [value.GetString()!]
            : value.EnumerateArray().Select(v => v.GetString()!).ToList();

        var validTypes = new[] { "string", "number", "integer", "boolean", "null", "object", "array" };
        if (expectedTypes.Any(t => !validTypes.Contains(t)))
            throw new JsonSchemaException($"Invalid 'type' keyword value: contains invalid type(s) {string.Join(", ", expectedTypes.Except(validTypes))}");

        if (value.ValueKind == JsonValueKind.Array && !expectedTypes.Any())
            throw new JsonSchemaException("Invalid 'type' keyword value: array must contain at least one valid type");

        constraints["type"] = (instance, _) =>
        {
            var instanceType = instance.ValueKind switch
            {
                JsonValueKind.String => "string",
                JsonValueKind.Number => instance.TryGetDecimal(out var number) && number % 1 == 0 ? "integer" : "number",
                JsonValueKind.True or JsonValueKind.False => "boolean",
                JsonValueKind.Null => "null",
                JsonValueKind.Object => "object",
                JsonValueKind.Array => "array",
                _ => throw new JsonSchemaException($"Unexpected JsonValueKind: {instance.ValueKind}")
            };

            var isValid = expectedTypes.Contains(instanceType) || (expectedTypes.Contains("number") && instanceType == "integer");
            return new KeywordResult(
                isValid,
                isValid ? null : $"Expected type '{string.Join(", ", expectedTypes)}', but found '{instanceType}'",
                null
            );
        };
    }
}
