using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Json.Schema;

public sealed class MultipleOfHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Number)
            throw new JsonSchemaException($"Invalid 'multipleOf' keyword value: expected number, found {value.ValueKind}");

        var multipleOf = value.GetDecimal();
        if (multipleOf <= 0)
            throw new JsonSchemaException("Invalid 'multipleOf' keyword value: must be greater than 0");

        constraints["multipleOf"] = (instance, _) =>
        {
            if (instance.ValueKind != JsonValueKind.Number)
                return KeywordResult.Skip;

            var number = instance.GetDecimal();
            var isValid = number % multipleOf == 0;
            return new KeywordResult(
                isValid,
                isValid ? null : $"Value {number} is not a multiple of {multipleOf}",
                null
            );
        };
    }
} 