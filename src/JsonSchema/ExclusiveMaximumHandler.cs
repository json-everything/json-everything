using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Json.Schema;

public sealed class ExclusiveMaximumHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Number)
            throw new JsonSchemaException($"Invalid 'exclusiveMaximum' keyword value: expected number, found {value.ValueKind}");

        var exclusiveMaximum = value.GetDecimal();

        constraints["exclusiveMaximum"] = (instance, _) =>
        {
            if (instance.ValueKind != JsonValueKind.Number)
                return KeywordResult.Skip;

            var number = instance.GetDecimal();
            var isValid = number < exclusiveMaximum;
            return new KeywordResult(
                isValid,
                isValid ? null : $"Value {number} is not less than exclusive maximum {exclusiveMaximum}",
                null
            );
        };
    }
} 