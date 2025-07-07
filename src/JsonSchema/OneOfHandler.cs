using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class OneOfHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException($"Invalid 'oneOf' keyword value: expected array, found {value.ValueKind}");

        var array = value.EnumerateArray().ToArray();
        if (array.Length == 0)
            throw new JsonSchemaException("Invalid 'oneOf' keyword value: array must contain at least one subschema");

        var subschemas = new JsonSchemaNode[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            var newContext = context with
            {
                SchemaPath = context.SchemaPath.Combine("oneOf", i),
                AdditionalSchemaPathFromParent = JsonPointer.Create(i),
                InstancePathFromParent = JsonPointer.Empty,
                FilterDependencyLocations = null
            };
            subschemas[i] = JsonSchema.BuildCore(array[i], newContext);
        }

        dependencies["oneOf"] = [.. subschemas];

        constraints["oneOf"] = (_, deps) =>
        {
            var validCount = deps.Count(dep => dep.IsValid);
            var isValid = validCount == 1;
            return new KeywordResult(
                isValid,
                isValid ? null : $"Instance must match exactly one subschema, but matched {validCount}",
                null
            );
        };
    }
} 