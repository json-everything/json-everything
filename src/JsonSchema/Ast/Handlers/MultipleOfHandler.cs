using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class MultipleOfHandler : IKeywordHandler
{
    public string Keyword => "multipleOf";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Number)
            throw new JsonSchemaException($"Invalid 'multipleOf' keyword value: expected number, found {keywordValue.ValueKind}");

        var multipleOf = keywordValue.GetDecimal();
        if (multipleOf <= 0)
            throw new JsonSchemaException("Invalid 'multipleOf' keyword value: must be greater than 0");

        properties["multipleOf"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Number)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("multipleOf", out var multipleOfObj) || multipleOfObj is not JsonElement multipleOfElement)
            return KeywordResult.Skip;

        var multipleOf = multipleOfElement.GetDecimal();
        var number = instance.GetDecimal();
        var isValid = number % multipleOf == 0;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"Value {number} is not a multiple of {multipleOf}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // MultipleOf keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 