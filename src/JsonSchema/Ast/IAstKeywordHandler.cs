using System.Collections.Generic;
using System.Text.Json;

namespace Json.Schema.Ast;

public interface IKeywordHandler
{
    string Keyword { get; }
    
    /// <summary>
    /// Build phase - convert JsonElement to AST representation
    /// </summary>
    void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties);
    
    /// <summary>
    /// Evaluation phase - validate using AST
    /// </summary>
    KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies);
    
    /// <summary>
    /// Declare what child nodes this keyword creates
    /// </summary>
    IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context);
} 