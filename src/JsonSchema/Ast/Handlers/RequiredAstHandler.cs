using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class RequiredHandler : IKeywordHandler
{
    public string Keyword => "required";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["required"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Object)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("required", out var requiredObj) || requiredObj is not JsonElement requiredElement)
            return KeywordResult.Skip;

        var requiredProperties = requiredElement.EnumerateArray().Select(x => x.GetString()!).ToList();
        var missingProperties = new List<string>();

        foreach (var requiredProperty in requiredProperties)
        {
            if (!instance.TryGetProperty(requiredProperty, out _))
            {
                missingProperties.Add(requiredProperty);
            }
        }

        if (missingProperties.Count > 0)
        {
            return new KeywordResult(false, $"Missing required properties: {string.Join(", ", missingProperties)}", null);
        }

        return new KeywordResult(true, null, null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Required keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 