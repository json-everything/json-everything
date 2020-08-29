using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `maxItems`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(MaxItemsKeywordJsonConverter))]
	public class MaxItemsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "maxItems";

		/// <summary>
		/// The expected maximum number of items.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Creates a new <see cref="MaxItemsKeyword"/>.
		/// </summary>
		/// <param name="value">The expected maximum number of items.</param>
		public MaxItemsKeyword(uint value)
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
			context.IsValid = Value >= number;
			if (!context.IsValid)
				context.Message = $"Value has more than {Value} items";
		}
	}

	internal class MaxItemsKeywordJsonConverter : JsonConverter<MaxItemsKeyword>
	{
		public override MaxItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetUInt32();

			return new MaxItemsKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MaxItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MaxItemsKeyword.Name, value.Value);
		}
	}
}