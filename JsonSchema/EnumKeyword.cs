using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `enum`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[JsonConverter(typeof(EnumKeywordJsonConverter))]
public class EnumKeyword : IJsonSchemaKeyword, IEquatable<EnumKeyword>
{
	internal const string Name = "enum";

	private readonly HashSet<JsonNode?> _values;

	/// <summary>
	/// The collection of enum values.
	/// </summary>
	/// <remarks>
	/// Enum values aren't necessarily strings; they can be of any JSON value.
	/// </remarks>
	public IReadOnlyCollection<JsonNode?> Values => _values;

	/// <summary>
	/// Creates a new <see cref="EnumKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of enum values.</param>
	public EnumKeyword(params JsonNode?[] values)
	{
		_values = new HashSet<JsonNode?>(values ?? throw new ArgumentNullException(nameof(values)),
			JsonNodeEqualityComparer.Instance);

		if (_values.Count != values.Length)
			throw new ArgumentException("`enum` requires unique values");
	}

	/// <summary>
	/// Creates a new <see cref="EnumKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of enum values.</param>
	public EnumKeyword(IEnumerable<JsonNode?> values)
	{
		_values = new HashSet<JsonNode?>(values ?? throw new ArgumentNullException(nameof(values)),
			JsonNodeEqualityComparer.Instance);

		if (_values.Count != values.Count())
			throw new ArgumentException("`enum` requires unique values");
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		if (Values.Contains(context.LocalInstance, JsonNodeEqualityComparer.Instance))
			context.LocalResult.Pass();
		else
			context.LocalResult.Fail(Name, ErrorMessages.Enum, ("received", context.LocalInstance), ("values", Values));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(EnumKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		// Don't need ContentsEqual here because that method considers counts.
		// We know that with a hash set, all counts are 1.
		return Values.Count == other.Values.Count &&
			   Values.All(x => other.Values.Contains(x, JsonNodeEqualityComparer.Instance));
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as EnumKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Values.GetUnorderedCollectionHashCode(element => element?.GetEquivalenceHashCode() ?? 0);
	}
}

internal class EnumKeywordJsonConverter : JsonConverter<EnumKeyword>
{
	public override EnumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var array = JsonSerializer.Deserialize<JsonArray>(ref reader);
		if (array is null)
			throw new JsonException("Expected an array, but received null");

		return new EnumKeyword((IEnumerable<JsonNode>)array!);
	}
	public override void Write(Utf8JsonWriter writer, EnumKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(EnumKeyword.Name);
		writer.WriteStartArray();
		foreach (var node in value.Values)
		{
			JsonSerializer.Serialize(writer, node, options);
		}
		writer.WriteEndArray();
	}
}

public static partial class ErrorMessages
{
	private static string? _enum;

	/// <summary>
	/// Gets or sets the error message for <see cref="EnumKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[values]] - the available values in the schema
	///
	/// The default messages are static and do not use these tokens as enum values
	/// may be any JSON type and could be quite large.  They are provided to support
	/// custom messages.
	/// </remarks>
	public static string Enum
	{
		get => _enum ?? Get();
		set => _enum = value;
	}
}