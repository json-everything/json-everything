using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class AnyOfHandler : IKeywordHandler
{
    public string Keyword => "anyOf";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["anyOf"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (!node.Properties.TryGetValue("anyOf", out var anyOfObj) || anyOfObj is not JsonElement anyOfElement)
            return KeywordResult.Skip;

        // At least one dependency must be valid for anyOf to pass
        var validResults = dependencies.Where(d => d.IsValid).ToList();
        
        if (validResults.Count > 0)
        {
            return new KeywordResult(true, null, null);
        }

        // If no schemas are valid, anyOf fails
        return new KeywordResult(false, "Instance must match at least one of the subschemas", null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        if (keywordValue.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException("anyOf must be an array");

        var array = keywordValue.EnumerateArray().ToArray();
        if (array.Length == 0)
            throw new JsonSchemaException("anyOf array must contain at least one subschema");

        // Create a child for each schema in the anyOf array
        var index = 0;
        foreach (var subSchema in array)
        {
            yield return new ChildSpec(
                InstancePath: JsonPointer.Empty, // Same instance for all schemas
                SchemaPath: JsonPointer.Create(index.ToString()),
                SubSchema: subSchema
            );
            index++;
        }
    }
} 