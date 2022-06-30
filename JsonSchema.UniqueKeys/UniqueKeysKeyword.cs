using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema.UniqueKeys;

/// <summary>
/// Represents the `data` keyword.
/// </summary>
[SchemaKeyword(Name)]
[SchemaPriority(int.MinValue)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.UniqueKeysId)]
[JsonConverter(typeof(UniqueKeysKeywordJsonConverter))]
public class UniqueKeysKeyword : IJsonSchemaKeyword, IEquatable<UniqueKeysKeyword>
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

	internal const string Name = "uniqueKeys";

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
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.Array)
		{
			context.LocalResult.Pass();
			context.WrongValueKind(schemaValueType);
			return;
		}

		var array = (JsonArray)context.LocalInstance!;
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
				{
					if (context.Options.OutputFormat == OutputFormat.Flag)
					{
						context.LocalResult.Fail(Name, $"Found duplicate items at indices {i} and {j}");
						context.ExitKeyword(Name);
						return;
					}
					matchedIndexPairs.Add((i, j));
				}
			}
		}

		if (!matchedIndexPairs.Any())
			context.LocalResult.Pass();
		else
		{
			var pairs = string.Join(", ", matchedIndexPairs.Select(d => $"({d.Item1}, {d.Item2})"));
			context.LocalResult.Fail(Name, ErrorMessages.UniqueItems, ("pairs", pairs));
		}
		context.ExitKeyword(Name);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(UniqueKeysKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;

		return Keys.SequenceEqual(other.Keys);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as UniqueKeysKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Keys.GetHashCode();
	}
}

internal class UniqueKeysKeywordJsonConverter : JsonConverter<UniqueKeysKeyword>
{
	public override UniqueKeysKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected array");

		var references = JsonSerializer.Deserialize<List<JsonPointer>>(ref reader, options)!;
		return new UniqueKeysKeyword(references);
	}

	public override void Write(Utf8JsonWriter writer, UniqueKeysKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(UniqueKeysKeyword.Name);
		JsonSerializer.Serialize(writer, value.Keys, options);
	}
}