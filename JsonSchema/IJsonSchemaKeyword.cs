using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Defines basic functionality for schema keywords.
/// </summary>
public interface IJsonSchemaKeyword
{
	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	void Evaluate(EvaluationContext context);
}

public interface IConstrainer
{
	KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IEnumerable<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context);
}

public class SchemaConstraint
{
	private readonly JsonSchema _localSchema;
	
	public JsonPointer EvaluationPath { get; }
	public Uri SchemaLocation { get; }
	public JsonPointer InstanceLocation { get; }

	public JsonPointer RelativeSchemaLocation { get; set; } = JsonPointer.Empty;
	public KeywordConstraint[] Constraints { get; set; } = Array.Empty<KeywordConstraint>();

	internal SchemaConstraint? Source { get; set; }

	internal SchemaConstraint(JsonPointer evaluationPath, Uri schemaLocation, JsonPointer instanceLocation, JsonSchema localSchema)
	{
		EvaluationPath = evaluationPath;
		SchemaLocation = schemaLocation;
		InstanceLocation = instanceLocation;
		_localSchema = localSchema;
	}

	internal SchemaEvaluation BuildEvaluation(JsonNode? localInstance, JsonPointer instanceLocation, JsonPointer evaluationPath)
	{
		if (Source != null)
			Constraints = Source.Constraints;

		if (InstanceLocation != JsonPointer.Empty)
			instanceLocation = instanceLocation.Combine(InstanceLocation);
		if (EvaluationPath != JsonPointer.Empty)
			evaluationPath = evaluationPath.Combine(EvaluationPath);

		var evaluation = new SchemaEvaluation(localInstance,
			new EvaluationResults(evaluationPath, SchemaLocation, instanceLocation),
			Constraints.Length == 0
				? Array.Empty<KeywordEvaluation>()
				: new KeywordEvaluation[Constraints.Length]
		);

		if (RelativeSchemaLocation != JsonPointer.Empty)
			evaluation.Results.SetSchemaReference(RelativeSchemaLocation);

		if (_localSchema.BoolValue.HasValue)
		{
			if (!_localSchema.BoolValue.Value)
				evaluation.Results.Fail();

			return evaluation;
		}

		for (int i = 0; i < Constraints.Length; i++)
		{
			evaluation.KeywordEvaluations[i] = Constraints[i].BuildEvaluation(evaluation, instanceLocation, evaluationPath);
		}

		return evaluation;
	}
}

public class SchemaEvaluation
{
	internal static SchemaEvaluation Skip { get; } = new(null, new EvaluationResults(JsonPointer.Empty, new Uri("schema://nope"), JsonPointer.Empty), Array.Empty<KeywordEvaluation>());

	public JsonNode? LocalInstance { get; }
	public EvaluationResults Results { get; }

	public KeywordEvaluation[] KeywordEvaluations { get; }

	internal SchemaEvaluation(JsonNode? localInstance, EvaluationResults results, KeywordEvaluation[] evaluations)
	{
		LocalInstance = localInstance;
		Results = results;
		KeywordEvaluations = evaluations;
	}

	public void Evaluate()
	{
		foreach (var keyword in KeywordEvaluations)
		{
			keyword.Evaluate();
		}

		if (!KeywordEvaluations.All(x => x.Results.IsValid))
			Results.Fail();
	}
}

public class KeywordConstraint
{
	public static KeywordConstraint Skip { get; } = new(string.Empty, _ => { });

	public string? Keyword { get; }
	public Action<KeywordEvaluation> Evaluator { get; }

	public KeywordConstraint[] KeywordDependencies { get; set; } = Array.Empty<KeywordConstraint>(); // siblings
	public SchemaConstraint[] SubschemaDependencies { get; set; } = Array.Empty<SchemaConstraint>(); // children

	public KeywordConstraint(string keyword, Action<KeywordEvaluation> evaluator)
	{
		Keyword = keyword;
		Evaluator = evaluator;
	}

	internal KeywordEvaluation BuildEvaluation(SchemaEvaluation schemaEvaluation, JsonPointer instanceLocation, JsonPointer evaluationPath)
	{
		var evaluation = new KeywordEvaluation(this, schemaEvaluation.LocalInstance, schemaEvaluation.Results);

		if (KeywordDependencies.Length != 0)
		{
			evaluation.KeywordEvaluations = new KeywordEvaluation[KeywordDependencies.Length];
			for (int i = 0; i < KeywordDependencies.Length; i++)
			{
				var dependency = schemaEvaluation.KeywordEvaluations
					.FirstOrDefault(x => x.Constraint.Keyword == KeywordDependencies[i].Keyword);
				evaluation.KeywordEvaluations[i] = dependency ?? KeywordEvaluation.Skip;
			}
		}
		else
			evaluation.KeywordEvaluations = Array.Empty<KeywordEvaluation>();

		if (SubschemaDependencies.Length != 0)
		{
			evaluation.SubschemaEvaluations = new SchemaEvaluation[SubschemaDependencies.Length];
			for (int i = 0; i < SubschemaDependencies.Length; i++)
			{
				var dependency = SubschemaDependencies[i];
				if (!dependency.InstanceLocation.TryEvaluate(schemaEvaluation.LocalInstance, out var instance))
				{
					evaluation.SubschemaEvaluations[i] = SchemaEvaluation.Skip;
					continue;
				}

				var localEvaluation = dependency.BuildEvaluation(instance, instanceLocation, evaluationPath);
				evaluation.SubschemaEvaluations[i] = localEvaluation;
				schemaEvaluation.Results.Details.Add(localEvaluation.Results);
			}
		}
		else
			evaluation.SubschemaEvaluations = Array.Empty<SchemaEvaluation>();

		return evaluation;
	}
}

public class KeywordEvaluation
{
	private bool _evaluated;

	internal static KeywordEvaluation Skip { get; } = new() { _evaluated = true };

	public KeywordConstraint Constraint { get; }
	public JsonNode? LocalInstance { get; }
	public EvaluationResults Results { get; }

	public KeywordEvaluation[] KeywordEvaluations { get; set; }
	public SchemaEvaluation[] SubschemaEvaluations { get; set; }

	internal KeywordEvaluation(KeywordConstraint constraint, JsonNode? localInstance, EvaluationResults results)
	{
		Constraint = constraint;
		LocalInstance = localInstance;
		Results = results;
	}
	private KeywordEvaluation(){}

	public void Evaluate()
	{
		if (_evaluated) return;

		//Results.Details.Add(Results);

		foreach (var evaluation in SubschemaEvaluations)
		{
			evaluation.Evaluate();
		}

		Constraint.Evaluator(this);

		_evaluated = true;
	}
}

public class ConstraintBuilderContext
{
	public SchemaRegistry SchemaRegistry { get; set; }
	internal Stack<(string, JsonPointer)> NavigatedReferences { get; } = new();

	internal ConstraintBuilderContext(){}
}