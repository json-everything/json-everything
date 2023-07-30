using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `else`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaPriority(10)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[DependsOnAnnotationsFrom(typeof(IfKeyword))]
[JsonConverter(typeof(ElseKeywordJsonConverter))]
public class ElseKeyword : IJsonSchemaKeyword, ISchemaContainer
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "else";

	/// <summary>
	/// The schema to match.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="ElseKeyword"/>.
	/// </summary>
	/// <param name="value">The schema to match.</param>
	public ElseKeyword(JsonSchema value)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, ConstraintBuilderContext context)
	{
		var ifConstraint = localConstraints.FirstOrDefault(x => x.Keyword == IfKeyword.Name);
		if (ifConstraint == null)
			return KeywordConstraint.Skip;

		var subschemaConstraint = Schema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);

		return new KeywordConstraint(Name, Evaluator)
		{
			SiblingDependencies = new[] { ifConstraint },
			ChildDependencies = new[] { subschemaConstraint }
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, ConstraintBuilderContext context)
	{
		if (!evaluation.Results.TryGetAnnotation(IfKeyword.Name, out var ifAnnotation) || ifAnnotation!.GetValue<bool>())
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var subSchemaEvaluation = evaluation.ChildEvaluations[0];
		if (!subSchemaEvaluation.Results.IsValid)
			evaluation.Results.Fail();
	}
}

internal class ElseKeywordJsonConverter : JsonConverter<ElseKeyword>
{
	public override ElseKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;

		return new ElseKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, ElseKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(ElseKeyword.Name);
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}