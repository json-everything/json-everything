using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(MinContainsKeywordJsonConverter))]
	public class MinContainsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "minContains";

		public int Value { get; }

		public MinContainsKeyword(int value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			if (Value == 0)
			{
				context.IsValid = true;
				var containsContext = context.SiblingContexts.FirstOrDefault(c => c.SchemaLocation.Segments.LastOrDefault().Value == ContainsKeyword.Name);
				if (containsContext != null)
					containsContext.IsValid = true;
				return;
			}

			if (context.Instance.ValueKind != JsonValueKind.Array)
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
			context.IsValid = Value <= containsCount;
			if (!context.IsValid)
				context.Message = $"Value has less than {Value} items that matched the schema provided by the {ContainsKeyword.Name} keyword";
		}
	}

	public class MinContainsKeywordJsonConverter : JsonConverter<MinContainsKeyword>
	{
		public override MinContainsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetInt32();

			return new MinContainsKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MinContainsKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MinContainsKeyword.Name, value.Value);
		}
	}
}