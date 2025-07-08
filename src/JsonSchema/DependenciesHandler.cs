using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class DependenciesHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'dependencies' keyword value: expected object, found {value.ValueKind}");

        var schemaDependencies = new Dictionary<string, JsonSchemaNode>();
        var propertyDependencies = new Dictionary<string, List<string>>();

        foreach (var property in value.EnumerateObject())
        {
            var dependency = property.Value;
            
            if (value.ValueKind == JsonValueKind.Array)
            {
                var dependentProperties = dependency.EnumerateArray()
                    .Select(v => v.ValueKind == JsonValueKind.String
                        ? v.GetString()!
                        : throw new JsonSchemaException($"Invalid 'dependencies' keyword value: all elements must be strings for property '{property.Name}'")
                    )
                    .ToList();

                propertyDependencies[property.Name] = dependentProperties;
            }
            else
            {
                var node = JsonSchema.BuildCore(dependency, context with
                {
                    SchemaPath = context.SchemaPath.Combine("dependencies", property.Name),
                    AdditionalSchemaPathFromParent = JsonPointerHelpers.GetCachedPointer(property.Name),
                    InstancePathFromParent = JsonPointer.Empty,
                    FilterDependencyLocations = (_, instance, _) => 
                        instance.ValueKind == JsonValueKind.Object && 
                        instance.EnumerateObject().Any(p => p.Name == property.Name)
                });
                schemaDependencies[property.Name] = node;
            }
        }

        if (schemaDependencies.Count > 0)
        {
            dependencies["dependencies"] = [..schemaDependencies.Values];
        }

        constraints["dependencies"] = (instance, deps) =>
        {
            if (instance.ValueKind != JsonValueKind.Object)
                return KeywordResult.Skip;

            var instanceProperties = instance.EnumerateObject()
                .Select(p => p.Name)
                .ToList();

            var missingDependencies = new List<string>();
            foreach (var kvp in propertyDependencies)
            {
	            var property = kvp.Key;
	            var requiredProperties = kvp.Value;
                if (instanceProperties.Contains(property))
                {
                    var missing = requiredProperties
                        .Where(prop => !instanceProperties.Contains(prop))
                        .ToList();

                    if (missing.Count > 0) 
                        missingDependencies.Add($"When property '{property}' is present, the following properties must also be present: {string.Join(", ", missing)}");
                }
            }

            var schemaResults = new List<EvaluationOutput>();
            foreach (var dep in deps)
            {
                schemaResults.Add(dep);
            }

            var schemaValid = schemaResults.Count == 0 || schemaResults.All(r => r.IsValid);
            var propertyValid = missingDependencies.Count == 0;

            var isValid = schemaValid && propertyValid;
            var errorMessage = !isValid
                ? string.Join("; ", missingDependencies)
                : null;

            return new KeywordResult(isValid, errorMessage, null);
        };
    }
} 