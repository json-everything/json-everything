using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class MinItemsHandler : IKeywordHandler
{
    public string Keyword => "minItems";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Number || !keywordValue.TryGetDecimal(out var minItems) || minItems < 0 || minItems % 1 != 0)
            throw new JsonSchemaException($"Invalid 'minItems' keyword value: expected non-negative integer, found {keywordValue.ValueKind}");

        properties["minItems"] = (int)minItems;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Array)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("minItems", out var minItemsObj) || minItemsObj is not int minItems)
            return KeywordResult.Skip;

        var array = instance.EnumerateArray().ToArray();
        var isValid = array.Length >= minItems;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"Array length {array.Length} is less than minimum length {minItems}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // MinItems keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 