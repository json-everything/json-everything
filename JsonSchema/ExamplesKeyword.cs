using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema
{
	/// <summary>
	/// Handles `examples`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Metadata201909Id)]
	[Vocabulary(Vocabularies.Metadata202012Id)]
	[JsonConverter(typeof(ExamplesKeywordJsonConverter))]
	public class ExamplesKeyword : IJsonSchemaKeyword, IEquatable<ExamplesKeyword>
	{
		internal const string Name = "examples";

		/// <summary>
		/// The collection of example values.
		/// </summary>
		public IReadOnlyList<JsonElement> Values { get; }

		/// <summary>
		/// Creates a new <see cref="ExamplesKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of example values.</param>
		public ExamplesKeyword(params JsonElement[] values)
		{
			Values = values?.Select(e => e.Clone()).ToList() ?? throw new ArgumentNullException(nameof(values));
		}

		/// <summary>
		/// Creates a new <see cref="ExamplesKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of example values.</param>
		public ExamplesKeyword(IEnumerable<JsonElement> values)
		{
			Values = values?.Select(e => e.Clone()).ToList() ?? throw new ArgumentNullException(nameof(values));
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			context.LocalResult.SetAnnotation(Name, Values);
			context.LocalResult.Pass();
			context.ExitKeyword(Name, true);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(ExamplesKeyword? other)
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
			return Equals(obj as ExamplesKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Values.GetUnorderedCollectionHashCode(element => element.GetEquivalenceHashCode());
		}
	}

	internal class ExamplesKeywordJsonConverter : JsonConverter<ExamplesKeyword>
	{
		public override ExamplesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);

			if (document.RootElement.ValueKind != JsonValueKind.Array)
				throw new JsonException("Expected array");

			return new ExamplesKeyword(document.RootElement.EnumerateArray());
		}
		public override void Write(Utf8JsonWriter writer, ExamplesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ExamplesKeyword.Name);
			writer.WriteStartArray();
			foreach (var element in value.Values)
			{
				writer.WriteValue(element);
			}
			writer.WriteEndArray();
		}
	}
}