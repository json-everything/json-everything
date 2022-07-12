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
[SchemaKeyword(_Name)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.OpenApiId)]
[JsonConverter(typeof(XmlKeywordJsonConverter))]
public class XmlKeyword : IJsonSchemaKeyword, IEquatable<XmlKeyword>
{
	// ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles
	internal const string _Name = "xml";
#pragma warning restore IDE1006 // Naming Styles

	private readonly JsonNode? _json;

	/// <summary>
	/// The URI of the namespace definition. This MUST be in the form of an absolute URI.
	/// </summary>
	public Uri? Namespace { get; }
	/// <summary>
	/// Replaces the name of the element/attribute used for the described schema property.
	/// When defined within `items`, it will affect the name of the individual XML elements within the list.
	/// When defined alongside `type` being `array` (outside the `items`), it will affect the wrapping element and
	/// only if `wrapped` is `true`. If `wrapped` is `false`, it will be ignored.
	/// </summary>
	public string? Name { get; }
	/// <summary>
	/// The prefix to be used for the name.
	/// </summary>
	public string? Prefix { get; }
	/// <summary>
	/// Declares whether the property definition translates to an attribute instead of an
	/// element. Default value is `false`.
	/// </summary>
	public bool? Attribute { get; }
	/// <summary>
	/// MAY be used only for an array definition. Signifies whether the array is wrapped (for example, `<books><book/><book/></books>`)
	/// or unwrapped (`<book/><book/>`). Default value is `false`. The definition takes effect only when defined alongside `type`
	/// being `array` (outside the `items`).
	/// </summary>
	public bool? Wrapped { get; }
	/// <summary>
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </summary>
	public IReadOnlyDictionary<string, JsonNode?>? Extensions { get; }

	/// <summary>
	/// Creates a new <see cref="ExternalDocsKeyword"/>.
	/// </summary>
	/// <param name="namespace">The URI of the namespace definition. This MUST be in the form of an absolute URI.</param>
	/// <param name="name">Replaces the name of the element/attribute used for the described schema property.
	/// When defined within `items`, it will affect the name of the individual XML elements within the list.
	/// When defined alongside `type` being `array` (outside the `items`), it will affect the wrapping element and
	/// only if `wrapped` is `true`. If `wrapped` is `false`, it will be ignored.</param>
	/// <param name="prefix">The prefix to be used for the name.</param>
	/// <param name="attribute">Declares whether the property definition translates to an attribute instead of an
	/// element. Default value is `false`.</param>
	/// <param name="wrapped">
	/// MAY be used only for an array definition. Signifies whether the array is wrapped (for example, `<books><book/><book/></books>`)
	/// or unwrapped (`<book/><book/>`). Default value is `false`. The definition takes effect only when defined alongside `type`
	/// being `array` (outside the `items`).</param>
	/// <param name="extensions">
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </param>
	public XmlKeyword(Uri? @namespace, string? name, string? prefix, bool? attribute, bool? wrapped, IReadOnlyDictionary<string, JsonNode?>? extensions)
	{
		Namespace = @namespace;
		Name = name;
		Prefix = prefix;
		Attribute = attribute;
		Wrapped = wrapped;
		Extensions = extensions;

		_json = JsonSerializer.SerializeToNode(this);
	}

	internal XmlKeyword(Uri? @namespace, string? name, string? prefix, bool? attribute, bool? wrapped, IReadOnlyDictionary<string, JsonNode?>? extensions, JsonNode? json)
		: this(@namespace, name, prefix, attribute, wrapped, extensions)
	{
		_json = json;
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(_Name);
		context.LocalResult.SetAnnotation(_Name, _json);
		context.ExitKeyword(_Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(XmlKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (Namespace != other.Namespace) return false;
		if (Name != other.Name) return false;
		if (Prefix != other.Prefix) return false;
		if (Attribute != other.Attribute) return false;
		if (Wrapped != other.Wrapped) return false;

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
		return Equals(obj as XmlKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (Prefix != null ? Prefix.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ Attribute.GetHashCode();
			hashCode = (hashCode * 397) ^ Wrapped.GetHashCode();
			hashCode = (hashCode * 397) ^ (Extensions != null ? Extensions.GetStringDictionaryHashCode() : 0);
			return hashCode;
		}
	}
}

internal class XmlKeywordJsonConverter : JsonConverter<XmlKeyword>
{
	// ReSharper disable UnusedAutoPropertyAccessor.Local
	private class Model
	{
		[JsonPropertyName("namespace")]
		public Uri? Namespace { get; set; }
		[JsonPropertyName("name")]
		public string? Name { get; set; }
		[JsonPropertyName("prefix")]
		public string? Prefix { get; set; }
		[JsonPropertyName("attribute")]
		public bool? Attribute { get; set; }
		[JsonPropertyName("wrapped")]
		public bool? Wrapped { get; set; }
	}
	// ReSharper restore UnusedAutoPropertyAccessor.Local

	public override XmlKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode>(ref reader, options);

		var model = node.Deserialize<Model>(options);

		var extensionData = node!.AsObject().Where(x => x.Key.StartsWith("x-"))
			.ToDictionary(x => x.Key, x => x.Value);

		return new XmlKeyword(model!.Namespace, model.Name, model.Prefix, model.Attribute, model.Wrapped, extensionData, node);
	}
	public override void Write(Utf8JsonWriter writer, XmlKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DiscriminatorKeyword.Name);
		writer.WriteStartObject();
		if (value.Namespace != null)
			writer.WriteString("namespace", value.Namespace.OriginalString);
		if (value.Name != null)
			writer.WriteString("name", value.Name);
		if (value.Prefix != null)
			writer.WriteString("prefix", value.Prefix);
		if (value.Attribute.HasValue)
			writer.WriteBoolean("attribute", value.Attribute.Value);
		if (value.Wrapped.HasValue)
			writer.WriteBoolean("wrapped", value.Wrapped.Value);

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