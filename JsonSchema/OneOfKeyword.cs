using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Json.Schema;

/// <summary>
/// Handles `oneOf`.
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
[JsonConverter(typeof(OneOfKeywordJsonConverter))]
public class OneOfKeyword : IJsonSchemaKeyword, ISchemaCollector, IEquatable<OneOfKeyword>
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
	/// <param name="token">The cancellation token used by the caller.</param>
	public async Task Evaluate(EvaluationContext context, CancellationToken token)
	{
		context.EnterKeyword(Name);
		var validCount = 0;

		var tokenSource = new CancellationTokenSource();
		token.Register(tokenSource.Cancel);

		var tasks = Schemas.Select(async (schema, i) =>
		{
			if (tokenSource.Token.IsCancellationRequested) return true;

			context.Log(() => $"Processing {Name}[{i}]...");
			var branch = context.ParallelBranch(context.EvaluationPath.Combine(Name, i), schema);
			await branch.Evaluate();
			context.Log(() => $"{Name}[{i}] {context.LocalResult.IsValid.GetValidityString()}.");
			return branch.LocalResult.IsValid;
		}).ToList();

		if (context.ApplyOptimizations)
		{
			var passedValidation = await tasks.WhenAny(x => x, tokenSource.Token);
			tasks.Remove(passedValidation);
			if (passedValidation != null)
				validCount++;

			if (tasks.Any())
			{
				var otherPassedValidation = await tasks.WhenAny(x => x, tokenSource.Token);
				if (otherPassedValidation != null)
				{
					tokenSource.Cancel();
					validCount++;
				}
			}
		}
		else
		{
			await Task.WhenAll(tasks);
			validCount = tasks.Count(x => x.Result);
		}

		if (validCount != 1)
			context.LocalResult.Fail(Name, ErrorMessages.OneOf, ("count", validCount));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
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