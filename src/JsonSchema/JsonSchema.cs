using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public static class JsonSchema
{
    private static JsonSchemaNode CreateTrue(BuildContext context) => new(
        baseIri: context.BaseUri,
        schemaLocation: context.SchemaPath,
        constraints: new() { [""] = (_, _) => KeywordResult.Skip },
        dependencies: new(),
        schemaPathFromParent: context.AdditionalSchemaPathFromParent,
        instancePathFromParent: context.InstancePathFromParent,
        context.FilterDependencyLocations
    );

    private static JsonSchemaNode CreateFalse(BuildContext context) => new(
        baseIri: context.BaseUri,
        schemaLocation: context.SchemaPath,
        constraints: new() { [""] = (_, _) => new KeywordResult(false, "Instance validation failed against false schema", null) },
        dependencies: new(),
        schemaPathFromParent: context.AdditionalSchemaPathFromParent,
        instancePathFromParent: context.InstancePathFromParent,
        context.FilterDependencyLocations
    );

    public sealed record BuildContext(
        Uri BaseUri,
        JsonPointer CurrentInstanceLocation,
        JsonPointer SchemaPath,
        Dictionary<Uri, JsonElement> SchemaResources,
        Dictionary<string, Uri> Anchors,
        Dictionary<(Uri SchemaUri, JsonPointer Location), JsonSchemaNode> Visited,
        JsonPointer AdditionalSchemaPathFromParent,
        JsonPointer InstancePathFromParent,
        JsonElement LocalSchema,
        Func<JsonPointer, JsonElement, JsonElement, bool>? FilterDependencyLocations = null
    );

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
            CurrentInstanceLocation: JsonPointer.Empty,
            SchemaPath: JsonPointer.Empty,
            SchemaResources: new Dictionary<Uri, JsonElement>(options.SchemaRegistry.Schemas),
            Anchors: new Dictionary<string, Uri>(),
            Visited: new Dictionary<(Uri, JsonPointer), JsonSchemaNode>(),
            AdditionalSchemaPathFromParent: JsonPointer.Empty,
            InstancePathFromParent: JsonPointer.Empty,
            LocalSchema: schema
        );

        var rootNode = BuildCore(schema, context);
        return rootNode;
    }

    public static JsonSchemaNode BuildCore(JsonElement schema, BuildContext context)
    {
        // Handle boolean schemas
        if (schema.ValueKind == JsonValueKind.True)
            return CreateTrue(context);
        if (schema.ValueKind == JsonValueKind.False)
            return CreateFalse(context);

        // Verify that the schema is an object
        if (schema.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid schema: expected object, found {schema.ValueKind}");

        var nodeKey = (context.BaseUri, context.SchemaPath);
        if (context.Visited.TryGetValue(nodeKey, out var existingNode))
            return existingNode;

        // Process schema metadata first
        var newBaseUri = ProcessSchemaMetadata(schema, context);
        var updatedContext = context with { BaseUri = newBaseUri, LocalSchema = schema };

        // Create initial node with empty collections
        var node = new JsonSchemaNode(
            baseIri: updatedContext.BaseUri,
            schemaLocation: updatedContext.SchemaPath,
            constraints: new(),
            dependencies: new(),
            schemaPathFromParent: updatedContext.AdditionalSchemaPathFromParent,
            instancePathFromParent: updatedContext.InstancePathFromParent,
            filterDependencyLocations: context.FilterDependencyLocations
        );

        // Add the node to visited before processing dependencies
        context.Visited[nodeKey] = node;

        foreach (var property in schema.EnumerateObject())
        {
            var keyword = property.Name;
            var value = property.Value;

            if (KeywordHandlers.Handlers.TryGetValue(keyword, out var handler))
            {
                handler.Build(
                    value,
                    updatedContext,
                    node.Constraints,
                    node.Dependencies
                );
            }
        }

        return node;
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
		return Evaluate(node, instance, JsonPointer.Empty, JsonPointer.Empty);
	}

	private static EvaluationOutput Evaluate(JsonSchemaNode node, JsonElement instance, JsonPointer currentInstanceLocation, JsonPointer currentEvaluationPath)
	{
		// Track which dependencies have been evaluated
		var evaluatedDependencies = new Dictionary<string, List<EvaluationOutput>>();
		var allResults = new List<EvaluationOutput>();
		var isValid = true;
		var errors = new Dictionary<string, string>();
		var annotations = new Dictionary<string, JsonElement>();

		// Evaluate all dependencies first
		foreach (var kvp in node.Dependencies)
		{
			var keyword = kvp.Key;
			var dependencies = kvp.Value;
			foreach (var dependency in dependencies)
			{
				EvaluateDependency(keyword, dependency, instance, currentInstanceLocation, currentEvaluationPath, allResults, evaluatedDependencies);
			}
		}

		// Now evaluate constraints as soon as their dependencies are available
		foreach (var kvp in node.Constraints)
		{
			var keyword = kvp.Key;
			var constraint = kvp.Value;
			var keywordDeps = evaluatedDependencies.GetValueOrDefault(keyword, []);
			var result = constraint(instance, keywordDeps);

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

		return new EvaluationOutput(
			isValid,
			currentInstanceLocation,
			currentEvaluationPath,
			node.SchemaIri,
			errors.Count > 0 ? errors : null,
			isValid ? annotations : null,
			allResults.Count > 0 ? allResults : null
		);
	}

	private static void EvaluateDependency(string keyword, JsonSchemaNode dependency, JsonElement instance, JsonPointer currentInstanceLocation, JsonPointer currentEvaluationPath, List<EvaluationOutput> allResults, Dictionary<string, List<EvaluationOutput>> evaluatedDependencies)
	{
		// Get all matching instance values for the dependency's path
		var matches = dependency.InstancePathFromParent.EvaluateWithWildcards(instance);
		if (matches.Length == 0) return;

		var dependencyResults = new List<EvaluationOutput>();
		foreach (var (matchValue, matchLocation) in matches)
		{
			// Apply the filter if it exists
			if (dependency.FilterDependencyLocations != null &&
				!dependency.FilterDependencyLocations(matchLocation, matchValue, instance))
				continue;

			// Build the full instance location for the dependency
			var dependencyInstanceLocation = currentInstanceLocation.Combine(matchLocation);
			// Build the evaluation path including the keyword and additional schema path
			var dependencyEvaluationPath =
				dependency.AdditionalSchemaPathFromParent.SegmentCount == 0
					? currentEvaluationPath.Combine(keyword)
					: currentEvaluationPath.Combine(keyword).Combine(dependency.AdditionalSchemaPathFromParent);

			// Evaluate the dependency immediately
			var result = Evaluate(dependency, matchValue, dependencyInstanceLocation, dependencyEvaluationPath);
			dependencyResults.Add(result);
			allResults.Add(result);
		}

		if (dependencyResults.Count > 0)
			evaluatedDependencies[keyword] = dependencyResults;
	}
}

public class SchemaRegistry
{
	public static SchemaRegistry Global { get; } = new();

	internal Dictionary<Uri, JsonElement> Schemas { get; } = new();

	public SchemaRegistry(){}

	public SchemaRegistry(SchemaRegistry other)
	{
		Schemas = new Dictionary<Uri, JsonElement>(other.Schemas);
	}

	public void Register(JsonElement schema, Uri? baseUri = null)
	{
		if (baseUri is null)
		{
			if (schema.ValueKind != JsonValueKind.Object)
				throw new ArgumentException("Only object schemas with $id can be registered without supplying a base IRI");

			var idElement = schema.EnumerateObject().FirstOrDefault(x => x.Name == "$id").Value;
			if (idElement.ValueKind != JsonValueKind.String)
				throw new ArgumentException("Only object schemas with $id can be registered without supplying a base IRI");

			var id = idElement.GetString();
			if (!Uri.TryCreate(id, UriKind.Absolute, out baseUri))
				throw new ArgumentException("$id must be a valid absolute IRI");
		}

		Schemas.Add(baseUri, schema);
	}
}

public class JsonSchemaOptions
{
	public static JsonSchemaOptions Global { get; } = new();

	public JsonSchemaOptions() { }

	public JsonSchemaOptions(JsonSchemaOptions other)
	{
		SchemaRegistry = new SchemaRegistry(other.SchemaRegistry);
	}

	public SchemaRegistry SchemaRegistry { get; set; } = SchemaRegistry.Global;
}