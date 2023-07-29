using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

public class SchemaConstraint
{
	private readonly JsonSchema _localSchema;
	private readonly JsonPointer _relativeEvaluationPath;

	public Uri SchemaBaseUri { get; }
	public JsonPointer RelativeInstanceLocation { get; }
	public JsonPointer BaseInstanceLocation { get; }

	public KeywordConstraint[] Constraints { get; set; } = Array.Empty<KeywordConstraint>();
	public Func<KeywordEvaluation, IEnumerable<JsonPointer>>? InstanceLocator { get; set; }

	internal Guid Id { get; } = Guid.NewGuid();
	internal JsonPointer BaseSchemaOffset { get; set; } = JsonPointer.Empty;
	internal SchemaConstraint? Source { get; set; }
	internal bool UseLocatorAsInstance { get; set; }

	internal SchemaConstraint(JsonPointer relativeEvaluationPath, JsonPointer baseInstanceLocation, JsonPointer relativeInstanceLocation, Uri schemaBaseUri, JsonSchema localSchema)
	{
		_relativeEvaluationPath = relativeEvaluationPath;
		SchemaBaseUri = schemaBaseUri;
		BaseInstanceLocation = baseInstanceLocation;
		RelativeInstanceLocation = relativeInstanceLocation;
		_localSchema = localSchema;
	}

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
		) { Id = Id };

		if (BaseSchemaOffset != JsonPointer.Empty)
			evaluation.Results.SetSchemaReference(BaseSchemaOffset);

		if (_localSchema.BoolValue.HasValue)
		{
			if (!_localSchema.BoolValue.Value)
				evaluation.Results.Fail(string.Empty, "All values fail against the false schema");

			return evaluation;
		}

		for (int i = 0; i < Constraints.Length; i++)
		{
			evaluation.KeywordEvaluations[i] = Constraints[i].BuildEvaluation(evaluation, instanceLocation, evaluationPath);
		}

		return evaluation;
	}
}