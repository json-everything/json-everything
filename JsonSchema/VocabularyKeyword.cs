using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `$vocabulary`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(VocabularyKeywordJsonConverter))]
public class VocabularyKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$vocabulary";

	private Dictionary<Uri, bool> _allVocabularies;

	/// <summary>
	/// The collection of vocabulary requirements.
	/// </summary>
	public IReadOnlyDictionary<Uri, bool> Vocabulary { get; }

	/// <summary>
	/// Creates a new <see cref="VocabularyKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of vocabulary requirements.</param>
#pragma warning disable CS8618
	public VocabularyKeyword(IReadOnlyDictionary<Uri, bool> values)
	{
		Vocabulary = values ?? throw new ArgumentNullException(nameof(values));
	}
#pragma warning restore CS8618

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	/// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	/// Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		_allVocabularies = Vocabulary.ToDictionary(x => x.Key, x => x.Value);
		switch (context.EvaluatingAs)
		{
			case SpecVersion.Unspecified:
			case SpecVersion.Draft201909:
				_allVocabularies[new Uri(Vocabularies.Core201909Id)] = true;
				break;
			case SpecVersion.Draft202012:
				_allVocabularies[new Uri(Vocabularies.Core202012Id)] = true;
				break;
			case SpecVersion.DraftNext:
				_allVocabularies[new Uri(Vocabularies.CoreNextId)] = true;
				break;
		}

		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var violations = new List<Uri>();
		var overallResult = true;
		foreach (var kvp in _allVocabularies)
		{
			var isKnown = context.Options.VocabularyRegistry.IsKnown(kvp.Key);
			var isValid = !kvp.Value || isKnown;
			if (!isValid)
				violations.Add(kvp.Key);
			overallResult &= isValid;
		}

		if (!overallResult)
			evaluation.Results.Fail(Name, ErrorMessages.GetUnknownVocabularies(context.Options.Culture), ("vocabs", $"[{string.Join(", ", violations)}]"));
	}
}

/// <summary>
/// JSON converter for <see cref="VocabularyKeyword"/>.
/// </summary>
public sealed class VocabularyKeywordJsonConverter : JsonConverter<VocabularyKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="VocabularyKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override VocabularyKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var schema = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.DictionaryStringBoolean);
		var withUris = schema!.ToDictionary(kvp => new Uri(kvp.Key), kvp => kvp.Value);
		return new VocabularyKeyword(withUris);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, VocabularyKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var kvp in value.Vocabulary)
		{
			writer.WriteBoolean(kvp.Key.OriginalString, kvp.Value);
		}
		writer.WriteEndObject();
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for when a vocabulary is unknown but required.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[vocabs]] - the URI IDs of the missing vocabularies as a comma-delimited list
	/// </remarks>
	public static string? UnknownVocabularies { get; set; }

	/// <summary>
	/// Gets or sets the error message for when a vocabulary is unknown but required.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[vocabs]] - the URI IDs of the missing vocabularies as a comma-delimited list
	/// </remarks>
	public static string GetUnknownVocabularies(CultureInfo? culture)
	{
		return UnknownVocabularies ?? Get(culture);
	}
}