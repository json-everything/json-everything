using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class PropertyNamesHandler : IKeywordHandler
{
    // Use the same approach as the main implementation - a special key for property names
    private static readonly JsonPointer PropertyNameKey = JsonPointer.Create(Guid.NewGuid().ToString("N"));

    public string Keyword => "propertyNames";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["propertyNames"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Object)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("propertyNames", out var propertyNamesObj) || propertyNamesObj is not JsonElement propertyNamesElement)
            return KeywordResult.Skip;

        // Check that all property names passed validation
        var allValid = dependencies.All(dep => dep.IsValid);
        return new KeywordResult(
            allValid,
            allValid ? null : "All property names must match the subschema",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Create a child spec that validates property names using a special key wildcard
        yield return new ChildSpec(
            InstancePath: PropertyNameKey, // Use special key wildcard for property names
            SchemaPath: JsonPointer.Empty,
            SubSchema: keywordValue
        );
    }
} 