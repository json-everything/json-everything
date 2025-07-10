using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class UniqueItemsHandler : IKeywordHandler
{
    public string Keyword => "uniqueItems";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.True && keywordValue.ValueKind != JsonValueKind.False)
            throw new JsonSchemaException($"Invalid 'uniqueItems' keyword value: expected boolean, found {keywordValue.ValueKind}");

        properties["uniqueItems"] = keywordValue.GetBoolean();
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Array)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("uniqueItems", out var uniqueItemsObj) || uniqueItemsObj is not bool uniqueItems)
            return KeywordResult.Skip;

        // If uniqueItems is false, no validation is needed
        if (!uniqueItems)
            return KeywordResult.Skip;

        var array = instance.EnumerateArray().ToArray();
        if (array.Length <= 1)
            return new KeywordResult(true, null, null);

        // Compare each element with all subsequent elements
        for (int i = 0; i < array.Length - 1; i++)
        {
            var current = array[i];
            var rest = array.Skip(i + 1);
            if (rest.Any(x => x.DeepEquals(current)))
            {
                var duplicateIndex = Array.FindIndex(array, i + 1, x => x.DeepEquals(current));
                return new KeywordResult(
                    false,
                    $"Array contains duplicate items at indices {i} and {duplicateIndex}",
                    null
                );
            }
        }

        return new KeywordResult(true, null, null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // UniqueItems keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 