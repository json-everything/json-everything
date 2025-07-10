using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class MaxItemsHandler : IKeywordHandler
{
    public string Keyword => "maxItems";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Number || !keywordValue.TryGetDecimal(out var maxItems) || maxItems < 0 || maxItems % 1 != 0)
            throw new JsonSchemaException($"Invalid 'maxItems' keyword value: expected non-negative integer, found {keywordValue.ValueKind}");

        properties["maxItems"] = (int)maxItems;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Array)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("maxItems", out var maxItemsObj) || maxItemsObj is not int maxItems)
            return KeywordResult.Skip;

        var array = instance.EnumerateArray().ToArray();
        var isValid = array.Length <= maxItems;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"Array length {array.Length} is greater than maximum length {maxItems}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // MaxItems keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 