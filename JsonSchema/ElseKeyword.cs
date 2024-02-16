using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `else`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[DependsOnAnnotationsFrom(typeof(IfKeyword))]
[JsonConverter(typeof(ElseKeywordJsonConverter))]
public class ElseKeyword : IJsonSchemaKeyword, ISchemaContainer, IKeywordHandler
{
	public static ElseKeyword Handler { get; } = new(true);

	bool IKeywordHandler.Evaluate(FunctionalEvaluationContext context)
	{
		if (!context.LocalSchema.AsObject().TryGetValue(Name, out var requirement, out _)) return true;

		if (!context.Annotations.TryGetValue(IfKeyword.Name, out var annotation) ||
		    annotation.AsValue().GetBool() == true)
			return true;

		var localContext = context;
		localContext.LocalSchema = requirement!;

		return JsonSchema.Evaluate(localContext);
	}

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
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		var ifConstraint = localConstraints.FirstOrDefault(x => x.Keyword == IfKeyword.Name);
		if (ifConstraint == null)
			return KeywordConstraint.Skip;

		var subschemaConstraint = Schema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);

		return new KeywordConstraint(Name, Evaluator)
		{
			SiblingDependencies = [ifConstraint],
			ChildDependencies = [subschemaConstraint]
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
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

/// <summary>
/// JSON converter for <see cref="ElseKeyword"/>.
/// </summary>
public sealed class ElseKeywordJsonConverter : WeaklyTypedJsonConverter<ElseKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="ElseKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override ElseKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = options.Read(ref reader, JsonSchemaSerializerContext.Default.JsonSchema)!;

		return new ElseKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, ElseKeyword value, JsonSerializerOptions options)
	{
		options.Write(writer, value.Schema, JsonSchemaSerializerContext.Default.JsonSchema);
	}
}