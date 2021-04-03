using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `enum`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[Vocabulary(Vocabularies.Validation202012Id)]
	[JsonConverter(typeof(EnumKeywordJsonConverter))]
	public class EnumKeyword : IJsonSchemaKeyword, IEquatable<EnumKeyword>
	{
		internal const string Name = "enum";

		/// <summary>
		/// The collection of enum values (they don't need to be strings).
		/// </summary>
		public IReadOnlyList<JsonElement> Values { get; }

		/// <summary>
		/// Creates a new <see cref="EnumKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of enum values.</param>
		public EnumKeyword(params JsonElement[] values)
		{
			Values = values?.Select(e => e.Clone()).ToList() ?? throw new ArgumentNullException(nameof(values));
		}

		/// <summary>
		/// Creates a new <see cref="EnumKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of enum values.</param>
		public EnumKeyword(IEnumerable<JsonElement> values)
		{
			Values = values?.Select(e => e.Clone()).ToList() ?? throw new ArgumentNullException(nameof(values));
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.Options.Log.EnterKeyword(Name);
			context.IsValid = Values.Contains(context.LocalInstance, JsonElementEqualityComparer.Instance);
			if (!context.IsValid)
				context.Message = "Expected value to match one of the values specified by the enum";
			context.Options.Log.ExitKeyword(Name, context.IsValid);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(EnumKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Values.ContentsEqual(other.Values, JsonElementEqualityComparer.Instance);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as EnumKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Values.GetUnorderedCollectionHashCode(element => element.GetEquivalenceHashCode());
		}
	}

	internal class EnumKeywordJsonConverter : JsonConverter<EnumKeyword>
	{
		public override EnumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var document = JsonDocument.ParseValue(ref reader);

			if (document.RootElement.ValueKind != JsonValueKind.Array)
				throw new JsonException("Expected array");

			if (document.RootElement.GetArrayLength() == 0)
				throw new JsonException("Enums must contain a value");

			return new EnumKeyword(document.RootElement.EnumerateArray());
		}
		public override void Write(Utf8JsonWriter writer, EnumKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(EnumKeyword.Name);
			writer.WriteStartArray();
			foreach (var element in value.Values)
			{
				writer.WriteValue(element);
			}
			writer.WriteEndArray();
		}
	}
}