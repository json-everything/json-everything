using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace Json.Schema;

public sealed class PatternPropertiesHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'patternProperties' keyword value: expected object, found {value.ValueKind}");

        var patternNodes = new Dictionary<Regex, JsonSchemaNode>();
        foreach (var property in value.EnumerateObject())
        {
            var regex = new Regex(property.Name, RegexOptions.ECMAScript);
            var newContext = context with
            {
                CurrentInstanceLocation = context.CurrentInstanceLocation.Combine(JsonPointerHelpers.Wildcard),
                InstancePathFromParent = JsonPointerHelpers.Wildcard,
                SchemaPath = context.SchemaPath.Combine("patternProperties", property.Name),
                AdditionalSchemaPathFromParent = JsonPointer.Empty,
                FilterDependencyLocations = (pointer, _, parent) =>
                {
                    if (parent.ValueKind != JsonValueKind.Object) return false;

                    if (pointer.SegmentCount != 1) return false;

                    var propertyName = pointer[0].ToString();
                    return regex.IsMatch(propertyName);
                }
            };

            var node = JsonSchema.BuildCore(property.Value, newContext);
            patternNodes[regex] = node;
        }

        dependencies["patternProperties"] = [..patternNodes.Values];

        constraints["patternProperties"] = (instance, deps) =>
        {
            if (instance.ValueKind != JsonValueKind.Object)
                return KeywordResult.Skip;

            // Check that all properties matching any pattern passed validation
            var allValid = deps.All(dep => dep.IsValid);
            return new KeywordResult(
                allValid,
                allValid ? null : "All properties matching patterns must match their respective schemas",
                null
            );
        };
    }
} 