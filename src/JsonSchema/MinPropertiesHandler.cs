using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

public sealed class MinPropertiesHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var minProperties) || minProperties < 0 || minProperties % 1 != 0)
            throw new JsonSchemaException($"Invalid 'minProperties' keyword value: expected non-negative integer, found {value.ValueKind}");

        var minPropertiesInt = (int)minProperties;
        constraints["minProperties"] = (instance, _) =>
        {
            if (instance.ValueKind != JsonValueKind.Object)
                return KeywordResult.Skip;

            var propertyCount = instance.EnumerateObject().Count();
            var isValid = propertyCount >= minPropertiesInt;
            return new KeywordResult(
                isValid,
                isValid ? null : $"Object must have at least {minPropertiesInt} properties, but has {propertyCount}",
                null
            );
        };
    }
} 