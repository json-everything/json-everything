using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class ExclusiveMinimumHandler : IKeywordHandler
{
    public string Keyword => "exclusiveMinimum";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Number)
            throw new JsonSchemaException($"Invalid 'exclusiveMinimum' keyword value: expected number, found {keywordValue.ValueKind}");

        properties["exclusiveMinimum"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Number)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("exclusiveMinimum", out var exclusiveMinObj) || exclusiveMinObj is not JsonElement exclusiveMinElement)
            return KeywordResult.Skip;

        var exclusiveMinimum = exclusiveMinElement.GetDecimal();
        var number = instance.GetDecimal();
        var isValid = number > exclusiveMinimum;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"Value {number} is not greater than exclusive minimum {exclusiveMinimum}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // ExclusiveMinimum keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 