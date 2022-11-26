using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `oneOf`.
/// </summary>
[Applicator]
[SchemaPriority(20)]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[SchemaDraft(Draft.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(OneOfKeywordJsonConverter))]
public class OneOfKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaCollector, IEquatable<OneOfKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "oneOf";

	/// <summary>
	/// The keywords schema collection.
	/// </summary>
	public IReadOnlyList<JsonSchema> Schemas { get; }

	/// <summary>
	/// Creates a new <see cref="OneOfKeyword"/>.
	/// </summary>
	/// <param name="values">The keywords schema collection.</param>
	public OneOfKeyword(params JsonSchema[] values)
	{
		Schemas = values.ToReadOnlyList() ?? throw new ArgumentNullException(nameof(values));
	}

	/// <summary>
	/// Creates a new <see cref="OneOfKeyword"/>.
	/// </summary>
	/// <param name="values">The keywords schema collection.</param>
	public OneOfKeyword(IEnumerable<JsonSchema> values)
	{
		Schemas = values.ToReadOnlyList();
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		var validCount = 0;
		for (var i = 0; i < Schemas.Count; i++)
		{
			var i1 = i;
			context.Log(() => $"Processing {Name}[{i1}]...");
			var schema = Schemas[i];
			context.Push(context.EvaluationPath.Combine(Name, i), schema);
			context.Evaluate();
			validCount += context.LocalResult.IsValid ? 1 : 0;
			context.Log(() => $"{Name}[{i1}] {context.LocalResult.IsValid.GetValidityString()}.");
			context.Pop();
			if (validCount > 1 && context.ApplyOptimizations) break;
		}

		if (validCount != 1)
			context.LocalResult.Fail(Name, ErrorMessages.OneOf, ("count", validCount));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, DynamicScope scope, JsonPointer instanceLocation, EvaluationOptions options)
	{
		var relevantEvaluationPaths = Enumerable.Range(0, Schemas.Count).Select(i => subschemaPath.Combine(Name, i));

		for (var i = 0; i < Schemas.Count; i++)
		{
			var subschema = Schemas[i];
			foreach (var requirement in subschema.GenerateRequirements(scope, subschemaPath.Combine(Name, i), instanceLocation, options))
			{
				yield return requirement;
			}
		}

		yield return new Requirement(subschemaPath, instanceLocation,
			(_, cache, _) =>
			{
				var relevantResults = cache.Where(x => relevantEvaluationPaths.Contains(x.SubschemaPath));
				var groupedBySubschema = relevantResults.GroupBy(x => x.SubschemaPath);
				var validCount = groupedBySubschema.Count(x => x.All(y => y.ValidationResult != false));

				return new KeywordResult(Name, subschemaPath, scope.LocalScope, instanceLocation)
				{
					ValidationResult = validCount == 1,
					Error = ErrorMessages.OneOf.ReplaceTokens(("count", validCount))
				};
			});
	}

	void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		foreach (var schema in Schemas)
		{
			schema.RegisterSubschemas(registry, currentUri);
		}
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(OneOfKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Schemas.ContentsEqual(other.Schemas);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as OneOfKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schemas.GetUnorderedCollectionHashCode();
	}
}

internal class OneOfKeywordJsonConverter : JsonConverter<OneOfKeyword>
{
	public override OneOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.StartArray)
		{
			var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options)!;
			return new OneOfKeyword(schemas);
		}

		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;
		return new OneOfKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, OneOfKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(OneOfKeyword.Name);
		writer.WriteStartArray();
		foreach (var schema in value.Schemas)
		{
			JsonSerializer.Serialize(writer, schema, options);
		}
		writer.WriteEndArray();
	}
}

public static partial class ErrorMessages
{
	private static string? _oneOf;

	/// <summary>
	/// Gets or sets the error message for <see cref="OneOfKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[count]] - the number of subschemas that passed validation
	/// </remarks>
	public static string OneOf
	{
		get => _oneOf ?? Get();
		set => _oneOf = value;
	}
}