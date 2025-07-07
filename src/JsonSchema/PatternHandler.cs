using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema;

public sealed class PatternHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        if (value.ValueKind != JsonValueKind.String)
            throw new JsonSchemaException($"Invalid 'pattern' keyword value: expected string, found {value.ValueKind}");

        var pattern = value.GetString()!;
        try
        {
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            constraints["pattern"] = (instance, _) =>
            {
                if (instance.ValueKind != JsonValueKind.String)
                    return KeywordResult.Skip;

                var str = instance.GetString()!;
                var isValid = regex.IsMatch(str);
                return new KeywordResult(
                    isValid,
                    isValid ? null : $"String '{str}' does not match pattern '{pattern}'",
                    null
                );
            };
        }
        catch (ArgumentException ex)
        {
            throw new JsonSchemaException($"Invalid 'pattern' keyword value: invalid regular expression: {ex.Message}");
        }
    }
} 