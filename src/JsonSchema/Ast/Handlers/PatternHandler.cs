using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Ast;

public class PatternHandler : IKeywordHandler
{
    public string Keyword => "pattern";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.String)
            throw new JsonSchemaException($"Invalid 'pattern' keyword value: expected string, found {keywordValue.ValueKind}");

        var pattern = keywordValue.GetString()!;
        try
        {
            var regex = new Regex(pattern, RegexOptions.ECMAScript);
            properties["pattern"] = regex;
            properties["pattern.string"] = pattern;
        }
        catch (ArgumentException ex)
        {
            throw new JsonSchemaException($"Invalid 'pattern' keyword value: invalid regular expression: {ex.Message}");
        }
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.String)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("pattern", out var regexObj) || regexObj is not Regex regex ||
            !node.Properties.TryGetValue("pattern.string", out var patternObj) || patternObj is not string pattern)
            return KeywordResult.Skip;

        var str = instance.GetString()!;
        var isValid = regex.IsMatch(str);
        
        return new KeywordResult(
            isValid,
            isValid ? null : $"String '{str}' does not match pattern '{pattern}'",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Pattern keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 