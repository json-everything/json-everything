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
[JsonConverter(typeof(DiscriminatorKeywordJsonConverter))]
public class DiscriminatorKeyword : IJsonSchemaKeyword
{
	internal const string Name = "discriminator";

	private readonly JsonNode? _json;

	/// <summary>
	/// The name of the property in the payload that will hold the discriminator value.
	/// </summary>
	public string PropertyName { get; }
	/// <summary>
	/// An object to hold mappings between payload values and schema names or references.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Mapping { get; }
	/// <summary>
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </summary>
	public IReadOnlyDictionary<string, JsonNode?>? Extensions { get; }

	/// <summary>
	/// Creates a new <see cref="DiscriminatorKeyword"/>.
	/// </summary>
	/// <param name="propertyName">The name of the property in the payload that will hold the discriminator value.</param>
	/// <param name="mapping">An object to hold mappings between payload values and schema names or references.</param>
	/// <param name="extensions">
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </param>
	public DiscriminatorKeyword(string propertyName, IReadOnlyDictionary<string, string>? mapping, IReadOnlyDictionary<string, JsonNode?>? extensions)
	{
		PropertyName = propertyName;
		Mapping = mapping;
		Extensions = extensions;

		_json = JsonSerializer.SerializeToNode(this, OpenApiSerializerContext.Default.DiscriminatorKeyword);
	}

	internal DiscriminatorKeyword(string propertyName, IReadOnlyDictionary<string, string>? mapping, IReadOnlyDictionary<string, JsonNode?>? extensions, JsonNode? json)
	{
		PropertyName = propertyName;
		Mapping = mapping;
		Extensions = extensions;

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
/// JSON converter for <see cref="DiscriminatorKeyword"/>.
/// </summary>
public sealed class DiscriminatorKeywordJsonConverter : JsonConverter<DiscriminatorKeyword>
{
	private class Model
	{
#pragma warning disable CS8618
		// ReSharper disable UnusedAutoPropertyAccessor.Local
		[JsonPropertyName("propertyName")]
		public string PropertyName { get; set; }
#pragma warning restore CS8618
		[JsonPropertyName("mapping")]
		public Dictionary<string, string>? Mapping { get; set; }
		// ReSharper restore UnusedAutoPropertyAccessor.Local
	}

	/// <summary>Reads and converts the JSON to type <see cref="DiscriminatorKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override DiscriminatorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode>(ref reader, options);

		var model = node.Deserialize<Model>(options);

		var extensionData = node!.AsObject().Where(x => x.Key.StartsWith("x-"))
			.ToDictionary(x => x.Key, x => x.Value);

		return new DiscriminatorKeyword(model!.PropertyName, model.Mapping, extensionData, node);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, DiscriminatorKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteString("propertyName", value.PropertyName);
		if (value.Mapping != null)
		{
			writer.WritePropertyName("mapping");
			JsonSerializer.Serialize(writer, value.Mapping, options);
		}

		if (value.Extensions != null)
		{
			foreach (var extension in value.Extensions)
			{
				writer.WritePropertyName(extension.Key);
				JsonSerializer.Serialize(writer, extension.Value, options);
			}
		}
		writer.WriteEndObject();
	}
}