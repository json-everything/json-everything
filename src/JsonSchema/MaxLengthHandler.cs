using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace Json.Schema;

public sealed class MaxLengthHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var maxLength) || maxLength < 0 || maxLength % 1 != 0)
            throw new JsonSchemaException($"Invalid 'maxLength' keyword value: expected non-negative integer, found {value.ValueKind}");

        var maxLengthInt = (int)maxLength;
        constraints["maxLength"] = (instance, _) =>
        {
            if (instance.ValueKind != JsonValueKind.String)
                return KeywordResult.Skip;

            var str = instance.GetString()!;
            var length = new StringInfo(str).LengthInTextElements;
            var isValid = length <= maxLengthInt;
            return new KeywordResult(
                isValid,
                isValid ? null : $"String grapheme length {length} is greater than maximum length {maxLengthInt}",
                null
            );
        };
    }
} 