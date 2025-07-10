using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class DependentRequiredHandler : IKeywordHandler
{
    public string Keyword => "dependentRequired";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'dependentRequired' keyword value: expected object, found {keywordValue.ValueKind}");

        var propertyDependencies = new Dictionary<string, List<string>>();
        foreach (var property in keywordValue.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Array)
                throw new JsonSchemaException($"Invalid 'dependentRequired' keyword value: expected array for property '{property.Name}', found {property.Value.ValueKind}");

            var dependentProperties = property.Value.EnumerateArray()
                .Select(v => v.ValueKind == JsonValueKind.String ? v.GetString()! : throw new JsonSchemaException($"Invalid 'dependentRequired' keyword value: all elements must be strings for property '{property.Name}'"))
                .ToList();

            propertyDependencies[property.Name] = dependentProperties;
        }

        properties["dependentRequired"] = propertyDependencies;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Object)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("dependentRequired", out var dependenciesObj) || 
            dependenciesObj is not Dictionary<string, List<string>> propertyDependencies)
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

        var isValid = missingDependencies.Count == 0;
        return new KeywordResult(
            isValid,
            isValid ? null : string.Join("; ", missingDependencies),
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // DependentRequired keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 