using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `minContains`.
	/// </summary>
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[Vocabulary(Vocabularies.Validation202012Id)]
	[JsonConverter(typeof(MinContainsKeywordJsonConverter))]
	public class MinContainsKeyword : IJsonSchemaKeyword, IEquatable<MinContainsKeyword>
	{
		internal const string Name = "minContains";

		/// <summary>
		/// The minimum expected matching items.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Creates a new <see cref="MinContainsKeyword"/>.
		/// </summary>
		/// <param name="value">The minimum expected matching items.</param>
		public MinContainsKeyword(uint value)
		{
			Value = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			if (Value == 0)
			{
				context.IsValid = true;
				if (context.HasSiblingContexts)
				{
					var containsContext = context.SiblingContexts.FirstOrDefault(c => c.SchemaLocation.Segments.LastOrDefault().Value == ContainsKeyword.Name);
					if (containsContext != null)
						containsContext.IsValid = true;
				}
				return;
			}

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
			context.IsValid = Value <= containsCount;
			if (!context.IsValid)
				context.Message = $"Value has less than {Value} items that matched the schema provided by the {ContainsKeyword.Name} keyword";
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(MinContainsKeyword? other)
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
			return Equals(obj as MinContainsKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return (int)Value;
		}
	}

	internal class MinContainsKeywordJsonConverter : JsonConverter<MinContainsKeyword>
	{
		public override MinContainsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetUInt32();

			return new MinContainsKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MinContainsKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MinContainsKeyword.Name, value.Value);
		}
	}
}