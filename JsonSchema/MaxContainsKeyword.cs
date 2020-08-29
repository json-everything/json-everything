using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(MaxContainsKeywordJsonConverter))]
	public class MaxContainsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "maxContains";

		public uint Value { get; }

		public MaxContainsKeyword(uint value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Array)
			{
				context.IsValid = true;
				return;
			}

			var annotation = context.TryGetAnnotation(ContainsKeyword.Name);
			if (annotation == null)
			{
				context.IsValid = true;
				return;
			}

			var containsCount = (int) annotation;
			context.IsValid = Value >= containsCount;
			if (!context.IsValid)
				context.Message = $"Value has more than {Value} items that matched the schema provided by the {ContainsKeyword.Name} keyword";
		}
	}

	internal class MaxContainsKeywordJsonConverter : JsonConverter<MaxContainsKeyword>
	{
		public override MaxContainsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetUInt32();

			return new MaxContainsKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MaxContainsKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MaxContainsKeyword.Name, value.Value);
		}
	}
}