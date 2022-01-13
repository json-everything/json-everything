using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema
{
	/// <summary>
	/// Handles `const`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[Vocabulary(Vocabularies.Validation202012Id)]
	[JsonConverter(typeof(ConstKeywordJsonConverter))]
	public class ConstKeyword : IJsonSchemaKeyword, IEquatable<ConstKeyword>
	{
		internal const string Name = "const";

		/// <summary>
		/// The constant value.
		/// </summary>
		public JsonElement Value { get; }

		/// <summary>
		/// Creates a new <see cref="ConstKeyword"/>.
		/// </summary>
		/// <param name="value">The constant value.</param>
		public ConstKeyword(JsonElement value)
		{
			Value = value.Clone();
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			if (Value.IsEquivalentTo(context.LocalInstance))
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail("Expected value to match given value");
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(ConstKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Value.IsEquivalentTo(other.Value);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as ConstKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Value.GetEquivalenceHashCode();
		}
	}

	internal class ConstKeywordJsonConverter : JsonConverter<ConstKeyword>
	{
		public override ConstKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			var element = document.RootElement;

			return new ConstKeyword(element);
		}
		public override void Write(Utf8JsonWriter writer, ConstKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ConstKeyword.Name);
			value.Value.WriteTo(writer);
		}
	}
}