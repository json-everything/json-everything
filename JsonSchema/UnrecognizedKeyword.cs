using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles unrecognized keywords.
/// </summary>
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[JsonConverter(typeof(UnrecognizedKeywordJsonConverter))]
public class UnrecognizedKeyword : IJsonSchemaKeyword, IEquatable<UnrecognizedKeyword>
{
	/// <summary>
	/// The name or key of the keyword.
	/// </summary>
	public string Name { get; }
	
	/// <summary>
	/// The value of the keyword.
	/// </summary>
	public JsonNode? Value { get; }

	/// <summary>
	/// Creates a new <see cref="UnrecognizedKeyword"/>.
	/// </summary>
	/// <param name="name">The name of the keyword.</param>
	/// <param name="value">The value of the keyword.</param>
	public UnrecognizedKeyword(string name, JsonNode? value)
	{
		Name = name;
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
		context.LocalResult.Pass();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(UnrecognizedKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Name == other.Name && Value.IsEquivalentTo(other.Value);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as UnrecognizedKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			return (Name.GetHashCode() * 397) ^ (Value?.GetHashCode() ?? 0);
		}
	}
}

internal class UnrecognizedKeywordJsonConverter : JsonConverter<UnrecognizedKeyword>
{
	public override UnrecognizedKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException("Unrecognized keywords should be handled manually during JsonSchema deserialization.");
	}

	public override void Write(Utf8JsonWriter writer, UnrecognizedKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(value.Name);
		JsonSerializer.Serialize(writer, value.Value, options);
	}
}