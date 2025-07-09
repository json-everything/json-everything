using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;
using Json.Schema;

namespace Json.Schema.Ast;

public class ItemsHandler : IKeywordHandler
{
    public string Keyword => "items";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        properties["items"] = keywordValue;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Array)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("items", out var itemsObj) || itemsObj is not JsonElement itemsElement)
            return KeywordResult.Skip;

        // All dependencies should be valid (item validations)
        foreach (var dep in dependencies)
        {
            if (!dep.IsValid)
                return new KeywordResult(false, $"Item validation failed", null);
        }

        // Create annotation with evaluated indices
        var evaluatedIndices = new List<int>();
        foreach (var dep in dependencies)
        {
            var index = ExtractArrayIndex(dep.InstanceLocation);
            if (index >= 0)
                evaluatedIndices.Add(index);
        }

        if (evaluatedIndices.Count > 0)
        {
            var annotation = JsonSerializer.SerializeToElement(evaluatedIndices);
            return new KeywordResult(true, null, annotation);
        }

        return new KeywordResult(true, null, null);
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        // Items keyword creates a child for each array item (using wildcard)
        yield return new ChildSpec(
            InstancePath: JsonPointerHelpers.Wildcard,
            SchemaPath: JsonPointer.Empty,
            SubSchema: keywordValue
        );
    }

    private static int ExtractArrayIndex(JsonPointer instanceLocation)
    {
        if (instanceLocation.SegmentCount == 0) return -1;
        var lastSegment = instanceLocation.GetSegment(instanceLocation.SegmentCount - 1).ToString();
        return int.TryParse(lastSegment, out var index) ? index : -1;
    }
} 