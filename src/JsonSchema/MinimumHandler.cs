using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Json.Schema;

public sealed class MinimumHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        // Validate that the keyword value is a number
        if (value.ValueKind != JsonValueKind.Number)
            throw new JsonSchemaException($"Invalid 'minimum' keyword value: expected number, found {value.ValueKind}");

        var minimum = value.GetDecimal();

        constraints["minimum"] = (instance, _) =>
        {
            // Only validate if the instance is a number
            if (instance.ValueKind != JsonValueKind.Number)
                return KeywordResult.Skip;

            var number = instance.GetDecimal();
            var isValid = number >= minimum;
            return new KeywordResult(
                isValid,
                isValid ? null : $"Value {number} is less than minimum {minimum}",
                null
            );
        };
    }
} 