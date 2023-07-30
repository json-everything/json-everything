using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `if`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(IfKeywordJsonConverter))]
public class IfKeyword : IJsonSchemaKeyword, ISchemaContainer
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "if";

	/// <summary>
	/// The schema to match.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="IfKeyword"/>.
	/// </summary>
	/// <param name="value">The schema to match.</param>
	public IfKeyword(JsonSchema value)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		var subschemaDependency = Schema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = new[] { subschemaDependency }
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var subSchemaEvaluation = evaluation.ChildEvaluations.Single();

		evaluation.Results.SetAnnotation(Name, subSchemaEvaluation.Results.IsValid);
	}
}

internal class IfKeywordJsonConverter : JsonConverter<IfKeyword>
{
	public override IfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;

		return new IfKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, IfKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(IfKeyword.Name);
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}