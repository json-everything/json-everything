using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class ConditionalHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        var schema = context.LocalSchema;

        if (!schema.TryGetProperty("if", out var ifElement))
            return;

        var ifNode = JsonSchema.BuildCore(ifElement, context with
        {
            SchemaPath = context.SchemaPath.Combine("if"),
            AdditionalSchemaPathFromParent = JsonPointer.Empty,
            InstancePathFromParent = JsonPointer.Empty,
            FilterDependencyLocations = null
        });

        JsonSchemaNode? thenNode = null;
        if (schema.TryGetProperty("then", out var thenElement))
        {
            thenNode = JsonSchema.BuildCore(thenElement, context with
            {
                SchemaPath = context.SchemaPath.Combine("then"),
                AdditionalSchemaPathFromParent = JsonPointer.Empty,
                InstancePathFromParent = JsonPointer.Empty,
                FilterDependencyLocations = null
            });
        }

        JsonSchemaNode? elseNode = null;
        if (schema.TryGetProperty("else", out var elseElement))
        {
            elseNode = JsonSchema.BuildCore(elseElement, context with
            {
                SchemaPath = context.SchemaPath.Combine("else"),
                AdditionalSchemaPathFromParent = JsonPointer.Empty,
                InstancePathFromParent = JsonPointer.Empty,
                FilterDependencyLocations = null
            });
        }

        var allNodes = new List<JsonSchemaNode> { ifNode };
        if (thenNode != null) allNodes.Add(thenNode);
        if (elseNode != null) allNodes.Add(elseNode);
        dependencies["if"] = [..allNodes];

        constraints["if"] = (_, deps) =>
        {
            var ifResult = deps[0];
            var isValid = true;
            string? errorMessage = null;
            string? keywordOverride = null;

            if (ifResult.IsValid)
            {
                if (thenNode != null && deps.Count > 1)
                {
                    var thenResult = deps[1];
                    if (!thenResult.IsValid)
                    {
                        isValid = false;
                        errorMessage = "Instance matches the if subschema, but fails to match the then subschema";
                        keywordOverride = "then";
                    }
                }
            }
            else
            {
                if (elseNode != null)
                {
                    var elseResult = deps[^1];
                    if (!elseResult.IsValid)
                    {
                        isValid = false;
                        errorMessage = "Instance does not match the if subschema, but fails to match the else subschema";
                        keywordOverride = "else";
                    }
                }
            }

            return new KeywordResult(isValid, errorMessage, null, keywordOverride);
        };
    }
} 