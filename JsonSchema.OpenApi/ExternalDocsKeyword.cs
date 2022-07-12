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
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.OpenApiId)]
[JsonConverter(typeof(ExternalDocsKeywordJsonConverter))]
public class ExternalDocsKeyword : IJsonSchemaKeyword, IEquatable<ExternalDocsKeyword>
{
	internal const string Name = "externalDocs";

	private JsonNode? _json;

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
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);

		// todo ??? 

		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(ExternalDocsKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (Url != other.Url) return false;
		if (Description == null) return other.Description == null;
		if ((Extensions == null) != (other.Extensions == null)) return false;

		if (Extensions != null)
		{
			var keysMatch = Extensions.Count == other.Extensions!.Count &&
			                !Extensions.Keys.Except(other.Extensions.Keys).Any();
			if (!keysMatch) return false;

			var mapsMatch = Extensions.Join(other.Extensions,
					x => x.Key,
					y => y.Key,
					(x, y) => x.Value == y.Value)
				.All(x => x);

			return mapsMatch;
		}

		return true;
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as ExternalDocsKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = Url.GetHashCode();
			hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (Extensions != null ? Extensions.GetStringDictionaryHashCode() : 0);
			return hashCode;
		}
	}
}

internal class ExternalDocsKeywordJsonConverter : JsonConverter<ExternalDocsKeyword>
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

	public override ExternalDocsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode>(ref reader, options);

		var model = node.Deserialize<Model>(options);

		var extensionData = node!.AsObject().Where(x => x.Key.StartsWith("x-"))
			.ToDictionary(x => x.Key, x => x.Value);

		return new ExternalDocsKeyword(model!.Url, model.Description, extensionData, node);
	}
	public override void Write(Utf8JsonWriter writer, ExternalDocsKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DiscriminatorKeyword.Name);
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