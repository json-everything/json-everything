using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Ast;

public static class JsonSchema
{
    private const string BaseUriDomain = "https://json-everything.net";

    private static string GenerateBaseUri()
    {
        var id = Guid.NewGuid().ToString("N")[^12..];
        return $"{BaseUriDomain}/{id}";
    }

    public static JsonSchemaNode Build(JsonElement schema, JsonSchemaOptions? options = null)
    {
        options ??= JsonSchemaOptions.Global;

        var context = new BuildContext(
            BaseUri: new Uri(GenerateBaseUri()),
            SchemaPath: JsonPointer.Empty,
            SchemaResources: new Dictionary<Uri, JsonElement>(options.SchemaRegistry.Schemas),
            Anchors: new Dictionary<string, Uri>(),
            Visited: new Dictionary<(Uri, JsonPointer), JsonSchemaNode>(),
            ReferenceCache: new Dictionary<Uri, JsonSchemaNode>(),
            InstancePath: JsonPointer.Empty
        );

        return BuildCore(schema, context);
    }

    private static JsonSchemaNode BuildCore(JsonElement schema, BuildContext context)
    {
        // Handle boolean schemas
        if (schema.ValueKind == JsonValueKind.True)
        {
            var trueAst = new JsonSchemaNode(context.BaseUri, context.SchemaPath, JsonSchemaNodeType.True, new(), new());
            return trueAst;
        }
        if (schema.ValueKind == JsonValueKind.False)
        {
            var falseAst = new JsonSchemaNode(context.BaseUri, context.SchemaPath, JsonSchemaNodeType.False, new(), new());
            return falseAst;
        }

        // Verify that the schema is an object
        if (schema.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid schema: expected object, found {schema.ValueKind}");

        // Check for cycles
        var nodeKey = (context.BaseUri, context.SchemaPath);
        if (context.Visited.TryGetValue(nodeKey, out var existingNode))
        {
            return existingNode;
        }

        // Handle $ref
        if (schema.TryGetProperty("$ref", out var refElement))
        {
            var refNode = BuildReferenceNode(refElement.GetString()!, context);
            return refNode;
        }

        // Process schema metadata first
        var newBaseUri = ProcessSchemaMetadata(schema, context);
        var updatedContext = context with { BaseUri = newBaseUri, CurrentSchema = schema };

        // Create placeholder for cycle detection
        var node = new JsonSchemaNode(updatedContext.BaseUri, updatedContext.SchemaPath, JsonSchemaNodeType.Object, new(), new());
        context.Visited[nodeKey] = node;

        // Build children for each keyword
        var properties = new Dictionary<string, object>();
        var children = new List<JsonSchemaNode>();

        foreach (var prop in schema.EnumerateObject())
        {
            if (KeywordHandlers.Handlers.TryGetValue(prop.Name, out var handler))
            {
                handler.BuildAst(prop.Value, updatedContext, properties);

                var childSpecs = handler.GetChildSpecs(prop.Value, updatedContext);
                foreach (var childSpec in childSpecs)
                {
                    var childContext = updatedContext with 
                    { 
                        SchemaPath = updatedContext.SchemaPath.Combine(prop.Name).Combine(childSpec.SchemaPath),
                        InstancePath = childSpec.InstancePath
                    };
                    var child = BuildCore(childSpec.SubSchema, childContext);
                    child = child with 
                    { 
                        InstancePathFromParent = childSpec.InstancePath,
                        SchemaPathFromParent = JsonPointer.Create(prop.Name).Combine(childSpec.SchemaPath)
                    };
                    children.Add(child);
                }
            }
        }

        // Update the node with final data
        var finalNode = node with { Properties = properties, Children = children };
        return finalNode;
    }

    private static JsonSchemaNode BuildReferenceNode(string refValue, BuildContext context)
    {
        // Parse the reference
        var resolvedUri = new Uri(context.BaseUri, refValue);

        // Check cache first
        if (context.ReferenceCache.TryGetValue(resolvedUri, out var cachedTarget))
        {
            return new JsonSchemaNode(
                BaseUri: context.BaseUri,
                SchemaLocation: context.SchemaPath,
                NodeType: JsonSchemaNodeType.Reference,
                Properties: new() { ["$ref"] = refValue },
                Children: new(),
                ResolvedReference: resolvedUri,
                ReferenceTarget: cachedTarget
            );
        }

        // Resolve the reference
        var (targetSchema, targetUri) = ResolveReference(resolvedUri, context);

        // Build the target schema
        var targetContext = context with 
        { 
            BaseUri = targetUri,
            SchemaPath = JsonPointer.Empty
        };

        var targetNode = BuildCore(targetSchema, targetContext);

        // Cache the resolved target
        context.ReferenceCache[resolvedUri] = targetNode;

        // Return reference node that points to resolved target
        return new JsonSchemaNode(
            BaseUri: context.BaseUri,
            SchemaLocation: context.SchemaPath,
            NodeType: JsonSchemaNodeType.Reference,
            Properties: new() { ["$ref"] = refValue },
            Children: new(),
            ResolvedReference: resolvedUri,
            ReferenceTarget: targetNode
        );
    }

    private static (JsonElement Schema, Uri Uri) ResolveReference(Uri referenceUri, BuildContext context)
    {
        var baseUri = new Uri(referenceUri.GetLeftPart(UriPartial.Path));
        var fragment = referenceUri.Fragment;

        // Try to find the schema in resources
        if (context.SchemaResources.TryGetValue(baseUri, out var schemaElement))
        {
            if (string.IsNullOrEmpty(fragment) || fragment == "#")
            {
                return (schemaElement, baseUri);
            }

            // Handle JSON Pointer fragment
            if (fragment.StartsWith("#/"))
            {
                var pointer = JsonPointer.Parse(fragment[1..]);
                if (pointer.TryEvaluate(schemaElement.AsNode(), out var targetNode))
                {
                    var targetElement = JsonSerializer.SerializeToElement(targetNode);
                    return (targetElement, baseUri);
                }
            }
        }

        throw new JsonSchemaException($"Could not resolve reference: {referenceUri}");
    }

    private static Uri ProcessSchemaMetadata(JsonElement schema, BuildContext context)
    {
        var baseUri = context.BaseUri;

        context.SchemaResources.TryAdd(baseUri, schema);

        if (schema.TryGetProperty("$id", out var idElement))
        {
            var idValue = idElement.GetString()!;
            baseUri = new Uri(context.BaseUri, idValue);
            context.SchemaResources.Add(baseUri, schema);
        }

        if (schema.TryGetProperty("$anchor", out var anchorElement))
        {
            var anchor = anchorElement.GetString()!;
            context.Anchors[anchor] = new Uri(baseUri, $"#{anchor}");
        }

        return baseUri;
    }

    public static EvaluationOutput Evaluate(JsonSchemaNode node, JsonElement instance)
    {
        return EvaluateCore(node, instance, JsonPointer.Empty, JsonPointer.Empty);
    }

    private static EvaluationOutput EvaluateCore(JsonSchemaNode node, JsonElement instance, JsonPointer instanceLocation, JsonPointer evaluationPath)
    {
        // Handle boolean schemas
        if (node.NodeType == JsonSchemaNodeType.True)
        {
            var trueResult = new EvaluationOutput(true, instanceLocation, evaluationPath, node.BaseUri, null, null, null);
            return trueResult;
        }
        if (node.NodeType == JsonSchemaNodeType.False)
        {
            var falseResult = new EvaluationOutput(false, instanceLocation, evaluationPath, node.BaseUri, 
                new Dictionary<string, string> { [""] = "Instance validation failed against false schema" }, null, null);
            return falseResult;
        }

        // Handle references - delegate to target
        if (node.NodeType == JsonSchemaNodeType.Reference)
        {
            if (node.ReferenceTarget == null)
                throw new JsonSchemaException($"Unresolved reference: {node.Properties["$ref"]}");

            var refEvaluationPath = evaluationPath.Combine("$ref");
            var refResult = EvaluateCore(node.ReferenceTarget, instance, instanceLocation, refEvaluationPath);
            return refResult;
        }

        // Normal object evaluation
        var isValid = true;
        var errors = new Dictionary<string, string>();
        var annotations = new Dictionary<string, JsonElement>();
        var allResults = new List<EvaluationOutput>();

        // Group children by their parent keyword for dependency evaluation
        var childrenByKeyword = GroupChildrenByKeyword(node);

        // Evaluate ALL keyword handlers that exist in the schema properties
        foreach (var propertyKvp in node.Properties)
        {
            var keyword = propertyKvp.Key;
            
            if (KeywordHandlers.Handlers.TryGetValue(keyword, out var handler))
            {
                // Evaluate child dependencies first (if any)
                var dependencyResults = new List<EvaluationOutput>();
                var hasChildren = childrenByKeyword.TryGetValue(keyword, out var keywordChildren);
                
                if (hasChildren)
                {
                    foreach (var child in keywordChildren!)
                    {
                        var matches = child.InstancePathFromParent.EvaluateWithWildcards(instance);
                        
                        foreach (var (matchValue, matchLocation) in matches)
                        {
                            var childInstanceLocation = instanceLocation.Combine(matchLocation);
                            var childEvaluationPath = evaluationPath.Combine(child.SchemaPathFromParent);

                            var childResult = EvaluateCore(child, matchValue, childInstanceLocation, childEvaluationPath);
                            dependencyResults.Add(childResult);
                            allResults.Add(childResult);
                        }
                    }
                }

                // Now evaluate the keyword constraint
                var result = handler.Evaluate(node, instance, dependencyResults);

                if (result.IsValid)
                {
                    if (result.Annotation.HasValue)
                        annotations[keyword] = result.Annotation.Value;
                }
                else
                {
                    isValid = false;
                    if (result.ErrorMessage != null)
                    {
                        var errorKeyword = result.KeywordOverride ?? keyword;
                        errors[errorKeyword] = result.ErrorMessage;
                    }
                }
            }
        }

        var finalResult = new EvaluationOutput(
            isValid,
            instanceLocation,
            evaluationPath,
            node.BaseUri,
            errors.Count > 0 ? errors : null,
            isValid ? annotations : null,
            allResults.Count > 0 ? allResults : null
        );
        
        return finalResult;
    }

    private static Dictionary<string, List<JsonSchemaNode>> GroupChildrenByKeyword(JsonSchemaNode node)
    {
        var result = new Dictionary<string, List<JsonSchemaNode>>();
        
        foreach (var child in node.Children)
        {
            // Extract keyword from the schema path
            var keyword = ExtractKeywordFromPath(child.SchemaPathFromParent);
            if (!string.IsNullOrEmpty(keyword))
            {
                if (!result.ContainsKey(keyword))
                    result[keyword] = new List<JsonSchemaNode>();
                result[keyword].Add(child);
            }
        }

        return result;
    }

    private static string ExtractKeywordFromPath(JsonPointer path)
    {
        if (path.SegmentCount == 0) return string.Empty;
        return path.GetSegment(0).ToString();
    }
} 