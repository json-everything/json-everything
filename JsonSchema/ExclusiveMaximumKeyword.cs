using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `exclusiveMaximum`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[Vocabulary(Vocabularies.Validation202012Id)]
	[JsonConverter(typeof(ExclusiveMaximumKeywordJsonConverter))]
	public class ExclusiveMaximumKeyword : IJsonSchemaKeyword, IEquatable<ExclusiveMaximumKeyword>
	{
		internal const string Name = "exclusiveMaximum";
	
		/// <summary>
		/// The maximum value.
		/// </summary>
		public decimal Value { get; }

		/// <summary>
		/// Creates a new <see cref="ExclusiveMaximumKeyword"/>.
		/// </summary>
		/// <param name="value">The maximum value.</param>
		public ExclusiveMaximumKeyword(decimal value)
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
			if (context.LocalInstance.ValueKind != JsonValueKind.Number)
			{
				context.LocalResult.Pass();
				context.WrongValueKind(context.LocalInstance.ValueKind);
				return;
			}

			var number = context.LocalInstance.GetDecimal();
			if (Value > number)
				context.LocalResult.Pass();
			else 
				context.LocalResult.Fail($"{number} is not greater than {Value}");
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(ExclusiveMaximumKeyword? other)
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
			return Equals(obj as ExclusiveMaximumKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}

	internal class ExclusiveMaximumKeywordJsonConverter : JsonConverter<ExclusiveMaximumKeyword>
	{
		public override ExclusiveMaximumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new ExclusiveMaximumKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, ExclusiveMaximumKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(ExclusiveMaximumKeyword.Name, value.Value);
		}
	}
}