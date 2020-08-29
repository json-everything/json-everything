using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `minProperties`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(MinPropertiesKeywordJsonConverter))]
	public class MinPropertiesKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "minProperties";

		/// <summary>
		/// The minimum expected number of properties.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Creates a new <see cref="MinPropertiesKeyword"/>.
		/// </summary>
		/// <param name="value">The minimum expected number of properties.</param>
		public MinPropertiesKeyword(uint value)
		{
			Value = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Object)
			{
				context.IsValid = true;
				return;
			}

			var number = context.LocalInstance.EnumerateObject().Count();
			context.IsValid = Value <= number;
			if (!context.IsValid)
				context.Message = $"Value has more than {Value} properties";
		}
	}

	internal class MinPropertiesKeywordJsonConverter : JsonConverter<MinPropertiesKeyword>
	{
		public override MinPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetUInt32();

			return new MinPropertiesKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MinPropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MinPropertiesKeyword.Name, value.Value);
		}
	}
}  