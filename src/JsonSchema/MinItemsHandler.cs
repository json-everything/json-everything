using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

public sealed class MinItemsHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var minItems) || minItems < 0 || minItems % 1 != 0)
            throw new JsonSchemaException($"Invalid 'minItems' keyword value: expected non-negative integer, found {value.ValueKind}");

        var minItemsInt = (int)minItems;
        constraints["minItems"] = (instance, _) =>
        {
            if (instance.ValueKind != JsonValueKind.Array)
                return KeywordResult.Skip;

            var array = instance.EnumerateArray().ToArray();
            var isValid = array.Length >= minItemsInt;
            return new KeywordResult(
                isValid,
                isValid ? null : $"Array length {array.Length} is less than minimum length {minItemsInt}",
                null
            );
        };
    }
} 