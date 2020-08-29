using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cysharp.Text;
using Json.More;

namespace Json.Schema
{
	/// <summary>
	/// Handles `uniqueItems`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(UniqueItemsKeywordJsonConverter))]
	public class UniqueItemsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "uniqueItems";

		/// <summary>
		/// Whether items should be unique.
		/// </summary>
		public bool Value { get; }

		/// <summary>
		/// Creates a new <see cref="UniqueItemsKeyword"/>.
		/// </summary>
		/// <param name="value">Whether items should be unique.</param>
		public UniqueItemsKeyword(bool value)
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

			if (!Value)
			{
				context.IsValid = true;
				return;
			}

			var count = context.LocalInstance.GetArrayLength();
			var duplicates = new List<(int, int)>();
			for (int i = 0; i < count - 1; i++)
			for (int j = i + 1; j < count; j++)
			{
				if (context.LocalInstance[i].IsEquivalentTo(context.LocalInstance[j]))
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

	internal class UniqueItemsKeywordJsonConverter : JsonConverter<UniqueItemsKeyword>
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