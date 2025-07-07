using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace Json.Schema;

public sealed class MinLengthHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var minLength) || minLength < 0 || minLength % 1 != 0)
            throw new JsonSchemaException($"Invalid 'minLength' keyword value: expected non-negative integer, found {value.ValueKind}");

        var minLengthInt = (int)minLength;
        constraints["minLength"] = (instance, _) =>
        {
            if (instance.ValueKind != JsonValueKind.String)
                return KeywordResult.Skip;

            var str = instance.GetString()!;
            var length = new StringInfo(str).LengthInTextElements;
            var isValid = length >= minLengthInt;
            return new KeywordResult(
                isValid,
                isValid ? null : $"String grapheme length {length} is less than minimum length {minLengthInt}",
                null
            );
        };
    }
} 