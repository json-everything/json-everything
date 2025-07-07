using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class NotHandler : IKeywordHandler
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
            SchemaPath = context.SchemaPath.Combine("not"),
            AdditionalSchemaPathFromParent = JsonPointer.Empty,
            InstancePathFromParent = JsonPointer.Empty,
            FilterDependencyLocations = null
        };
        var subschema = JsonSchema.BuildCore(value, newContext);

        dependencies["not"] = [subschema];

        constraints["not"] = (_, deps) =>
        {
            var isValid = !deps[0].IsValid;
            return new KeywordResult(
                isValid,
                isValid ? null : "Instance must not match the subschema",
                null
            );
        };
    }
} 