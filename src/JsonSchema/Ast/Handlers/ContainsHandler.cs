using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class ContainsHandler : IKeywordHandler
{
    public string Keyword => "contains";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["contains"] = keywordValue;
        
        // Get minContains and maxContains from the local schema
        var minContains = 1; // Default value
        var maxContains = int.MaxValue; // Default value

        if (context.CurrentSchema?.TryGetProperty("minContains", out var minContainsElement) == true)
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

        if (context.CurrentSchema?.TryGetProperty("maxContains", out var maxContainsElement) == true)
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

        properties["contains.minContains"] = minContains;
        properties["contains.maxContains"] = maxContains;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Array)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("contains.minContains", out var minContainsObj) || 
            minContainsObj is not int minContains)
            minContains = 1;

        if (!node.Properties.TryGetValue("contains.maxContains", out var maxContainsObj) || 
            maxContainsObj is not int maxContains)
            maxContains = int.MaxValue;

        var array = instance.EnumerateArray().ToArray();
        if (array.Length == 0)
            return new KeywordResult(
                minContains == 0,
                minContains == 0 ? null : "Empty array does not contain any items matching the schema",
                null
            );

        // Count how many items match the contains schema based on dependency results
        var matches = dependencies.Count(dep => dep.IsValid);
        var isValid = matches >= minContains && matches <= maxContains;
        return new KeywordResult(
            isValid,
            isValid ? null : $"Array contains {matches} matching items, which is not within the required range of {minContains} to {maxContains}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Create a child spec that validates all array items
        yield return new ChildSpec(
            InstancePath: JsonPointerHelpers.Wildcard, // All array items
            SchemaPath: JsonPointer.Empty,
            SubSchema: keywordValue
        );
    }
} 