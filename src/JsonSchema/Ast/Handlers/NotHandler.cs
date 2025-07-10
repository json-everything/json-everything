using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class NotHandler : IKeywordHandler
{
    public string Keyword => "not";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["not"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (!node.Properties.TryGetValue("not", out var notObj) || notObj is not JsonElement notElement)
            return KeywordResult.Skip;

        // Not keyword inverts the result - if the subschema is valid, not fails
        var isValid = dependencies.Count == 0 || !dependencies[0].IsValid;
        
        return new KeywordResult(
            isValid,
            isValid ? null : "Instance must not match the subschema",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Create a child spec for the not subschema
        yield return new ChildSpec(
            InstancePath: JsonPointer.Empty, // Apply to the same instance
            SchemaPath: JsonPointer.Empty,
            SubSchema: keywordValue
        );
    }
} 