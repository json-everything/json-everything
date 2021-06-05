using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema.UniqueKeys
{
	/// <summary>
	/// Represents the `data` keyword.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaPriority(int.MinValue)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.UniqueKeysId)]
	[JsonConverter(typeof(UniqueKeysKeywordJsonConverter))]
	public class UniqueKeysKeyword : IJsonSchemaKeyword, IEquatable<UniqueKeysKeyword>
	{
		internal const string Name = "uniqueKeys";

		/// <summary>
		/// The collection of keywords and references.
		/// </summary>
		public IEnumerable<JsonPointer> Keys { get; }

		/// <summary>
		/// Creates an instance of the <see cref="UniqueKeysKeyword"/> class.
		/// </summary>
		/// <param name="references">The collection of keywords and references.</param>
		public UniqueKeysKeyword(IEnumerable<JsonPointer> references)
		{
			Keys = references;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(UniqueKeysKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return Keys.SequenceEqual(other.Keys);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as UniqueKeysKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Keys.GetHashCode();
		}
	}

	internal class UniqueKeysKeywordJsonConverter : JsonConverter<UniqueKeysKeyword>
	{
		public override UniqueKeysKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var references = JsonSerializer.Deserialize<List<JsonPointer>>(ref reader, options);
			return new UniqueKeysKeyword(references);
		}

		public override void Write(Utf8JsonWriter writer, UniqueKeysKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(UniqueKeysKeyword.Name);
			JsonSerializer.Serialize(writer, value.Keys, options);
		}
	}
}
