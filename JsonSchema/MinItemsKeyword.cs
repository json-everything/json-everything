using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `minItems`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(MinItemsKeywordJsonConverter))]
	public class MinItemsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "minItems";

		/// <summary>
		/// The expected minimum number of items.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Create a new <see cref="MinItemsKeyword"/>.
		/// </summary>
		/// <param name="value">The expected minimum number of items.</param>
		public MinItemsKeyword(uint value)
		{
			Value = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Array)
			{
				context.IsValid = true;
				return;
			}

			var number = context.LocalInstance.GetArrayLength();
			context.IsValid = Value <= number;
			if (!context.IsValid)
				context.Message = $"Value has less than {Value} items";
		}
	}

	internal class MinItemsKeywordJsonConverter : JsonConverter<MinItemsKeyword>
	{
		public override MinItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetUInt32();

			return new MinItemsKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MinItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MinItemsKeyword.Name, value.Value);
		}
	}
}