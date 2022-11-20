using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `$vocabulary`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaPriority(long.MinValue)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[SchemaDraft(Draft.DraftNext)]
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(VocabularyKeywordJsonConverter))]
public class VocabularyKeyword : IJsonSchemaKeyword, IEquatable<VocabularyKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$vocabulary";

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
		Vocabulary = values ?? throw new ArgumentNullException(nameof(values));
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		var overallResult = true;
		var violations = new List<Uri>();
		var vocabularies = Vocabulary.ToDictionary(x => x.Key, x => x.Value);
		switch (context.Options.EvaluatingAs)
		{
			case Draft.Unspecified:
			case Draft.Draft201909:
				vocabularies[new Uri(Vocabularies.Core201909Id)] = true;
				break;
			case Draft.Draft202012:
				vocabularies[new Uri(Vocabularies.Core202012Id)] = true;
				break;
			case Draft.DraftNext:
				vocabularies[new Uri(Vocabularies.CoreNextId)] = true;
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

		if (!overallResult)
			context.LocalResult.Fail(Name, ErrorMessages.UnknownVocabularies, ("vocabs", $"[{string.Join(", ", violations)}]"));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, Uri baseUri, JsonPointer instanceLocation, EvaluationOptions options)
	{
		return Enumerable.Empty<Requirement>();
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(VocabularyKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (Vocabulary.Count != other.Vocabulary.Count) return false;
		var byUri = Vocabulary.Join(other.Vocabulary,
				tv => tv.Key.OriginalString,
				ov => ov.Key.OriginalString,
				(tv, ov) => new { ThisVocab = tv.Value, OtherVocab = ov.Value })
			.ToArray();
		if (Vocabulary.Count != byUri.Length) return false;
		return byUri.All(x => x.ThisVocab == x.OtherVocab);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as VocabularyKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Vocabulary.GetCollectionHashCode();
	}
}

internal class VocabularyKeywordJsonConverter : JsonConverter<VocabularyKeyword>
{
	public override VocabularyKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var schema = JsonSerializer.Deserialize<Dictionary<string, bool>>(ref reader, options);
		var withUris = schema!.ToDictionary(kvp => new Uri(kvp.Key), kvp => kvp.Value);
		return new VocabularyKeyword(withUris);
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

public static partial class ErrorMessages
{
	private static string? _unknownVocabularies;

	/// <summary>
	/// Gets or sets the error message for when a vocabulary is unknown but required.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[vocabs]] - the URI IDs of the missing vocabularies as a comma-delimited list
	/// </remarks>
	public static string UnknownVocabularies
	{
		get => _unknownVocabularies ?? Get();
		set => _unknownVocabularies = value;
	}
}