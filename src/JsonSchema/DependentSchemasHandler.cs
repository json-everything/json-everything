using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class DependentSchemasHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'dependentSchemas' keyword value: expected object, found {value.ValueKind}");

        var propertyDependencies = new Dictionary<string, JsonSchemaNode>();
        foreach (var property in value.EnumerateObject())
        {
            var node = JsonSchema.BuildCore(property.Value, context with
            {
                SchemaPath = context.SchemaPath.Combine("dependentSchemas", property.Name),
                AdditionalSchemaPathFromParent = JsonPointerHelpers.GetCachedPointer(property.Name),
                InstancePathFromParent = JsonPointer.Empty,
                FilterDependencyLocations = (_, instance, _) => 
                    instance.ValueKind == JsonValueKind.Object && 
                    instance.EnumerateObject().Any(p => p.Name == property.Name)
            });
            propertyDependencies[property.Name] = node;
        }

        dependencies["dependentSchemas"] = [..propertyDependencies.Values];

        constraints["dependentSchemas"] = (instance, deps) =>
        {
            if (instance.ValueKind != JsonValueKind.Object)
                return KeywordResult.Skip;

            var results = new List<EvaluationOutput>();
            foreach (var dep in deps)
            {
                results.Add(dep);
            }

            if (results.Count == 0)
                return KeywordResult.Skip;

            var isValid = results.All(r => r.IsValid);
            return new KeywordResult(
                isValid,
                isValid ? null : "Instance must validate against the dependent schema when the specified property is present",
                null
            );
        };
    }
} 