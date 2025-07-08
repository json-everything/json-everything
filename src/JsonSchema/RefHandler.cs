using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public sealed class RefHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        // Validate that the keyword value is a string
        if (value.ValueKind != JsonValueKind.String)
            throw new JsonSchemaException($"Invalid '$ref' keyword value: expected string, found {value.ValueKind}");

        var reference = value.GetString()!;

        // Use Uri.TryCreate to validate the IRI reference
        if (!Uri.TryCreate(reference, UriKind.RelativeOrAbsolute, out _))
            throw new JsonSchemaException($"Invalid '$ref' keyword value: '{reference}' is not a valid IRI reference");

        var resolved = ResolveReference(reference, context);

        var node = JsonSchema.BuildCore(resolved.Schema, resolved.Context);
        dependencies["$ref"] = [node];

        constraints["$ref"] = (_, deps) =>
        {
            // The instance must match the referenced schema
            var isValid = deps[0].IsValid;
            return new KeywordResult(
                isValid,
                isValid ? null : "Instance must match the referenced schema",
                null
            );
        };
    }

    private static (JsonElement Schema, JsonSchema.BuildContext Context, JsonPointer SchemaPath) ResolveReference(
        string reference,
        JsonSchema.BuildContext context)
    {
        var uri = new Uri(context.BaseUri, reference);
        var (baseUri, fragment) = SplitUri(uri);

        if (!context.SchemaResources.TryGetValue(baseUri, out var rootSchema))
            throw new InvalidOperationException($"Schema resource not found: {baseUri}");

        var target = rootSchema;
        var schemaPath = JsonPointer.Empty;
        if (!string.IsNullOrEmpty(fragment))
        {
            var pointer = context.Anchors.TryGetValue(fragment, out var anchoredUri)
                ? anchoredUri.Fragment
                : fragment;
            schemaPath = JsonPointer.Parse(pointer);
            target = schemaPath.Evaluate(rootSchema) ??
                     throw new JsonSchemaRefException(uri.ToString());
        }

        return (target, context with
        {
            BaseUri = baseUri,
            SchemaResources = context.SchemaResources,
            Anchors = context.Anchors,
            SchemaPath = schemaPath,
            AdditionalSchemaPathFromParent = JsonPointerHelpers.GetCachedPointer("$ref"),
            InstancePathFromParent = JsonPointer.Empty,
            FilterDependencyLocations = null
        }, schemaPath);
    }

    private static (Uri BaseUri, string? Fragment) SplitUri(Uri uri)
    {
        var builder = new UriBuilder(uri) { Fragment = string.Empty };
        return (builder.Uri, uri.Fragment.TrimStart('#'));
    }
}

public class JsonSchemaRefException(string uri) : Exception($"Could not find resource with URI {uri}");