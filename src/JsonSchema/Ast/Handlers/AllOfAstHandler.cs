using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class AllOfHandler : IKeywordHandler
{
    public string Keyword => "allOf";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["allOf"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (!node.Properties.TryGetValue("allOf", out var allOfObj) || allOfObj is not JsonElement allOfElement)
            return KeywordResult.Skip;

        // All dependencies must be valid for allOf to pass
        var invalidResults = dependencies.Where(d => !d.IsValid).ToList();
        
        if (invalidResults.Count > 0)
        {
            // Find the first error for reporting
            var firstError = invalidResults.First();
            var errorMessage = firstError.Errors?.FirstOrDefault().Value ?? "Schema validation failed";
            return new KeywordResult(false, $"allOf validation failed: {errorMessage}", null);
        }

        // If all schemas are valid, allOf passes
        return new KeywordResult(true, null, null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        if (keywordValue.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException("allOf must be an array");

        // Create a child for each schema in the allOf array
        var index = 0;
        foreach (var subSchema in keywordValue.EnumerateArray())
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