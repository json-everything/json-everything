using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cysharp.Text;
using Json.More;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(UniqueItemsKeywordJsonConverter))]
	public class UniqueItemsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "uniqueItems";

		public bool Value { get; }

		public UniqueItemsKeyword(bool value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Array)
			{
				context.IsValid = true;
				return;
			}

			if (!Value)
			{
				context.IsValid = true;
				return;
			}

			var count = context.Instance.GetArrayLength();
			var duplicates = new List<(int, int)>();
			for (int i = 0; i < count - 1; i++)
			for (int j = i + 1; j < count; j++)
			{
				if (context.Instance[i].IsEquivalentTo(context.Instance[j]))
					duplicates.Add((i, j));
			}

			context.IsValid = !duplicates.Any();
			if (!context.IsValid)
			{
				context.IsValid = false;
				var pairs = ZString.Join(", ", duplicates.Select(d => $"({d.Item1}, {d.Item2})"));
				context.Message = $"Found duplicates at the following index pairs: {pairs}";
			}
		}
	}

	public class UniqueItemsKeywordJsonConverter : JsonConverter<UniqueItemsKeyword>
	{
		public override UniqueItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
				throw new JsonException("Expected boolean");

			var number = reader.GetBoolean();

			return new UniqueItemsKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, UniqueItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WriteBoolean(UniqueItemsKeyword.Name, value.Value);
		}
	}
}