using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

public sealed class RequiredHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException($"Invalid 'required' keyword value: expected array, found {value.ValueKind}");

        var requiredProperties = value.EnumerateArray()
            .Select(v => v.ValueKind == JsonValueKind.String ? v.GetString()! : throw new JsonSchemaException("Invalid 'required' keyword value: all elements must be strings"))
            .ToList();

        constraints["required"] = (instance, _) =>
        {
            if (instance.ValueKind != JsonValueKind.Object)
                return KeywordResult.Skip;

            var instanceProperties = instance.EnumerateObject()
                .Select(x => x.Name)
                .ToList();

            var missingProperties = requiredProperties
                .Where(prop => instanceProperties.All(p => p != prop))
                .ToList();

            var isValid = missingProperties.Count == 0;
            return new KeywordResult(
                isValid,
                isValid ? null : $"Required properties missing: {string.Join(", ", missingProperties)}",
                null
            );
        };
    }
} 