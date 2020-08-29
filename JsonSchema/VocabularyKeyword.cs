using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `$vocabulary`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaPriority(long.MinValue)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[JsonConverter(typeof(VocabularyKeywordJsonConverter))]
	public class VocabularyKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$vocabulary";

		/// <summary>
		/// The collection of vocabulary requirements.
		/// </summary>
		public IReadOnlyDictionary<Uri, bool> Vocabulary { get; }

		/// <summary>
		/// Creates a new <see cref="VocabularyKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of vocabulary requirements.</param>
		public VocabularyKeyword(IReadOnlyDictionary<Uri, bool> values)
		{
			Vocabulary = values;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			var overallResult = true;
			var violations = new List<Uri>();
			var vocabularies = Vocabulary.ToDictionary(x => x.Key, x => x.Value);
			switch (context.Options.ValidateAs)
			{
				case Draft.Unspecified:
				case Draft.Draft201909:
					vocabularies[new Uri(Vocabularies.Core201909Id)] = true;
					break;
			}
			foreach (var kvp in vocabularies)
			{
				var isKnown = context.Options.VocabularyRegistry.IsKnown(kvp.Key);
				var isValid = !kvp.Value || isKnown;
				if (!isValid)
					violations.Add(kvp.Key);
				overallResult &= isValid;
				if (!overallResult && context.ApplyOptimizations) break;
			}
			context.IsValid = overallResult;
			if (!overallResult)
				context.Message = $"Validator does not know about these required vocabularies: [{string.Join(", ", violations)}]";
		}
	}

	internal class VocabularyKeywordJsonConverter : JsonConverter<VocabularyKeyword>
	{
		public override VocabularyKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var schema = JsonSerializer.Deserialize<Dictionary<Uri, bool>>(ref reader, options);
			return new VocabularyKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, VocabularyKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(VocabularyKeyword.Name);
			writer.WriteStartObject();
			foreach (var kvp in value.Vocabulary)
			{
				writer.WriteBoolean(kvp.Key.OriginalString, kvp.Value);
			}
			writer.WriteEndObject();
		}
	}
}