using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class AnyOfHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        // Validate that the keyword value is an array
        if (value.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException($"Invalid 'anyOf' keyword value: expected array, found {value.ValueKind}");

        var array = value.EnumerateArray().ToArray();
        if (array.Length == 0)
            throw new JsonSchemaException("Invalid 'anyOf' keyword value: array must contain at least one subschema");

        // Create subschemas for each element
        var subschemas = new JsonSchemaNode[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            var newContext = context with
            {
                SchemaPath = context.SchemaPath.Combine("anyOf", i),
                AdditionalSchemaPathFromParent = JsonPointerHelpers.GetCachedPointer(i),
                InstancePathFromParent = JsonPointer.Empty,
                FilterDependencyLocations = null
            };
            subschemas[i] = JsonSchema.BuildCore(array[i], newContext);
        }

        dependencies["anyOf"] = [.. subschemas];

        constraints["anyOf"] = (_, deps) =>
        {
            // Check if any of the dependency results are valid
            var anyValid = deps.Any(dep => dep.IsValid);

            return new KeywordResult(anyValid, anyValid ? null : "Instance must match at least one of the subschemas", null);
        };
    }
}