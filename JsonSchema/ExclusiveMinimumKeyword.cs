using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `exclusiveMinimum`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(ExclusiveMinimumKeywordJsonConverter))]
	public class ExclusiveMinimumKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "exclusiveMinimum";

		/// <summary>
		/// The minimum value.
		/// </summary>
		public decimal Value { get; }

		/// <summary>
		/// Creates a new <see cref="ExclusiveMinimumKeyword"/>.
		/// </summary>
		/// <param name="value">The minimum value.</param>
		public ExclusiveMinimumKeyword(decimal value)
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
			context.IsValid = Value < number;
			if (!context.IsValid)
				context.Message = $"{number} is not less than {Value}";
		}
	}

	internal class ExclusiveMinimumKeywordJsonConverter : JsonConverter<ExclusiveMinimumKeyword>
	{
		public override ExclusiveMinimumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new ExclusiveMinimumKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, ExclusiveMinimumKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(ExclusiveMinimumKeyword.Name, value.Value);
		}
	}
} 