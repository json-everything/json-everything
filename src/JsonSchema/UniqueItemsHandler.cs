using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

public sealed class UniqueItemsHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.True && value.ValueKind != JsonValueKind.False)
            throw new JsonSchemaException($"Invalid 'uniqueItems' keyword value: expected boolean, found {value.ValueKind}");

        if (value.ValueKind == JsonValueKind.False)
        {
            // If uniqueItems is false, we don't need to add any constraints
            return;
        }

        constraints["uniqueItems"] = (instance, _) =>
        {
            if (instance.ValueKind != JsonValueKind.Array)
                return KeywordResult.Skip;

            var array = instance.EnumerateArray().ToArray();
            if (array.Length <= 1)
                return KeywordResult.Skip;

            // Compare each element with all subsequent elements
            for (int i = 0; i < array.Length - 1; i++)
            {
                var current = array[i];
                var rest = array.Skip(i + 1);
                if (rest.Any(x => x.DeepEquals(current)))
                {
                    var duplicateIndex = Array.FindIndex(array, i + 1, x => x.DeepEquals(current));
                    return new KeywordResult(
                        false,
                        $"Array contains duplicate items at indices {i} and {duplicateIndex}",
                        null
                    );
                }
            }

            return new KeywordResult(true, null, null);
        };
    }
} 