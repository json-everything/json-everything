using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class PropertiesHandler : IKeywordHandler
{
    public string Keyword => "properties";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["properties"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Object)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("properties", out var propertiesObj) || propertiesObj is not JsonElement propertiesElement)
            return KeywordResult.Skip;

        // All dependencies should be valid (property validations)
        foreach (var dep in dependencies)
        {
            if (!dep.IsValid)
                return new KeywordResult(false, $"Property validation failed", null);
        }

        // Create annotation with evaluated properties
        var evaluatedProperties = new List<string>();
        foreach (var dep in dependencies)
        {
            var propertyName = ExtractPropertyName(dep.InstanceLocation);
            if (!string.IsNullOrEmpty(propertyName))
                evaluatedProperties.Add(propertyName);
        }

        if (evaluatedProperties.Count > 0)
        {
            var annotation = JsonSerializer.SerializeToElement(evaluatedProperties);
            return new KeywordResult(true, null, annotation);
        }

        return new KeywordResult(true, null, null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Each property in the properties object becomes a child node
        foreach (var property in keywordValue.EnumerateObject())
        {
            yield return new ChildSpec(
                InstancePath: JsonPointer.Create(property.Name),
                SchemaPath: JsonPointer.Create(property.Name),
                SubSchema: property.Value
            );
        }
    }

    private static string ExtractPropertyName(JsonPointer instanceLocation)
    {
        if (instanceLocation.SegmentCount == 0) return string.Empty;
        return instanceLocation.GetSegment(instanceLocation.SegmentCount - 1).ToString();
    }
} 