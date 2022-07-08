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
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.OpenApiId)]
[JsonConverter(typeof(DiscriminatorKeywordJsonConverter))]
public class DiscriminatorKeyword : IJsonSchemaKeyword, IEquatable<DiscriminatorKeyword>
{
	internal const string Name = "discriminator";

	/// <summary>
	/// Gets the property name.
	/// </summary>
	public string PropertyName { get; }
	/// <summary>
	/// Gets the mapping information.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Mapping { get; }
	/// <summary>
	/// Gets the extension data.
	/// </summary>
	public IReadOnlyDictionary<string, JsonNode?>? Extensions { get; }

	/// <summary>
	/// Creates a new <see cref="DiscriminatorKeyword"/>.
	/// </summary>
	/// <param name="propertyName"></param>
	/// <param name="mapping"></param>
	/// <param name="extensions"></param>
	public DiscriminatorKeyword(string propertyName, IReadOnlyDictionary<string, string>? mapping, IReadOnlyDictionary<string, JsonNode?>? extensions)
	{
		PropertyName = propertyName;
		Mapping = mapping;
		Extensions = extensions;
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);

		// todo ??? 

		context.LocalResult.Pass();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(DiscriminatorKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (PropertyName != other.PropertyName) return false;
		if ((Mapping == null) != (other.Mapping == null)) return false;
		if ((Extensions == null) != (other.Extensions == null)) return false;

		bool keysMatch;
		bool valuesMatch;
		if (Mapping != null)
		{
			keysMatch = Mapping.Count == other.Mapping!.Count &&
			            !Mapping.Keys.Except(other.Mapping.Keys).Any();
			if (!keysMatch) return false;

			valuesMatch = Mapping.Join(other.Mapping,
					x => x.Key,
					y => y.Key,
					(x, y) => x.Value == y.Value)
				.All(x => x);

			if (!valuesMatch) return false;
		}

		if (Extensions != null)
		{
			keysMatch = Extensions.Count == other.Extensions!.Count &&
			            !Extensions.Keys.Except(other.Extensions.Keys).Any();
			if (!keysMatch) return false;

			valuesMatch = Extensions.Join(other.Extensions,
					x => x.Key,
					y => y.Key,
					(x, y) => x.Value.IsEquivalentTo(y.Value))
				.All(x => x);

			if (!valuesMatch) return false;
		}

		return true;
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as DiscriminatorKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = PropertyName.GetHashCode();
			hashCode = (hashCode * 397) ^ (Mapping != null ? Mapping.GetStringDictionaryHashCode() : 0);
			hashCode = (hashCode * 397) ^ (Extensions != null ? Extensions.GetStringDictionaryHashCode() : 0);
			return hashCode;
		}
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

		return new DiscriminatorKeyword(model!.PropertyName, model.Mapping, extensionData);
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
	}
}