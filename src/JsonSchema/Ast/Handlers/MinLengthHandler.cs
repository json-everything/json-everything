using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class MinLengthHandler : IKeywordHandler
{
    public string Keyword => "minLength";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Number || !keywordValue.TryGetDecimal(out var minLength) || minLength < 0 || minLength % 1 != 0)
            throw new JsonSchemaException($"Invalid 'minLength' keyword value: expected non-negative integer, found {keywordValue.ValueKind}");

        properties["minLength"] = (int)minLength;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.String)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("minLength", out var minLengthObj) || minLengthObj is not int minLength)
            return KeywordResult.Skip;

        var str = instance.GetString()!;
        var length = new StringInfo(str).LengthInTextElements;
        var isValid = length >= minLength;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"String grapheme length {length} is less than minimum length {minLength}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // MinLength keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 