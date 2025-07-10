using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class MaximumHandler : IKeywordHandler
{
    public string Keyword => "maximum";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Number)
            throw new JsonSchemaException($"Invalid 'maximum' keyword value: expected number, found {keywordValue.ValueKind}");

        properties["maximum"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Number)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("maximum", out var maximumObj) || maximumObj is not JsonElement maximumElement)
            return KeywordResult.Skip;

        var maximum = maximumElement.GetDecimal();
        var number = instance.GetDecimal();
        var isValid = number <= maximum;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"Value {number} is greater than maximum {maximum}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Maximum keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 