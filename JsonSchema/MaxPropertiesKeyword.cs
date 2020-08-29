using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `maxProperties`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(MaxPropertiesKeywordJsonConverter))]
	public class MaxPropertiesKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "maxProperties";

		/// <summary>
		/// The maximum expected number of properties.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Creates a new <see cref="MaxPropertiesKeyword"/>.
		/// </summary>
		/// <param name="value">The maximum expected number of properties.</param>
		public MaxPropertiesKeyword(uint value)
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
			context.IsValid = Value >= number;
			if (!context.IsValid)
				context.Message = $"Value has more than {Value} properties";
		}
	}

	internal class MaxPropertiesKeywordJsonConverter : JsonConverter<MaxPropertiesKeyword>
	{
		public override MaxPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetUInt32();

			return new MaxPropertiesKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MaxPropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MaxPropertiesKeyword.Name, value.Value);
		}
	}
}  