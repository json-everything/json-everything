using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Ast;

public class RefHandler : IKeywordHandler
{
    public string Keyword => "$ref";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["$ref"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        // $ref is handled at the AST level, not in individual keyword evaluation
        // This should never be called in normal operation
        return new KeywordResult(true, null, null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // $ref is handled at the AST level, not as child specs
        // This should never be called in normal operation
        return Enumerable.Empty<ChildSpec>();
    }
} 