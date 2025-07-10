using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class ConstHandler : IKeywordHandler
{
    public string Keyword => "const";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["const"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (!node.Properties.TryGetValue("const", out var constObj) || constObj is not JsonElement constElement)
            return KeywordResult.Skip;

        if (instance.DeepEquals(constElement))
        {
            return new KeywordResult(true, null, null);
        }

        return new KeywordResult(
            false,
            "Value must exactly match the specified constant value",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Const keyword doesn't create child nodes
        return Enumerable.Empty<ChildSpec>();
    }
} 