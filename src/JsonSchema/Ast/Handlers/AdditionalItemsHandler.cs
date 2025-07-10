using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class AdditionalItemsHandler : IKeywordHandler
{
    public string Keyword => "additionalItems";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["additionalItems"] = keywordValue;
        
        // Store the items count for evaluation
        var itemsCount = 0;
        if (context.CurrentSchema?.TryGetProperty("items", out var itemsElement) == true && 
            itemsElement.ValueKind == JsonValueKind.Array)
        {
            itemsCount = itemsElement.GetArrayLength();
        }
        
        properties["additionalItems.itemsCount"] = itemsCount;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Array)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("additionalItems.itemsCount", out var itemsCountObj) || 
            itemsCountObj is not int itemsCount)
            return KeywordResult.Skip;

        // If items is not present or not an array, skip validation
        if (itemsCount == 0) 
            return KeywordResult.Skip;

        // Filter dependencies to only include additional items (indices >= itemsCount)
        var additionalItemResults = dependencies.Where(dep => 
        {
            var location = dep.InstanceLocation;
            if (location.SegmentCount == 0) return false;
            
            var lastSegment = location.GetSegment(location.SegmentCount - 1);
            if (lastSegment.ToInt() >= itemsCount)
                return true;
            
            return false;
        }).ToList();

        // Check that all additional array elements passed validation
        var allValid = additionalItemResults.All(dep => dep.IsValid);
        return new KeywordResult(
            allValid,
            allValid ? null : "All additional array elements must match the subschema",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Get the number of items schemas from the local schema
        var itemsCount = 0;
        if (context.CurrentSchema?.TryGetProperty("items", out var itemsElement) == true && 
            itemsElement.ValueKind == JsonValueKind.Array)
        {
            itemsCount = itemsElement.GetArrayLength();
        }

        // If items is not present or not an array, skip validation
        if (itemsCount == 0) 
            yield break;

        // Create a child spec that validates all array items (we'll filter in Evaluate)
        yield return new ChildSpec(
            InstancePath: JsonPointerHelpers.Wildcard,
            SchemaPath: JsonPointer.Empty,
            SubSchema: keywordValue
        );
    }
} 