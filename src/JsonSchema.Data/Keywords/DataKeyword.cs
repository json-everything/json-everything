using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Path;
using Json.Pointer;

namespace Json.Schema.Data.Keywords;

/// <summary>
/// Represents the `data` keyword.
/// </summary>
public class DataKeyword : IKeywordHandler
{
	internal static readonly string[] CoreKeywords =
	[
		"$anchor",
		"$comment",
		"$defs",
		"$dynamicAnchor",
		"$dynamicRef",
		"$id",
		"$recursiveAnchor",
		"$recursiveRef",
		"$ref",
		"$schema",
		"$vocabulary"
	];

	/// <summary>
	/// Gets or sets the singleton instance of the DataKeyword class.
	/// </summary>
	public static DataKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "data";

	private DataKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Object)
			throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}.");

		var spec = new DataSpec();
		foreach (var property in value.EnumerateObject())
		{
			if (CoreKeywords.Contains(property.Name))
				throw new JsonSchemaException($"'{Name}' property values cannot be core keywords, found {property.Value.ValueKind}.");

			if (property.Value.ValueKind is not JsonValueKind.String)
				throw new JsonSchemaException($"'{Name}' property values must be strings, found {property.Value.ValueKind}.");

			var reference = property.Value.GetString()!;
			if (!reference.StartsWith("#") && JsonPointer.TryParse(reference, out var pointer))
				spec.References[property.Name] = new JsonPointerIdentifier(pointer);
			else if (RelativeJsonPointer.TryParse(reference, out var relativePointer))
				spec.References[property.Name] = new RelativeJsonPointerIdentifier(relativePointer);
			else if (JsonPath.TryParse(reference, out var path))
				spec.References[property.Name] = new JsonPathIdentifier(path);
			else if (Uri.TryCreate(reference, UriKind.RelativeOrAbsolute, out var uri))
				spec.References[property.Name] = new UriIdentifier(uri);
			else
				throw new JsonSchemaException($"Could not determine reference format for '{property.Name}'.");
		}

		return spec;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var spec = (DataSpec)keyword.Value!;
		var buildContext = BuildContext.From(keyword);
		var unresolved = new List<string>();
		foreach (var reference in spec.References)
		{
			JsonElement instance;
			if (reference.Value is UriIdentifier uriIdentifier)
			{
				var uri = buildContext.BaseUri.Resolve(uriIdentifier.Target);
				instance = buildContext.Options.GetDataRegistry().Get(uri) ??
				           (buildContext.Options.SchemaRegistry.Get(uri) as JsonSchema)?.Root.Source ??
				           default;
			}
			else
			{
				instance = context.InstanceRoot;
			}

			if (!reference.Value.TryResolve(instance, out var data))
			{
				unresolved.Add(reference.Value.ToString()!);
				continue;
			}

			spec.Data[reference.Key] = data;
		}

		if (unresolved.Count != 0)
			throw new DataRefResolutionException(unresolved);

		buildContext.LocalSchema = JsonSerializer.SerializeToElement(spec.Data, JsonSchemaDataSerializerContext.Default.DictionaryStringJsonElement);
		var subschema = JsonSchema.BuildNode(buildContext);

		var newContext = context with
		{
			EvaluationPath = context.EvaluationPath.Combine(Name)
		};

		var result = subschema.Evaluate(newContext);
		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = result.IsValid,
			Details = [result]
		};
	}
}