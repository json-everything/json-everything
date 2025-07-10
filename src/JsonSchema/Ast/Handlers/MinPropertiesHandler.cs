using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class MinPropertiesHandler : IKeywordHandler
{
    public string Keyword => "minProperties";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Number || !keywordValue.TryGetDecimal(out var minProperties) || minProperties < 0 || minProperties % 1 != 0)
            throw new JsonSchemaException($"Invalid 'minProperties' keyword value: expected non-negative integer, found {keywordValue.ValueKind}");

        properties["minProperties"] = (int)minProperties;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Object)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("minProperties", out var minPropertiesObj) || minPropertiesObj is not int minProperties)
            return KeywordResult.Skip;

        var propertyCount = instance.EnumerateObject().Count();
        var isValid = propertyCount >= minProperties;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"Object must have at least {minProperties} properties, but has {propertyCount}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // MinProperties keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 