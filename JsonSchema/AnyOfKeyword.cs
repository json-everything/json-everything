using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Json.Schema;

/// <summary>
/// Handles `anyOf`.
/// </summary>
[SchemaPriority(20)]
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(AnyOfKeywordJsonConverter))]
public class AnyOfKeyword : IJsonSchemaKeyword, ISchemaCollector, IEquatable<AnyOfKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "anyOf";

	/// <summary>
	/// The keywords schema collection.
	/// </summary>
	public IReadOnlyList<JsonSchema> Schemas { get; }

	/// <summary>
	/// Creates a new <see cref="AnyOfKeyword"/>.
	/// </summary>
	/// <param name="values">The set of schemas.</param>
	public AnyOfKeyword(params JsonSchema[] values)
	{
		Schemas = values.ToReadOnlyList() ?? throw new ArgumentNullException(nameof(values));
	}

	/// <summary>
	/// Creates a new <see cref="AnyOfKeyword"/>.
	/// </summary>
	/// <param name="values">The set of schemas.</param>
	public AnyOfKeyword(IEnumerable<JsonSchema> values)
	{
		Schemas = values.ToReadOnlyList() ?? throw new ArgumentNullException(nameof(values));
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	/// <param name="token">The cancellation token used by the caller.</param>
	public async Task Evaluate(EvaluationContext context, CancellationToken token)
	{
		context.EnterKeyword(Name);
		bool overallResult;

		var tokenSource = new CancellationTokenSource();
		token.Register(tokenSource.Cancel);

		var tasks = Schemas.Select(async (schema, i) =>
		{
			if (tokenSource.Token.IsCancellationRequested) return true;

			context.Log(() => $"Processing {Name}[{i}]...");
			var branch = context.ParallelBranch(context.EvaluationPath.Combine(Name, i), schema);
			await branch.Evaluate(tokenSource.Token);
			context.Log(() => $"{Name}[{i}] {branch.LocalResult.IsValid.GetValidityString()}.");
			return branch.LocalResult.IsValid;
		}).ToArray();

		if (context.ApplyOptimizations)
		{
			var passedValidation = await tasks.WhenAny(x => x, tokenSource.Token);
			tokenSource.Cancel();

			overallResult = passedValidation != null;
		}
		else
		{
			await Task.WhenAll(tasks);
			overallResult = tasks.Any(x => x.Result);
		}

		if (!overallResult)
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(AnyOfKeyword? other)
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
		return Equals(obj as AnyOfKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schemas.GetUnorderedCollectionHashCode();
	}
}

internal class AnyOfKeywordJsonConverter : JsonConverter<AnyOfKeyword>
{
	public override AnyOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.StartArray)
		{
			var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options)!;
			return new AnyOfKeyword(schemas);
		}

		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;
		return new AnyOfKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, AnyOfKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(AnyOfKeyword.Name);
		writer.WriteStartArray();
		foreach (var schema in value.Schemas)
		{
			JsonSerializer.Serialize(writer, schema, options);
		}
		writer.WriteEndArray();
	}
}