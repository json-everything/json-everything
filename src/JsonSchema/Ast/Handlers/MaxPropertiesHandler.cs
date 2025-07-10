using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class MaxPropertiesHandler : IKeywordHandler
{
    public string Keyword => "maxProperties";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Number || !keywordValue.TryGetDecimal(out var maxProperties) || maxProperties < 0 || maxProperties % 1 != 0)
            throw new JsonSchemaException($"Invalid 'maxProperties' keyword value: expected non-negative integer, found {keywordValue.ValueKind}");

        properties["maxProperties"] = (int)maxProperties;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Object)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("maxProperties", out var maxPropertiesObj) || maxPropertiesObj is not int maxProperties)
            return KeywordResult.Skip;

        var propertyCount = instance.EnumerateObject().Count();
        var isValid = propertyCount <= maxProperties;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"Object must have at most {maxProperties} properties, but has {propertyCount}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // MaxProperties keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 