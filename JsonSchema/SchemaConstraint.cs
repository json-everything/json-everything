using System;
using System.Collections.Generic;
using System.Linq;
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
	private readonly JsonPointer _relativeEvaluationPath;

	/// <summary>
	/// Gets the schema's base URI.
	/// </summary>
	public Uri SchemaBaseUri { get; }

	/// <summary>
	/// Gets the base location within the instance that is being evaluated.
	/// </summary>
	public JsonPointer BaseInstanceLocation { get; }

	/// <summary>
	/// Gets the location relative to <see cref="BaseInstanceLocation"/> within the instance that
	/// is being evaluated.
	/// </summary>
	public JsonPointer RelativeInstanceLocation { get; }

	/// <summary>
	/// Gets the set of keyword constraints.
	/// </summary>
	public KeywordConstraint[] Constraints { get; internal set; } = Array.Empty<KeywordConstraint>();

	/// <summary>
	/// Defines a method to identify relative instance locations at evaluation time.
	/// </summary>
	/// <remarks>
	/// When the relative instance location cannot be determined through static analysis,
	/// e.g. `additionalProperties`, this function can be used to dynamically provide
	/// JSON Pointers to those locations at evaluation time.
	/// </remarks>
	public Func<KeywordEvaluation, IEnumerable<JsonPointer>>? InstanceLocator { get; set; }

	/// <summary>
	/// Gets the local <see cref="JsonSchema"/>.
	/// </summary>
	public JsonSchema LocalSchema { get; }

	internal JsonPointer BaseSchemaOffset { get; set; } = JsonPointer.Empty;
	internal SchemaConstraint? Source { get; set; }
	internal bool UseLocatorAsInstance { get; set; }
	internal JsonArray? UnknownKeywords { get; set; }

	internal SchemaConstraint(JsonPointer relativeEvaluationPath, JsonPointer baseInstanceLocation, JsonPointer relativeInstanceLocation, Uri schemaBaseUri, JsonSchema localSchema)
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
	public SchemaEvaluation BuildEvaluation(JsonNode? localInstance, JsonPointer instanceLocation, JsonPointer evaluationPath, EvaluationOptions options)
	{
		if (Source != null)
			Constraints = Source.Constraints;

		if (RelativeInstanceLocation != JsonPointer.Empty)
			instanceLocation = instanceLocation.Combine(RelativeInstanceLocation);
		if (_relativeEvaluationPath != JsonPointer.Empty)
			evaluationPath = evaluationPath.Combine(_relativeEvaluationPath);

		var evaluation = new SchemaEvaluation(localInstance,
			RelativeInstanceLocation,
			new EvaluationResults(evaluationPath, SchemaBaseUri, instanceLocation, options),
			Constraints.Length == 0
				? Array.Empty<KeywordEvaluation>()
				: new KeywordEvaluation[Constraints.Length],
			options
		) { Id = _id };

		if (BaseSchemaOffset != JsonPointer.Empty)
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