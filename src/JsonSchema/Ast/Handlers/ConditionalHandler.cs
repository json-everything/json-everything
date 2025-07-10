using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class ConditionalHandler : IKeywordHandler
{
    public string Keyword => "if";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        // Store the if condition
        properties["if"] = keywordValue;
        
        // Look for then/else siblings in the current schema
        if (context.CurrentSchema?.TryGetProperty("then", out var thenElement) == true)
            properties["then"] = thenElement;
        if (context.CurrentSchema?.TryGetProperty("else", out var elseElement) == true)
            properties["else"] = elseElement;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        // Must have an if condition
        if (!node.Properties.TryGetValue("if", out var ifObj) || ifObj is not JsonElement)
            return KeywordResult.Skip;

        // Get the if condition result - it should be the first dependency
        var ifResult = dependencies.FirstOrDefault();
        if (ifResult == null)
            return KeywordResult.Skip;

        // Based on if result, check then or else
        if (ifResult.IsValid)
        {
            // If condition passed, check then (if present)
            if (node.Properties.TryGetValue("then", out var thenObj) && thenObj is JsonElement)
            {
                // Find the then result in dependencies (should be second)
                var thenResult = dependencies.Skip(1).FirstOrDefault();
                if (thenResult != null && !thenResult.IsValid)
                {
                    return new KeywordResult(false, "Instance matches the if subschema, but fails to match the then subschema", null);
                }
            }
        }
        else
        {
            // If condition failed, check else (if present)
            if (node.Properties.TryGetValue("else", out var elseObj) && elseObj is JsonElement)
            {
                // Find the else result in dependencies
                var elseResult = dependencies.LastOrDefault();
                if (elseResult != null && !elseResult.IsValid)
                {
                    return new KeywordResult(false, "Instance does not match the if subschema, but fails to match the else subschema", null);
                }
            }
        }

        return new KeywordResult(true, null, null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Always create child spec for the if condition
        yield return new ChildSpec(
            InstancePath: JsonPointer.Empty,
            SchemaPath: JsonPointer.Empty,
            SubSchema: keywordValue
        );
        
        // Create child spec for then if present
        if (context.CurrentSchema?.TryGetProperty("then", out var thenElement) == true)
        {
            yield return new ChildSpec(
                InstancePath: JsonPointer.Empty,
                SchemaPath: JsonPointer.Empty,
                SubSchema: thenElement
            );
        }
        
        // Create child spec for else if present  
        if (context.CurrentSchema?.TryGetProperty("else", out var elseElement) == true)
        {
            yield return new ChildSpec(
                InstancePath: JsonPointer.Empty,
                SchemaPath: JsonPointer.Empty,
                SubSchema: elseElement
            );
        }
    }
} 