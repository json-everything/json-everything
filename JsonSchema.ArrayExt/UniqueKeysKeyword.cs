using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Represents the `data` keyword.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.ArrayExtId)]
[JsonConverter(typeof(UniqueKeysKeywordJsonConverter))]
public class UniqueKeysKeyword : IJsonSchemaKeyword
{
	private class MaybeJsonNodeComparer : IEqualityComparer<(bool resolved, JsonNode? node)>
	{
		public static IEqualityComparer<(bool resolved, JsonNode? node)> Instance { get; } = new MaybeJsonNodeComparer();

		public bool Equals((bool resolved, JsonNode? node) x, (bool resolved, JsonNode? node) y)
		{
			if (!x.resolved) return !y.resolved;
			if (!y.resolved) return false;

			return JsonNodeEqualityComparer.Instance.Equals(x.node, y.node);
		}

		public int GetHashCode((bool resolved, JsonNode? node) obj)
		{
			return !obj.resolved ? 0 : JsonNodeEqualityComparer.Instance.GetHashCode(obj.node);
		}
	}

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "uniqueKeys";

	/// <summary>
	/// The collection of keywords and references.
	/// </summary>
	public IEnumerable<JsonPointer> Keys { get; }

	/// <summary>
	/// Creates an instance of the <see cref="UniqueKeysKeyword"/> class.
	/// </summary>
	/// <param name="references">The collection of keywords and references.</param>
	public UniqueKeysKeyword(IEnumerable<JsonPointer> references)
	{
		Keys = references;
	}

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
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (evaluation.LocalInstance is not JsonArray array) return;

		var collections = new List<List<(bool, JsonNode?)>>();
		foreach (var item in array)
		{
			var values = Keys.Select(x => (x.TryEvaluate(item, out var resolved), resolved));
			collections.Add(values.ToList());
		}

		var matchedIndexPairs = new List<(int, int)>();
		for (int i = 0; i < collections.Count; i++)
		{
			for (int j = i + 1; j < collections.Count; j++)
			{
				var a = collections[i];
				var b = collections[j];

				if (a.SequenceEqual(b, MaybeJsonNodeComparer.Instance)) 
					matchedIndexPairs.Add((i, j));
			}
		}

		if (matchedIndexPairs.Any())
		{
			var pairs = string.Join(", ", matchedIndexPairs.Select(d => $"({d.Item1}, {d.Item2})"));
			evaluation.Results.Fail(Name, ErrorMessages.GetUniqueItems(context.Options.Culture), ("duplicates", pairs));
		}
	}
}

/// <summary>
/// JSON converter for <see cref="UniqueKeysKeyword"/>.
/// </summary>
public sealed class UniqueKeysKeywordJsonConverter : JsonConverter<UniqueKeysKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="UniqueKeysKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override UniqueKeysKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected array");

		var references = JsonSerializer.Deserialize<List<JsonPointer>>(ref reader, options)!;
		return new UniqueKeysKeyword(references);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, UniqueKeysKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(UniqueKeysKeyword.Name);
		JsonSerializer.Serialize(writer, value.Keys, options);
	}
}