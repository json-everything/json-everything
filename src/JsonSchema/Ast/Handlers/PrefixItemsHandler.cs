using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public class PrefixItemsHandler : IKeywordHandler
{
    public string Keyword => "prefixItems";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException($"Invalid 'prefixItems' keyword value: expected array, found {keywordValue.ValueKind}");

        properties["prefixItems"] = keywordValue;
        properties["prefixItems.count"] = keywordValue.GetArrayLength();
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Array)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("prefixItems.count", out var countObj) || countObj is not int prefixCount)
            return KeywordResult.Skip;

        var array = instance.EnumerateArray().ToArray();
        if (array.Length == 0)
            return new KeywordResult(true, null, JsonSerializer.SerializeToElement(0));

        // Only validate up to the number of prefix schemas
        var itemsToValidate = Math.Min(array.Length, prefixCount);
        var allValid = dependencies.Take(itemsToValidate).All(dep => dep.IsValid);
        
        return new KeywordResult(
            allValid,
            allValid ? null : "One or more prefix items do not match their corresponding schemas",
            JsonSerializer.SerializeToElement(prefixCount)
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        if (keywordValue.ValueKind != JsonValueKind.Array)
            throw new JsonSchemaException($"Invalid 'prefixItems' keyword value: expected array, found {keywordValue.ValueKind}");

        var schemas = keywordValue.EnumerateArray().ToArray();
        if (schemas.Length == 0)
            yield break; // No items to validate

        // Create a child spec for each prefix schema
        for (int i = 0; i < schemas.Length; i++)
        {
            yield return new ChildSpec(
                InstancePath: JsonPointer.Create(i.ToString()),
                SchemaPath: JsonPointer.Create(i.ToString()),
                SubSchema: schemas[i]
            );
        }
    }
} 