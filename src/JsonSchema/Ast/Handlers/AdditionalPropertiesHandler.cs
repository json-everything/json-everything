using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace Json.Schema.Ast;

public class AdditionalPropertiesHandler : IKeywordHandler
{
    public string Keyword => "additionalProperties";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["additionalProperties"] = keywordValue;
        
        // Capture defined properties
        List<string>? definedProperties = null;
        if (context.CurrentSchema?.TryGetProperty("properties", out var propertiesElement) == true && 
            propertiesElement.ValueKind == JsonValueKind.Object)
        {
            definedProperties = propertiesElement.EnumerateObject().Select(x => x.Name).ToList();
        }

        // Capture pattern properties
        List<string>? patternProperties = null;
        if (context.CurrentSchema?.TryGetProperty("patternProperties", out var patternPropertiesElement) == true && 
            patternPropertiesElement.ValueKind == JsonValueKind.Object)
        {
            patternProperties = patternPropertiesElement.EnumerateObject().Select(p => p.Name).ToList();
        }

        properties["additionalProperties.definedProperties"] = definedProperties;
        properties["additionalProperties.patternProperties"] = patternProperties;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Object)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("additionalProperties.definedProperties", out var definedPropsObj) ||
            !node.Properties.TryGetValue("additionalProperties.patternProperties", out var patternPropsObj))
            return KeywordResult.Skip;

        var definedProperties = definedPropsObj as List<string>;
        var patternProperties = patternPropsObj as List<string>;

        // Create regex patterns for pattern properties
        var patternRegexes = patternProperties?.Select(p => new Regex(p)).ToList() ?? new List<Regex>();

        // Filter dependencies to only include additional properties
        var additionalPropertyResults = dependencies.Where(dep => 
        {
            var location = dep.InstanceLocation;
            if (location.SegmentCount == 0) return false;
            
            var propertyName = location.GetSegment(location.SegmentCount - 1).ToString();
            
            // Skip if it's a defined property
            if (definedProperties?.Contains(propertyName) == true)
                return false;

            // Skip if it matches any pattern property
            foreach (var regex in patternRegexes)
            {
                if (regex.IsMatch(propertyName))
                    return false;
            }

            return true;
        }).ToList();

        // Check that all additional properties passed validation
        var allValid = additionalPropertyResults.All(dep => dep.IsValid);
        return new KeywordResult(
            allValid,
            allValid ? null : "All additional properties must match the subschema",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Create a child spec that validates all object properties (we'll filter in Evaluate)
        yield return new ChildSpec(
            InstancePath: JsonPointerHelpers.Wildcard,
            SchemaPath: JsonPointer.Empty,
            SubSchema: keywordValue
        );
    }
} 