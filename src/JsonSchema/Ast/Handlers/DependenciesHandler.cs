using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class DependenciesHandler : IKeywordHandler
{
    public string Keyword => "dependencies";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'dependencies' keyword value: expected object, found {keywordValue.ValueKind}");

        var schemaDependencies = new Dictionary<string, JsonElement>();
        var propertyDependencies = new Dictionary<string, List<string>>();

        foreach (var property in keywordValue.EnumerateObject())
        {
            var dependency = property.Value;
            
            if (dependency.ValueKind == JsonValueKind.Array)
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
                schemaDependencies[property.Name] = dependency;
            }
        }

        properties["dependencies.schemaDependencies"] = schemaDependencies;
        properties["dependencies.propertyDependencies"] = propertyDependencies;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Object)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("dependencies.schemaDependencies", out var schemaDepsObj) ||
            !node.Properties.TryGetValue("dependencies.propertyDependencies", out var propertyDepsObj) ||
            schemaDepsObj is not Dictionary<string, JsonElement> schemaDependencies ||
            propertyDepsObj is not Dictionary<string, List<string>> propertyDependencies)
            return KeywordResult.Skip;

        var instanceProperties = new HashSet<string>(instance.EnumerateObject()
            .Select(p => p.Name));

        // Check property dependencies
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

        // Check schema dependencies
        var dependenciesByProperty = new Dictionary<string, List<EvaluationOutput>>();
        foreach (var dep in dependencies)
        {
            var schemaPath = dep.EvaluationPath;
            if (schemaPath.SegmentCount > 0)
            {
                var propertyName = schemaPath.GetSegment(schemaPath.SegmentCount - 1).ToString();
                if (!dependenciesByProperty.ContainsKey(propertyName))
                    dependenciesByProperty[propertyName] = new List<EvaluationOutput>();
                dependenciesByProperty[propertyName].Add(dep);
            }
        }

        var relevantSchemaResults = new List<EvaluationOutput>();
        foreach (var kvp in dependenciesByProperty)
        {
            var propertyName = kvp.Key;
            var propertyResults = kvp.Value;
            
            // Only include dependencies if the trigger property is present
            if (instanceProperties.Contains(propertyName))
            {
                relevantSchemaResults.AddRange(propertyResults);
            }
        }

        var propertyValid = missingDependencies.Count == 0;
        var schemaValid = relevantSchemaResults.Count == 0 || relevantSchemaResults.All(r => r.IsValid);

        var isValid = propertyValid && schemaValid;
        var errorMessage = !isValid
            ? string.Join("; ", missingDependencies)
            : null;

        return new KeywordResult(isValid, errorMessage, null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        if (keywordValue.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'dependencies' keyword value: expected object, found {keywordValue.ValueKind}");

        foreach (var property in keywordValue.EnumerateObject())
        {
            var dependency = property.Value;
            
            // Only create child specs for schema dependencies (not property dependencies)
            if (dependency.ValueKind != JsonValueKind.Array)
            {
                yield return new ChildSpec(
                    InstancePath: JsonPointer.Empty, // Apply to the same instance (object level)
                    SchemaPath: JsonPointer.Create(property.Name),
                    SubSchema: dependency
                );
            }
        }
    }
} 