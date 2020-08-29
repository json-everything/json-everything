using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `description`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Metadata201909Id)]
	[JsonConverter(typeof(DescriptionKeywordJsonConverter))]
	public class DescriptionKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "description";

		/// <summary>
		/// The description.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Creates a new <see cref="DescriptionKeyword"/>.
		/// </summary>
		/// <param name="value">The description.</param>
		public DescriptionKeyword(string value)
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

	internal class DescriptionKeywordJsonConverter : JsonConverter<DescriptionKeyword>
	{
		public override DescriptionKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new DescriptionKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, DescriptionKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(DescriptionKeyword.Name, value.Value);
		}
	}
}