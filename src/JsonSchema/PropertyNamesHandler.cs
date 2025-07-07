using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class PropertyNamesHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        var newContext = context with
        {
            CurrentInstanceLocation = context.CurrentInstanceLocation.Combine(JsonPointerHelpers.Key),
            InstancePathFromParent = JsonPointerHelpers.Key,
            SchemaPath = context.SchemaPath.Combine("propertyNames"),
            AdditionalSchemaPathFromParent = JsonPointer.Empty,
            FilterDependencyLocations = null
        };

        var node = JsonSchema.BuildCore(value, newContext);
        dependencies["propertyNames"] = [node];

        constraints["propertyNames"] = (instance, deps) =>
        {
            if (instance.ValueKind != JsonValueKind.Object)
                return KeywordResult.Skip;

            // Check that all property names passed validation
            var allValid = deps.All(dep => dep.IsValid);
            return new KeywordResult(
                allValid,
                allValid ? null : "All property names must match the subschema",
                null
            );
        };
    }
} 