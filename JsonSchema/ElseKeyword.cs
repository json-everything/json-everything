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
public class ElseKeyword : IJsonSchemaKeyword, ISchemaContainer, IEquatable<ElseKeyword>
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

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		if (!context.LocalResult.TryGetAnnotation(IfKeyword.Name, out var annotation))
		{
			context.NotApplicable(() => $"No annotation found for {IfKeyword.Name}.");
			return;
		}

		context.Log(() => $"Annotation for {IfKeyword.Name} is {annotation.AsJsonString()}.");
		var ifResult = annotation!.GetValue<bool>();
		if (ifResult)
		{
			context.NotApplicable(() => $"{Name} does not apply.");
			return;
		}

		context.Push(context.EvaluationPath.Combine(Name), Schema);
		context.Evaluate();
		var valid = context.LocalResult.IsValid;
		context.Pop();
		if (!valid) 
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
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

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(ElseKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Equals(Schema, other.Schema);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as ElseKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schema.GetHashCode();
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