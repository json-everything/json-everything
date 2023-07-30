using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

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

		_json = JsonSerializer.SerializeToNode(this);
	}

	internal DiscriminatorKeyword(string propertyName, IReadOnlyDictionary<string, string>? mapping, IReadOnlyDictionary<string, JsonNode?>? extensions, JsonNode? json)
	{
		PropertyName = propertyName;
		Mapping = mapping;
		Extensions = extensions;

		_json = json;
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, (e, _) => e.Results.SetAnnotation(Name, _json));
	}
}

internal class DiscriminatorKeywordJsonConverter : JsonConverter<DiscriminatorKeyword>
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

	public override DiscriminatorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode>(ref reader, options);

		var model = node.Deserialize<Model>(options);

		var extensionData = node!.AsObject().Where(x => x.Key.StartsWith("x-"))
			.ToDictionary(x => x.Key, x => x.Value);

		return new DiscriminatorKeyword(model!.PropertyName, model.Mapping, extensionData, node);
	}
	public override void Write(Utf8JsonWriter writer, DiscriminatorKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DiscriminatorKeyword.Name);
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