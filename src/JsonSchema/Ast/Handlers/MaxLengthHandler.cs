using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class MaxLengthHandler : IKeywordHandler
{
    public string Keyword => "maxLength";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Number || !keywordValue.TryGetDecimal(out var maxLength) || maxLength < 0 || maxLength % 1 != 0)
            throw new JsonSchemaException($"Invalid 'maxLength' keyword value: expected non-negative integer, found {keywordValue.ValueKind}");

        properties["maxLength"] = (int)maxLength;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.String)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("maxLength", out var maxLengthObj) || maxLengthObj is not int maxLength)
            return KeywordResult.Skip;

        var str = instance.GetString()!;
        var length = new StringInfo(str).LengthInTextElements;
        var isValid = length <= maxLength;
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"String grapheme length {length} is greater than maximum length {maxLength}",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // MaxLength keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 