using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Some extensions for <see cref="JsonSchema"/>
/// </summary>
public static partial class JsonSchemaExtensions
{
	/// <summary>
	/// Extends <see cref="JsonSchema.Evaluate"/> to take <see cref="JsonDocument"/>.
	/// </summary>
	/// <param name="jsonSchema">The schema.</param>
	/// <param name="jsonDocument">Instance to be evaluated.</param>
	/// <param name="options">The options to use for this evaluation.</param>
	/// <returns>A <see cref="EvaluationResults"/> that provides the outcome of the evaluation.</returns>
	public static EvaluationResults Evaluate(this JsonSchema jsonSchema, JsonDocument jsonDocument, EvaluationOptions? options = null)
	{
		return jsonSchema.Evaluate(jsonDocument.RootElement, options);
	}

	/// <summary>
	/// Extends <see cref="JsonSchema.Evaluate"/> to take <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="jsonSchema">The schema.</param>
	/// <param name="jsonElement">Instance to be evaluated.</param>
	/// <param name="options">The options to use for this evaluation.</param>
	/// <returns>A <see cref="EvaluationResults"/> that provides the outcome of the evaluation.</returns>
	public static EvaluationResults Evaluate(this JsonSchema jsonSchema, JsonElement jsonElement, EvaluationOptions? options = null)
	{
		return jsonSchema.Evaluate(jsonElement.AsNode(), options);
	}

	/// <summary>
	/// Generates a bundle schema that contains all of the externally referenced schemas
	/// in a single document.
	/// </summary>
	/// <param name="jsonSchema">The root schema.</param>
	/// <param name="options">Options (used for the schema registry)</param>
	/// <returns>A JSON Schema with all referenced schemas.</returns>
	/// <exception cref="JsonSchemaException">Thrown if a reference cannot be resolved.</exception>
	/// <exception cref="NotSupportedException">Thrown if a reference resolves to a non-JSON-Schema document.</exception>
	public static JsonSchema Bundle(this JsonSchema jsonSchema, EvaluationOptions? options = null)
	{
		options = EvaluationOptions.From(options ?? EvaluationOptions.Default);

		JsonSchema.Initialize(jsonSchema, options.SchemaRegistry);

		var schemasToSearch = new List<JsonSchema>();
		var externalSchemas = new Dictionary<string, JsonSchema>();
		var bundledReferences = new List<Uri>();
		var referencesToCheck = new List<Uri> { jsonSchema.BaseUri };

		while (referencesToCheck.Count != 0)
		{
			var nextReference = referencesToCheck[0];
			referencesToCheck.RemoveAt(0);

			var resolved = options.SchemaRegistry.Get(nextReference);
			if (resolved == null)
				throw new JsonSchemaException($"Cannot resolve reference: '{nextReference}'");
			if (resolved is not JsonSchema resolvedSchema)
				throw new NotSupportedException("Bundling is not supported for non-schema root documents");

			JsonSchema.Initialize(resolvedSchema, options.SchemaRegistry);

			if (!bundledReferences.Contains(nextReference))
				externalSchemas.Add(Guid.NewGuid().ToString("N").Substring(0, 10), resolvedSchema);
			schemasToSearch.Add(resolvedSchema);

			while (schemasToSearch.Count != 0)
			{
				var schema = schemasToSearch[0];
				schemasToSearch.RemoveAt(0);

				if (schema.Keywords == null) continue;

				schemasToSearch.AddRange(schema.Keywords.SelectMany(JsonSchema.GetSubschemas));

				if (schema.BaseUri != nextReference && !bundledReferences.Contains(schema.BaseUri))
					bundledReferences.Add(schema.BaseUri);

				var reference = schema.GetRef();
				if (reference != null)
				{
					var newUri = new Uri(schema.BaseUri, reference);
					if (newUri == schema.BaseUri) continue; // same document

					referencesToCheck.Add(newUri);
				}
			}
		}

		return new JsonSchemaBuilder()
			.Id(jsonSchema.BaseUri.OriginalString + "(bundled)")
			.Defs(externalSchemas)
			.Ref(jsonSchema.BaseUri);
	}
}