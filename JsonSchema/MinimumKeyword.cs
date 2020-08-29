using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `minimum`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(MinimumKeywordJsonConverter))]
	public class MinimumKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "minimum";

		/// <summary>
		/// The minimum expected value.
		/// </summary>
		public decimal Value { get; }

		/// <summary>
		/// Creates a new <see cref="MinimumKeyword"/>.
		/// </summary>
		/// <param name="value">The minimum expected value.</param>
		public MinimumKeyword(decimal value)
		{
			Value = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Number)
			{
				context.IsValid = true;
				return;
			}

			var number = context.LocalInstance.GetDecimal();
			context.IsValid = Value <= number;
			if (!context.IsValid)
				context.Message = $"{number} is less than or equal to {Value}";
		}
	}

	internal class MinimumKeywordJsonConverter : JsonConverter<MinimumKeyword>
	{
		public override MinimumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new MinimumKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MinimumKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MinimumKeyword.Name, value.Value);
		}
	}
}