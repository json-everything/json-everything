using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class TypeHandler : IKeywordHandler
{
    public string Keyword => "type";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["type"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (!node.Properties.TryGetValue("type", out var typeObj) || typeObj is not JsonElement typeElement)
            return KeywordResult.Skip;

        var expectedTypes = new List<string>();
        
        if (typeElement.ValueKind == JsonValueKind.String)
        {
            expectedTypes.Add(typeElement.GetString()!);
        }
        else if (typeElement.ValueKind == JsonValueKind.Array)
        {
            expectedTypes.AddRange(typeElement.EnumerateArray().Select(x => x.GetString()!));
        }

        var actualType = GetJsonType(instance);
        
        if (!expectedTypes.Contains(actualType))
        {
            return new KeywordResult(false, $"Expected type {string.Join(" or ", expectedTypes)}, but got {actualType}", null);
        }

        return new KeywordResult(true, null, null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Type keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }

    private static string GetJsonType(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => element.TryGetInt32(out _) ? "integer" : "number",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Array => "array",
            JsonValueKind.Object => "object",
            JsonValueKind.Null => "null",
            _ => "unknown"
        };
    }
} 