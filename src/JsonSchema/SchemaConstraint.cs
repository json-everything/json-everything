using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Describes the set of constraints a schema object will apply to any instance.
/// </summary>
/// <remarks>
/// This represents any work that can be performed as part of static analysis of the schema.
/// </remarks>
public class SchemaConstraint
{
	private readonly Guid _id = Guid.NewGuid();
	private readonly JsonPointer_Old _relativeEvaluationPath;

	/// <summary>
	/// Gets the schema's base URI.
	/// </summary>
	public Uri SchemaBaseUri { get; }

	/// <summary>
	/// Gets the base location within the instance that is being evaluated.
	/// </summary>
	public JsonPointer_Old BaseInstanceLocation { get; }

	/// <summary>
	/// Gets the location relative to <see cref="BaseInstanceLocation"/> within the instance that
	/// is being evaluated.
	/// </summary>
	public JsonPointer_Old RelativeInstanceLocation { get; }

	/// <summary>
	/// Gets the set of keyword constraints.
	/// </summary>
	public KeywordConstraint[] Constraints { get; internal set; } = [];

	/// <summary>
	/// Defines a method to identify relative instance locations at evaluation time.
	/// </summary>
	/// <remarks>
	/// When the relative instance location cannot be determined through static analysis,
	/// e.g. `additionalProperties`, this function can be used to dynamically provide
	/// JSON Pointers to those locations at evaluation time.
	/// </remarks>
	public Func<KeywordEvaluation, IEnumerable<JsonPointer_Old>>? InstanceLocator { get; set; }

	/// <summary>
	/// Gets the local <see cref="JsonSchema"/>.
	/// </summary>
	public JsonSchema LocalSchema { get; }

	internal JsonPointer_Old BaseSchemaOffset { get; set; } = JsonPointer_Old.Empty;
	internal SchemaConstraint? Source { get; set; }
	internal bool UseLocatorAsInstance { get; set; }
	internal JsonArray? UnknownKeywords { get; set; }

	internal SchemaConstraint(JsonPointer_Old relativeEvaluationPath, JsonPointer_Old baseInstanceLocation, JsonPointer_Old relativeInstanceLocation, Uri schemaBaseUri, JsonSchema localSchema)
	{
		_relativeEvaluationPath = relativeEvaluationPath;
		SchemaBaseUri = schemaBaseUri;
		BaseInstanceLocation = baseInstanceLocation;
		RelativeInstanceLocation = relativeInstanceLocation;
		LocalSchema = localSchema;
	}

	/// <summary>
	/// Builds an evaluation object.
	/// </summary>
	/// <param name="localInstance">The local instance being evaluated.</param>
	/// <param name="instanceLocation">The full instance location.</param>
	/// <param name="evaluationPath">The evaluation path.</param>
	/// <param name="options">Any evaluation options.  This will be passed via the evaluation context.</param>
	/// <returns>A schema evaluation object.</returns>
	public SchemaEvaluation BuildEvaluation(JsonNode? localInstance, JsonPointer_Old instanceLocation, JsonPointer_Old evaluationPath, EvaluationOptions options)
	{
		if (Source != null)
			Constraints = Source.Constraints;

		instanceLocation = instanceLocation.Combine(RelativeInstanceLocation);
		evaluationPath = evaluationPath.Combine(_relativeEvaluationPath);

		var evaluation = new SchemaEvaluation(localInstance,
			RelativeInstanceLocation,
			new EvaluationResults(evaluationPath, SchemaBaseUri, instanceLocation, options),
			Constraints.Length == 0
				? []
				: new KeywordEvaluation[Constraints.Length],
			options
		) { Id = _id };

		if (BaseSchemaOffset != JsonPointer_Old.Empty)
			evaluation.Results.SetSchemaReference(BaseSchemaOffset);

		if (LocalSchema.BoolValue.HasValue)
		{
			if (!LocalSchema.BoolValue.Value)
				evaluation.Results.Fail(string.Empty, ErrorMessages.FalseSchema);

			return evaluation;
		}

		for (int i = 0; i < Constraints.Length; i++)
		{
			evaluation.KeywordEvaluations[i] = Constraints[i].BuildEvaluation(evaluation, instanceLocation, evaluationPath);
		}

		return evaluation;
	}
}