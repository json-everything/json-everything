using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `deprecated`.
	/// </summary>
	[SchemaKeyword(Name)]
	[Vocabulary(Vocabularies.Metadata201909Id)]
	[SchemaDraft(Draft.Draft201909)]
	[JsonConverter(typeof(DeprecatedKeywordJsonConverter))]
	public class DeprecatedKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "deprecated";

		/// <summary>
		/// Whether the schema is deprecated.
		/// </summary>
		public bool Value { get; }

		/// <summary>
		/// Creates a new <see cref="DeprecatedKeyword"/>.
		/// </summary>
		/// <param name="value">Whether the schema is deprecated.</param>
		public DeprecatedKeyword(bool value)
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

	internal class DeprecatedKeywordJsonConverter : JsonConverter<DeprecatedKeyword>
	{
		public override DeprecatedKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.True || reader.TokenType != JsonTokenType.False)
				throw new JsonException("Expected boolean");

			var value = reader.GetBoolean();

			return new DeprecatedKeyword(value);
		}
		public override void Write(Utf8JsonWriter writer, DeprecatedKeyword value, JsonSerializerOptions options)
		{
			writer.WriteBoolean(DeprecatedKeyword.Name, value.Value);
		}
	}
}