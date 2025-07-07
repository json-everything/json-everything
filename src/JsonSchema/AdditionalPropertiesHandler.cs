using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace Json.Schema;

public sealed class AdditionalPropertiesHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        // Capture defined properties
        List<string>? definedProperties = null;
        if (context.LocalSchema.TryGetProperty("properties", out var propertiesElement) && 
            propertiesElement.ValueKind == JsonValueKind.Object)
        {
            definedProperties = propertiesElement.EnumerateObject().Select(x => x.Name).ToList();
        }

        // Capture pattern properties
        var patternRegexes = new List<Regex>();
        if (context.LocalSchema.TryGetProperty("patternProperties", out var patternPropertiesElement) && 
            patternPropertiesElement.ValueKind == JsonValueKind.Object)
        {
            patternRegexes = [..patternPropertiesElement.EnumerateObject().Select(p => new Regex(p.Name))];
        }

        var newContext = context with
        {
            CurrentInstanceLocation = context.CurrentInstanceLocation.Combine(JsonPointerHelpers.Wildcard),
            InstancePathFromParent = JsonPointerHelpers.Wildcard,
            SchemaPath = context.SchemaPath.Combine("additionalProperties"),
            AdditionalSchemaPathFromParent = JsonPointer.Empty,
            FilterDependencyLocations = (pointer, _, parent) =>
            {
                if (parent.ValueKind != JsonValueKind.Object)
                    return false;

                if (pointer.SegmentCount != 1)
                    return false;

                var propertyName = pointer[0].ToString();
                if (definedProperties != null && definedProperties.Contains(propertyName))
                    return false;

                foreach (var regex in patternRegexes)
                {
                    if (regex.IsMatch(propertyName))
                        return false;
                }

                return true;
            }
        };

        var node = JsonSchema.BuildCore(value, newContext);
        dependencies["additionalProperties"] = [node];

        constraints["additionalProperties"] = (instance, deps) =>
        {
            if (instance.ValueKind != JsonValueKind.Object)
                return KeywordResult.Skip;

            var allValid = deps.All(dep => dep.IsValid);
            return new KeywordResult(
                allValid,
                allValid ? null : "All additional properties must match the subschema",
                null
            );
        };
    }
} 