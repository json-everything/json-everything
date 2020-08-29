using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `format`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Format201909Id)]
	[JsonConverter(typeof(FormatKeywordJsonConverter))]
	public class FormatKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "format";

		/// <summary>
		/// The format.
		/// </summary>
		public Format Value { get; }

		/// <summary>
		/// Creates a new <see cref="FormatKeyword"/>.
		/// </summary>
		/// <param name="value">The format.</param>
		public FormatKeyword(Format value)
		{
			Value = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.String)
			{
				context.IsValid = true;
				return;
			}

			context.SetAnnotation(Name, Value.Key);
			context.IsValid = Value.Validate(context.LocalInstance);
		}
	}

	internal class FormatKeywordJsonConverter : JsonConverter<FormatKeyword>
	{
		public override FormatKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();
			var format = Formats.Get(str);

			return new FormatKeyword(format);
		}
		public override void Write(Utf8JsonWriter writer, FormatKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(FormatKeyword.Name, value.Value.Key);
		}
	}
}