using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class ItemsHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind == JsonValueKind.Array)
        {
            // Array form - similar to prefixItems but don't check for prefixItems
            var schemas = value.EnumerateArray().ToArray();
            if (schemas.Length == 0)
                return; // No items to validate

            var itemNodes = new JsonSchemaNode[schemas.Length];
            for (int i = 0; i < schemas.Length; i++)
            {
                var newContext = context with
                {
                    CurrentInstanceLocation = context.CurrentInstanceLocation.Combine(i),
                    InstancePathFromParent = JsonPointer.Create(i),
                    SchemaPath = context.SchemaPath.Combine("items", i),
                    AdditionalSchemaPathFromParent = JsonPointer.Create(i),
                    FilterDependencyLocations = (_, _, parent) =>
                    {
                        if (parent.ValueKind != JsonValueKind.Array)
                            return false;

                        return true;
                    }
                };
                itemNodes[i] = JsonSchema.BuildCore(schemas[i], newContext);
            }

            dependencies["items"] = [.. itemNodes];

            constraints["items"] = (instance, deps) =>
            {
                if (instance.ValueKind != JsonValueKind.Array)
                    return KeywordResult.Skip;

                var array = instance.EnumerateArray().ToArray();
                if (array.Length == 0)
                    return new KeywordResult(true, null, JsonSerializer.SerializeToElement(0));

                // Only validate up to the number of item schemas
                var itemsToValidate = Math.Min(array.Length, itemNodes.Length);
                var allValid = deps.Take(itemsToValidate).All(dep => dep.IsValid);

                return new KeywordResult(
                    allValid,
                    allValid ? null : "One or more items do not match their corresponding schemas",
                    JsonSerializer.SerializeToElement(itemNodes.Length)
                );
            };
        }
        else
        {
            var prefixItemsCount = 0;
            if (context.LocalSchema.TryGetProperty("prefixItems", out var prefixItemsElement) &&
                prefixItemsElement.ValueKind == JsonValueKind.Array)
            {
                prefixItemsCount = prefixItemsElement.GetArrayLength();
            }

            var newContext = context with
            {
                CurrentInstanceLocation = context.CurrentInstanceLocation.Combine(JsonPointerHelpers.Wildcard),
                InstancePathFromParent = JsonPointerHelpers.Wildcard,
                SchemaPath = context.SchemaPath.Combine("items"),
                AdditionalSchemaPathFromParent = JsonPointer.Empty,
                FilterDependencyLocations = prefixItemsCount > 0
                    ? (pointer, _, parent) =>
                    {
                        if (parent.ValueKind != JsonValueKind.Array)
                            return false;

                        if (pointer.SegmentCount != 1)
                            return false;
                        var index = pointer[0].ToInt();
                        return index >= prefixItemsCount;
                    }
                : null
            };

            var node = JsonSchema.BuildCore(value, newContext);
            dependencies["items"] = [node];

            constraints["items"] = (instance, deps) =>
            {
                if (instance.ValueKind != JsonValueKind.Array)
                    return KeywordResult.Skip;

                var array = instance.EnumerateArray().ToArray();
                if (array.Length <= prefixItemsCount)
                    return new KeywordResult(true, null, JsonSerializer.SerializeToElement(true));

                // Check that all array elements after prefixItems passed validation
                var allValid = deps.All(dep => dep.IsValid);
                return new KeywordResult(
                    allValid,
                    allValid ? null : "All array elements after prefix items must match the subschema",
                    JsonSerializer.SerializeToElement(true)
                );
            };
        }
    }
}