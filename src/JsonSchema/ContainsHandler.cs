using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class ContainsHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        // Get minContains and maxContains from the local schema
        var minContains = 1; // Default value
        var maxContains = int.MaxValue; // Default value

        if (context.LocalSchema.TryGetProperty("minContains", out var minContainsElement))
        {
            if (minContainsElement.ValueKind != JsonValueKind.Number || 
                !minContainsElement.TryGetDecimal(out var minContainsValue) || 
                minContainsValue < 0 || 
                minContainsValue % 1 != 0)
            {
                throw new JsonSchemaException($"Invalid 'minContains' keyword value: expected non-negative integer, found {minContainsElement.ValueKind}");
            }
            minContains = (int)minContainsValue;
        }

        if (context.LocalSchema.TryGetProperty("maxContains", out var maxContainsElement))
        {
            if (maxContainsElement.ValueKind != JsonValueKind.Number || 
                !maxContainsElement.TryGetDecimal(out var maxContainsValue) || 
                maxContainsValue < 0 || 
                maxContainsValue % 1 != 0)
            {
                throw new JsonSchemaException($"Invalid 'maxContains' keyword value: expected non-negative integer, found {maxContainsElement.ValueKind}");
            }
            maxContains = (int)maxContainsValue;
        }

        // Build the schema node for contains validation with proper paths
        var newContext = context with
        {
            CurrentInstanceLocation = context.CurrentInstanceLocation.Combine(JsonPointerHelpers.Wildcard),
            InstancePathFromParent = JsonPointerHelpers.Wildcard,
            SchemaPath = context.SchemaPath.Combine("contains"),
            AdditionalSchemaPathFromParent = JsonPointer.Empty,
            FilterDependencyLocations = null
        };
        var containsNode = JsonSchema.BuildCore(value, newContext);
        dependencies["contains"] = [containsNode];

        constraints["contains"] = (instance, deps) =>
        {
            if (instance.ValueKind != JsonValueKind.Array)
                return KeywordResult.Skip;

            var array = instance.EnumerateArray().ToArray();
            if (array.Length == 0)
                return new KeywordResult(
                    minContains == 0,
                    minContains == 0 ? null : "Empty array does not contain any items matching the schema",
                    null
                );

            // Count how many items match the contains schema based on dependency results
            var matches = deps.Count(dep => dep.IsValid);
            var isValid = matches >= minContains && matches <= maxContains;
            return new KeywordResult(
                isValid,
                isValid ? null : $"Array contains {matches} matching items, which is not within the required range of {minContains} to {maxContains}",
                null
            );
        };
    }
} 