using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class DependentSchemasHandler : IKeywordHandler
{
    public string Keyword => "dependentSchemas";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'dependentSchemas' keyword value: expected object, found {keywordValue.ValueKind}");

        properties["dependentSchemas"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Object)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("dependentSchemas", out var dependentSchemasObj) || 
            dependentSchemasObj is not JsonElement dependentSchemasElement)
            return KeywordResult.Skip;

        var instanceProperties = new HashSet<string>(instance.EnumerateObject()
            .Select(p => p.Name));

        // Group dependencies by their schema path (property name)
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

        // Only validate schemas for properties that are present in the instance
        var relevantResults = new List<EvaluationOutput>();
        foreach (var kvp in dependenciesByProperty)
        {
            var propertyName = kvp.Key;
            var propertyDependencies = kvp.Value;
            
            // Only include dependencies if the trigger property is present
            if (instanceProperties.Contains(propertyName))
            {
                relevantResults.AddRange(propertyDependencies);
            }
        }

        if (relevantResults.Count == 0)
            return KeywordResult.Skip;

        var isValid = relevantResults.All(r => r.IsValid);
        return new KeywordResult(
            isValid,
            isValid ? null : "Instance must validate against the dependent schema when the specified property is present",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        if (keywordValue.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'dependentSchemas' keyword value: expected object, found {keywordValue.ValueKind}");

        foreach (var property in keywordValue.EnumerateObject())
        {
            // Create a child spec for each dependent schema
            yield return new ChildSpec(
                InstancePath: JsonPointer.Empty, // Apply to the same instance (object level)
                SchemaPath: JsonPointer.Create(property.Name),
                SubSchema: property.Value
            );
        }
    }
} 