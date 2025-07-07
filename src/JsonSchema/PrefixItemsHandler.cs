using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class PrefixItemsHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException($"Invalid 'prefixItems' keyword value: expected array, found {value.ValueKind}");

        var schemas = value.EnumerateArray().ToArray();
        if (schemas.Length == 0)
            return; // No items to validate

        var prefixNodes = new JsonSchemaNode[schemas.Length];
        for (int i = 0; i < schemas.Length; i++)
        {
            var newContext = context with
            {
                CurrentInstanceLocation = context.CurrentInstanceLocation.Combine(i),
                InstancePathFromParent = JsonPointer.Create(i),
                SchemaPath = context.SchemaPath.Combine("prefixItems", i),
                AdditionalSchemaPathFromParent = JsonPointer.Create(i),
                FilterDependencyLocations = null
            };
            prefixNodes[i] = JsonSchema.BuildCore(schemas[i], newContext);
        }

        dependencies["prefixItems"] = [..prefixNodes];

        constraints["prefixItems"] = (instance, deps) =>
        {
            if (instance.ValueKind != JsonValueKind.Array)
                return KeywordResult.Skip;

            var array = instance.EnumerateArray().ToArray();
            if (array.Length == 0)
                return new KeywordResult(true, null, JsonSerializer.SerializeToElement(0));

            // Only validate up to the number of prefix schemas
            var itemsToValidate = Math.Min(array.Length, prefixNodes.Length);
            var allValid = deps.Take(itemsToValidate).All(dep => dep.IsValid);
            
            return new KeywordResult(
                allValid,
                allValid ? null : "One or more prefix items do not match their corresponding schemas",
                JsonSerializer.SerializeToElement(prefixNodes.Length)
            );
        };
    }
} 