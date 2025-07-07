using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

public sealed class EnumHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException($"Invalid 'enum' keyword value: expected array, found {value.ValueKind}");

        var enumValues = value.EnumerateArray().ToList();
        
        constraints["enum"] = (instance, _) =>
        {
            foreach (var enumValue in enumValues)
            {
                if (instance.DeepEquals(enumValue))
                {
                    return new KeywordResult(true, null, null);
                }
            }

            return new KeywordResult(
                false,
                "Value must match one of the specified enum values",
                null
            );
        };
    }
} 