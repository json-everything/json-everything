using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

public sealed class MaxItemsHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var maxItems) || maxItems < 0 || maxItems % 1 != 0)
            throw new JsonSchemaException($"Invalid 'maxItems' keyword value: expected non-negative integer, found {value.ValueKind}");

        var maxItemsInt = (int)maxItems;
        constraints["maxItems"] = (instance, _) =>
        {
            if (instance.ValueKind != JsonValueKind.Array)
                return KeywordResult.Skip;

            var array = instance.EnumerateArray().ToArray();
            var isValid = array.Length <= maxItemsInt;
            return new KeywordResult(
                isValid,
                isValid ? null : $"Array length {array.Length} is greater than maximum length {maxItemsInt}",
                null
            );
        };
    }
} 