using System;
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
[JsonConverter(typeof(ExampleKeywordJsonConverter))]
public class ExampleKeyword : IJsonSchemaKeyword, IEquatable<ExampleKeyword>
{
	internal const string Name = "example";

	/// <summary>
	/// The example value.
	/// </summary>
	public JsonNode? Value { get; }

	/// <summary>
	/// Creates a new <see cref="ExampleKeyword"/>.
	/// </summary>
	/// <param name="value">The example value.</param>
	public ExampleKeyword(JsonNode? value)
	{
		Value = value;
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		context.LocalResult.SetAnnotation(Name, Value);
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(ExampleKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Value.IsEquivalentTo(other.Value);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as ExampleKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Value?.GetEquivalenceHashCode() ?? 0;
	}
}

internal class ExampleKeywordJsonConverter : JsonConverter<ExampleKeyword>
{
	public override ExampleKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode>(ref reader, options);

		return new ExampleKeyword(node);
	}
	public override void Write(Utf8JsonWriter writer, ExampleKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(ExampleKeyword.Name);
		JsonSerializer.Serialize(writer, value.Value, options);
	}
}