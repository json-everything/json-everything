using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class MinimumHandler : IKeywordHandler
{
    public string Keyword => "minimum";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["minimum"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Number)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("minimum", out var minimumObj) || minimumObj is not JsonElement minimumElement)
            return KeywordResult.Skip;

        var minimumValue = minimumElement.GetDecimal();
        var instanceValue = instance.GetDecimal();

        if (instanceValue < minimumValue)
        {
            return new KeywordResult(false, $"Value {instanceValue} is less than minimum {minimumValue}", null);
        }

        return new KeywordResult(true, null, null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Minimum keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 