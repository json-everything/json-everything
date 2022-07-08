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

	/// <summary>
	/// Gets the namespace.
	/// </summary>
	public Uri? Namespace { get; }
	/// <summary>
	/// Gets the name.
	/// </summary>
	public string? Name { get; }
	/// <summary>
	/// Gets the prefix.
	/// </summary>
	public string? Prefix { get; }
	/// <summary>
	/// Gets whether there is an attribute.
	/// </summary>
	public bool? Attribute { get; }
	/// <summary>
	/// Gets whether the item is wrapped.
	/// </summary>
	public bool? Wrapped { get; }
	/// <summary>
	/// Gets the extension data.
	/// </summary>
	public IReadOnlyDictionary<string, JsonNode?>? Extensions { get; }

	/// <summary>
	/// Creates a new <see cref="ExternalDocsKeyword"/>.
	/// </summary>
	/// <param name="namespace"></param>
	/// <param name="name"></param>
	/// <param name="prefix"></param>
	/// <param name="wrapped"></param>
	/// <param name="extensions"></param>
	/// <param name="attribute"></param>
	public XmlKeyword(Uri? @namespace, string? name, string? prefix, bool? attribute, bool? wrapped, IReadOnlyDictionary<string, JsonNode?>? extensions)
	{
		Namespace = @namespace;
		Name = name;
		Prefix = prefix;
		Attribute = attribute;
		Wrapped = wrapped;
		Extensions = extensions;
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(_Name);

		// todo ??? 

		context.LocalResult.Pass();
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

		return new XmlKeyword(model!.Namespace, model.Name, model.Prefix, model.Attribute, model.Wrapped, extensionData);
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