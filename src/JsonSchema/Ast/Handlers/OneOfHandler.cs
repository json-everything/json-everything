using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class OneOfHandler : IKeywordHandler
{
    public string Keyword => "oneOf";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["oneOf"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (!node.Properties.TryGetValue("oneOf", out var oneOfObj) || oneOfObj is not JsonElement oneOfElement)
            return KeywordResult.Skip;

        // Count how many schemas are valid
        var validCount = dependencies.Count(dep => dep.IsValid);
        var isValid = validCount == 1;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"Instance must match exactly one subschema, but matched {validCount}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        if (keywordValue.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException($"Invalid 'oneOf' keyword value: expected array, found {keywordValue.ValueKind}");

        var array = keywordValue.EnumerateArray().ToArray();
        if (array.Length == 0)
            throw new JsonSchemaException("Invalid 'oneOf' keyword value: array must contain at least one subschema");

        // Create a child for each schema in the oneOf array
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