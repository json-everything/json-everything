using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema.OpenApi;

/// <summary>
/// Handles `example`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[Vocabulary(Vocabularies.OpenApiId)]
[JsonConverter(typeof(ExternalDocsKeywordJsonConverter))]
public class ExternalDocsKeyword : IJsonSchemaKeyword
{
	internal const string Name = "externalDocs";

	private readonly JsonNode? _json;

	/// <summary>
	/// The URL for the target documentation. This MUST be in the form of a URL.
	/// </summary>
	public Uri Url { get; }
	/// <summary>
	/// A description of the target documentation. CommonMark syntax MAY be used for rich text representation.
	/// </summary>
	public string? Description { get; }
	/// <summary>
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </summary>
	public IReadOnlyDictionary<string, JsonNode?>? Extensions { get; }

	/// <summary>
	/// Creates a new <see cref="ExternalDocsKeyword"/>.
	/// </summary>
	/// <param name="url">The URL for the target documentation. This MUST be in the form of a URL.</param>
	/// <param name="description">A description of the target documentation. CommonMark syntax MAY be used for rich text representation.</param>
	/// <param name="extensions">
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </param>
	public ExternalDocsKeyword(Uri url, string? description, IReadOnlyDictionary<string, JsonNode?>? extensions)
	{
		Url = url;
		Description = description;
		Extensions = extensions;

		_json = JsonSerializer.SerializeToNode(this);
	}
	internal ExternalDocsKeyword(Uri url, string? description, IReadOnlyDictionary<string, JsonNode?>? extensions, JsonNode? json)
		: this(url, description, extensions)
	{
		_json = json;
	}

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	/// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	/// Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, (e, _) => e.Results.SetAnnotation(Name, _json));
	}
}

/// <summary>
/// JSON converter for <see cref="ExternalDocsKeyword"/>.
/// </summary>
public sealed class ExternalDocsKeywordJsonConverter : Json.More.AotCompatibleJsonConverter<ExternalDocsKeyword>
{
	private class Model
	{
#pragma warning disable CS8618
		// ReSharper disable UnusedAutoPropertyAccessor.Local
		[JsonPropertyName("url")]
		public Uri Url { get; set; }
		[JsonPropertyName("description")]
		public string? Description { get; set; }
#pragma warning restore CS8618
		// ReSharper restore UnusedAutoPropertyAccessor.Local
	}

	/// <summary>Reads and converts the JSON to type <see cref="ExternalDocsKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override ExternalDocsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode>(ref reader, options);

		var model = node.Deserialize<Model>(options);

		var extensionData = node!.AsObject().Where(x => x.Key.StartsWith("x-"))
			.ToDictionary(x => x.Key, x => x.Value);

		return new ExternalDocsKeyword(model!.Url, model.Description, extensionData, node);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, ExternalDocsKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteString("propertyName", value.Url.OriginalString);
		if (value.Description != null)
			writer.WriteString("description", value.Description);

		if (value.Extensions != null)
		{
			foreach (var extension in value.Extensions)
			{
				writer.WritePropertyName(extension.Key);
				JsonSerializer.Serialize(writer, extension.Value, options);
			}
		}
	}
}