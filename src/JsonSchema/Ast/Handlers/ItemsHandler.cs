using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;
using Json.Schema;

namespace Json.Schema.Ast;

public class ItemsHandler : IKeywordHandler
{
    public string Keyword => "items";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["items"] = keywordValue;
        properties["items.isArray"] = keywordValue.ValueKind == JsonValueKind.Array;
        
        if (keywordValue.ValueKind == JsonValueKind.Array)
        {
            properties["items.schemaCount"] = keywordValue.GetArrayLength();
        }
        else
        {
            // Check for prefixItems to know how many items to skip
            var prefixItemsCount = 0;
            if (context.CurrentSchema?.TryGetProperty("prefixItems", out var prefixItemsElement) == true &&
                prefixItemsElement.ValueKind == JsonValueKind.Array)
            {
                prefixItemsCount = prefixItemsElement.GetArrayLength();
            }
            properties["items.prefixItemsCount"] = prefixItemsCount;
        }
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Array)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("items", out var itemsObj) || itemsObj is not JsonElement itemsElement)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("items.isArray", out var isArrayObj) || isArrayObj is not bool isArray)
            return KeywordResult.Skip;

        var array = instance.EnumerateArray().ToArray();
        
        if (isArray)
        {
            // Array form - validate items by position
            if (!node.Properties.TryGetValue("items.schemaCount", out var schemaCountObj) || schemaCountObj is not int schemaCount)
                return KeywordResult.Skip;

            if (array.Length == 0)
                return new KeywordResult(true, null, JsonSerializer.SerializeToElement(0));

            // Only validate up to the number of item schemas
            var itemsToValidate = Math.Min(array.Length, schemaCount);
            var relevantDependencies = dependencies.Take(itemsToValidate).ToList();
            var allValid = relevantDependencies.All(dep => dep.IsValid);

            return new KeywordResult(
                allValid,
                allValid ? null : "One or more items do not match their corresponding schemas",
                JsonSerializer.SerializeToElement(schemaCount)
            );
        }
        else
        {
            // Single schema form - validate items after prefixItems
            if (!node.Properties.TryGetValue("items.prefixItemsCount", out var prefixCountObj) || prefixCountObj is not int prefixItemsCount)
                return KeywordResult.Skip;

            if (array.Length <= prefixItemsCount)
                return new KeywordResult(true, null, JsonSerializer.SerializeToElement(true));

            // Filter dependencies to only include items after prefixItems
            var relevantDependencies = dependencies.Where(dep => 
            {
                var location = dep.InstanceLocation;
                if (location.SegmentCount == 0) return false;
                
                var lastSegment = location.GetSegment(location.SegmentCount - 1);
                if (lastSegment.ToInt() >= prefixItemsCount)
                    return true;
                
                return false;
            }).ToList();

            // Check that all array elements after prefixItems passed validation
            var allValid = relevantDependencies.All(dep => dep.IsValid);
            return new KeywordResult(
                allValid,
                allValid ? null : "All array elements after prefix items must match the subschema",
                JsonSerializer.SerializeToElement(true)
            );
        }
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        if (keywordValue.ValueKind == JsonValueKind.Array)
        {
            // Array form - create child spec for each schema
            var schemas = keywordValue.EnumerateArray().ToArray();
            if (schemas.Length == 0)
                yield break; // No items to validate

            for (int i = 0; i < schemas.Length; i++)
            {
                yield return new ChildSpec(
                    InstancePath: JsonPointer.Create(i),
                    SchemaPath: JsonPointer.Create(i),
                    SubSchema: schemas[i]
                );
            }
        }
        else
        {
            // Single schema form - create child spec with wildcard (for items after prefixItems)
            var prefixItemsCount = 0;
            if (context.CurrentSchema?.TryGetProperty("prefixItems", out var prefixItemsElement) == true &&
                prefixItemsElement.ValueKind == JsonValueKind.Array)
            {
                prefixItemsCount = prefixItemsElement.GetArrayLength();
            }

            // Single schema form - apply to all items (we'll filter in Evaluate)
            yield return new ChildSpec(
                InstancePath: JsonPointerHelpers.Wildcard,
                SchemaPath: JsonPointer.Empty,
                SubSchema: keywordValue
            );
        }
    }
} 