using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class AdditionalItemsHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        // Get the number of items schemas from the local schema
        var itemsCount = 0;
        if (context.LocalSchema.TryGetProperty("items", out var itemsElement) && 
            itemsElement.ValueKind == JsonValueKind.Array) 
	        itemsCount = itemsElement.GetArrayLength();

        // If items is not present or not an array, skip validation
        if (itemsCount == 0) return;

        var newContext = context with
        {
            CurrentInstanceLocation = context.CurrentInstanceLocation.Combine(JsonPointerHelpers.Wildcard),
            InstancePathFromParent = JsonPointerHelpers.Wildcard,
            SchemaPath = context.SchemaPath.Combine("additionalItems"),
            AdditionalSchemaPathFromParent = JsonPointer.Empty,
            FilterDependencyLocations = (pointer, _, parent) => 
            {
                if (parent.ValueKind != JsonValueKind.Array) return false;

                if (pointer.SegmentCount != 1) return false;

                var index = pointer[0].ToInt();
                return index >= itemsCount;
            }
        };

        var node = JsonSchema.BuildCore(value, newContext);
        dependencies["additionalItems"] = [node];

        constraints["additionalItems"] = (instance, deps) =>
        {
            if (instance.ValueKind != JsonValueKind.Array)
                return KeywordResult.Skip;

            // Check that all array elements after items schemas passed validation
            var allValid = deps.All(dep => dep.IsValid);
            return new KeywordResult(
                allValid, 
                allValid ? null : "All additional array elements must match the subschema", 
                null
            );
        };
    }
}