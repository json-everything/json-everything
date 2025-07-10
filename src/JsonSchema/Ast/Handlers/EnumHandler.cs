using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class EnumHandler : IKeywordHandler
{
    public string Keyword => "enum";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException($"Invalid 'enum' keyword value: expected array, found {keywordValue.ValueKind}");

        properties["enum"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (!node.Properties.TryGetValue("enum", out var enumObj) || enumObj is not JsonElement enumElement)
            return KeywordResult.Skip;

        foreach (var enumValue in enumElement.EnumerateArray())
        {
            if (instance.DeepEquals(enumValue))
            {
                return new KeywordResult(true, null, null);
            }
        }

        return new KeywordResult(
            false,
            "Value must match one of the specified enum values",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Enum keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 