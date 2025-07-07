using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

public sealed class MaxPropertiesHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var maxProperties) || maxProperties < 0 || maxProperties % 1 != 0)
            throw new JsonSchemaException($"Invalid 'maxProperties' keyword value: expected non-negative integer, found {value.ValueKind}");

        var maxPropertiesInt = (int)maxProperties;
        constraints["maxProperties"] = (instance, _) =>
        {
            if (instance.ValueKind != JsonValueKind.Object)
                return KeywordResult.Skip;

            var propertyCount = instance.EnumerateObject().Count();
            var isValid = propertyCount <= maxPropertiesInt;
            return new KeywordResult(
                isValid,
                isValid ? null : $"Object must have at most {maxPropertiesInt} properties, but has {propertyCount}",
                null
            );
        };
    }
} 