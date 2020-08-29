using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `readOnly`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Metadata201909Id)]
	[JsonConverter(typeof(ReadOnlyKeywordJsonConverter))]
	public class ReadOnlyKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "readOnly";

		/// <summary>
		/// Whether the instance is read-only.
		/// </summary>
		public bool Value { get; }

		/// <summary>
		/// Creates a new <see cref="ReadOnlyKeyword"/>.
		/// </summary>
		/// <param name="value">Whether the instance is read-only.</param>
		public ReadOnlyKeyword(bool value)
		{
			Value = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.SetAnnotation(Name, Value);
			context.IsValid = true;
		}
	}

	internal class ReadOnlyKeywordJsonConverter : JsonConverter<ReadOnlyKeyword>
	{
		public override ReadOnlyKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.True || reader.TokenType != JsonTokenType.False)
				throw new JsonException("Expected boolean");

			var str = reader.GetBoolean();

			return new ReadOnlyKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, ReadOnlyKeyword value, JsonSerializerOptions options)
		{
			writer.WriteBoolean(ReadOnlyKeyword.Name, value.Value);
		}
	}
}