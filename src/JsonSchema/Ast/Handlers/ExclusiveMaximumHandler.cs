using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class ExclusiveMaximumHandler : IKeywordHandler
{
    public string Keyword => "exclusiveMaximum";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Number)
            throw new JsonSchemaException($"Invalid 'exclusiveMaximum' keyword value: expected number, found {keywordValue.ValueKind}");

        properties["exclusiveMaximum"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Number)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("exclusiveMaximum", out var exclusiveMaxObj) || exclusiveMaxObj is not JsonElement exclusiveMaxElement)
            return KeywordResult.Skip;

        var exclusiveMaximum = exclusiveMaxElement.GetDecimal();
        var number = instance.GetDecimal();
        var isValid = number < exclusiveMaximum;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"Value {number} is not less than exclusive maximum {exclusiveMaximum}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // ExclusiveMaximum keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 