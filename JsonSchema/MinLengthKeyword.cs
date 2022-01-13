using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `minLength`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[Vocabulary(Vocabularies.Validation202012Id)]
	[JsonConverter(typeof(MinLengthKeywordJsonConverter))]
	public class MinLengthKeyword : IJsonSchemaKeyword, IEquatable<MinLengthKeyword>
	{
		internal const string Name = "minLength";

		/// <summary>
		/// The minimum expected string length.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Creates a new <see cref="MinLengthKeyword"/>.
		/// </summary>
		/// <param name="value">The minimum expected string length.</param>
		public MinLengthKeyword(uint value)
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
			if (context.LocalInstance.ValueKind != JsonValueKind.String)
			{
				context.LocalResult.Pass();
				context.WrongValueKind(context.LocalInstance.ValueKind);
				return;
			}

			var length = new StringInfo(context.LocalInstance.GetString()).LengthInTextElements;
			if (Value <= length)
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail($"Value is not longer than or equal to {Value} characters");
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(MinLengthKeyword? other)
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
			return Equals(obj as MinLengthKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return (int)Value;
		}
	}

	internal class MinLengthKeywordJsonConverter : JsonConverter<MinLengthKeyword>
	{
		public override MinLengthKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetUInt32();

			return new MinLengthKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MinLengthKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MinLengthKeyword.Name, value.Value);
		}
	}
}