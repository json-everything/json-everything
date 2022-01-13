using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

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

        private readonly HashSet<JsonElement> _values;

        /// <summary>
        /// The collection of enum values.
        /// </summary>
        /// <remarks>
        /// Enum values aren't necessarily strings; they can be of any JSON value.
        /// </remarks>
        public IReadOnlyCollection<JsonElement> Values => _values;

		/// <summary>
		/// Creates a new <see cref="EnumKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of enum values.</param>
		public EnumKeyword(params JsonElement[] values)
        {
            _values = new HashSet<JsonElement>(values?.Select(e => e.Clone()) ?? throw new ArgumentNullException(nameof(values)),
                JsonElementEqualityComparer.Instance);

            if (_values.Count != values.Length)
                throw new ArgumentException("`enum` requires unique values");
        }

		/// <summary>
		/// Creates a new <see cref="EnumKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of enum values.</param>
		public EnumKeyword(IEnumerable<JsonElement> values)
		{
            _values = new HashSet<JsonElement>(values?.Select(e => e.Clone()).ToList() ?? throw new ArgumentNullException(nameof(values)),
                JsonElementEqualityComparer.Instance);

            if (_values.Count != values.Count())
                throw new ArgumentException("`enum` requires unique values");
        }

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			if (Values.Contains(context.LocalInstance, JsonElementEqualityComparer.Instance))
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail("Expected value to match one of the values specified by the enum");
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(EnumKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			// Don't need ContentsEqual here because that method considers counts.
			// We know that with a hash set, all counts are 1.
            return Values.Count == other.Values.Count &&
                   Values.All(x => other.Values.Contains(x, JsonElementEqualityComparer.Instance));
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
			using var document = JsonDocument.ParseValue(ref reader);

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