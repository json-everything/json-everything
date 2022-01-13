using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[Vocabulary(Vocabularies.Validation202012Id)]
	[JsonConverter(typeof(UniqueItemsKeywordJsonConverter))]
	public class UniqueItemsKeyword : IJsonSchemaKeyword, IEquatable<UniqueItemsKeyword>
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
			context.EnterKeyword(Name);
			if (context.LocalInstance.ValueKind != JsonValueKind.Array)
			{
				context.LocalResult.Pass();
				context.WrongValueKind(context.LocalInstance.ValueKind);
				return;
			}

			if (!Value)
			{
				context.LocalResult.Pass();
				context.ExitKeyword(Name, true);
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

			if (!duplicates.Any())
				context.LocalResult.Pass();
			else
			{
				var pairs = string.Join(", ", duplicates.Select(d => $"({d.Item1}, {d.Item2})"));
				context.LocalResult.Fail($"Found duplicates at the following index pairs: {pairs}");
			}
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(UniqueItemsKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Value == other.Value;
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as UniqueItemsKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Value.GetHashCode();
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