using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaPriority(long.MinValue)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(VocabularyRegistry.Core201909Id)]
	[JsonConverter(typeof(VocabularyKeywordJsonConverter))]
	public class VocabularyKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$vocabulary";

		public IReadOnlyDictionary<Uri, bool> Vocabulary { get; }

		static VocabularyKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		public VocabularyKeyword(IReadOnlyDictionary<Uri, bool> values)
		{
			Vocabulary = values;
		}

		public void Validate(ValidationContext context)
		{
			var overallResult = true;
			var violations = new List<Uri>();
			foreach (var kvp in Vocabulary)
			{
				var isKnown = context.VocabularyRegistry.IsKnown(kvp.Key);
				var isValid = !kvp.Value || isKnown;
				if (!isValid)
					violations.Add(kvp.Key);
				overallResult &= isValid;
			}
			context.IsValid = overallResult;
			if (!overallResult)
				context.Message = $"Validator does not know about these required vocabularies: [{string.Join(", ", violations)}]";
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			var allVocabulary = sourceContexts.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			if (destContext.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allVocabulary);
			else if (allVocabulary.Any())
				destContext.Annotations[Name] = allVocabulary;
		}
	}

	public class VocabularyKeywordJsonConverter : JsonConverter<VocabularyKeyword>
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