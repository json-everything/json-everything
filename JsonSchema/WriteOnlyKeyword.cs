using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `writeOnly`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Metadata201909Id)]
	[JsonConverter(typeof(WriteOnlyKeywordJsonConverter))]
	public class WriteOnlyKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "writeOnly";

		/// <summary>
		/// Whether the instance is read-only.
		/// </summary>
		public bool Value { get; }

		/// <summary>
		/// Creates a new <see cref="WriteOnlyKeyword"/>.
		/// </summary>
		/// <param name="value">Whether the instance is read-only.</param>
		public WriteOnlyKeyword(bool value)
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

	internal class WriteOnlyKeywordJsonConverter : JsonConverter<WriteOnlyKeyword>
	{
		public override WriteOnlyKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.True || reader.TokenType != JsonTokenType.False)
				throw new JsonException("Expected boolean");

			var str = reader.GetBoolean();

			return new WriteOnlyKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, WriteOnlyKeyword value, JsonSerializerOptions options)
		{
			writer.WriteBoolean(WriteOnlyKeyword.Name, value.Value);
		}
	}
}