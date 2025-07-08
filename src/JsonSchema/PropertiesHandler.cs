using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class PropertiesHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        // Validate that the keyword value is an object
        if (value.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'properties' keyword value: expected object, found {value.ValueKind}");

        var nodes = new Dictionary<string, JsonSchemaNode>();
        var propertyNames = new HashSet<string>();

        foreach (var prop in value.EnumerateObject())
        {
            var propName = prop.Name;
            var propValue = prop.Value;

            var newContext = context with
            {
                CurrentInstanceLocation = context.CurrentInstanceLocation.Combine(propName),
                InstancePathFromParent = JsonPointerHelpers.GetCachedPointer(propName),
                SchemaPath = context.SchemaPath.Combine("properties", propName),
                AdditionalSchemaPathFromParent = JsonPointerHelpers.GetCachedPointer(propName),
                FilterDependencyLocations = null
            };

            nodes[propName] = JsonSchema.BuildCore(propValue, newContext);
            propertyNames.Add(propName);
        }

        dependencies["properties"] = [.. nodes.Values];

        constraints["properties"] = (instance, deps) =>
        {
            var foundProperties = new HashSet<string>();
            if (instance.ValueKind is JsonValueKind.Object)
            {
                foreach (var kvp in instance.EnumerateObject())
                {
                    if (!propertyNames.Contains(kvp.Name)) continue;

                    foundProperties.Add(kvp.Name);
                }
            }

            var allValid = deps.All(dep => dep.IsValid);
            return new KeywordResult(
                allValid,
                allValid ? null : "All properties must match their subschemas",
                foundProperties.Count == 0 ? null : JsonSerializer.SerializeToElement(foundProperties)
            );
        };
    }
}