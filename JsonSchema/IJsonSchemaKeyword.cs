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
	KeywordConstraint GetConstraint(JsonPointer evaluationPath,
		Uri schemaLocation,
		JsonPointer instanceLocation,
		IEnumerable<KeywordConstraint> localConstraints);
}

public class SchemaConstraint
{
	public JsonPointer EvaluationPath { get; set; }
	public Uri SchemaLocation { get; set; }
	public JsonSchema LocalSchema { get; set; }
	public JsonPointer InstanceLocation { get; set; }
	public KeywordConstraint[] Constraints { get; set; } = Array.Empty<KeywordConstraint>();

	public SchemaEvaluation BuildEvaluation(JsonNode? localInstance)
	{
		var evaluation = new SchemaEvaluation
		{
			Constraint = this,
			LocalInstance = localInstance,
			Results = new EvaluationResults(EvaluationPath, SchemaLocation, InstanceLocation),
			KeywordEvaluations = Constraints.Length == 0
				? Array.Empty<KeywordEvaluation>()
				: new KeywordEvaluation[Constraints.Length]
		};

		if (LocalSchema.BoolValue.HasValue)
		{
			if (!LocalSchema.BoolValue.Value)
				evaluation.Results.Fail();

			return evaluation;
		}

		for (int i = 0; i < Constraints.Length; i++)
		{
			evaluation.KeywordEvaluations[i] = Constraints[i].BuildEvaluation(evaluation);
		}

		return evaluation;
	}
}

public class SchemaEvaluation
{
	public SchemaConstraint Constraint { get; set; }
	public JsonNode? LocalInstance { get; set; }
	public EvaluationResults Results { get; set; }

	public KeywordEvaluation[] KeywordEvaluations { get; set; }

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
	public string? Keyword { get; set; }

	public KeywordConstraint[] KeywordDependencies { get; set; } = Array.Empty<KeywordConstraint>(); // siblings
	public SchemaConstraint[] SubschemaDependencies { get; set; } = Array.Empty<SchemaConstraint>(); // children

	public Action<KeywordEvaluation> Evaluator { get; set; }

	public KeywordEvaluation BuildEvaluation(SchemaEvaluation schemaEvaluation)
	{
		var evaluation = new KeywordEvaluation
		{
			Constraint = this,
			LocalInstance = schemaEvaluation.LocalInstance,
			Results = schemaEvaluation.Results
		};

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
				// TODO: local instance probably needs to update
				var localEvaluation = SubschemaDependencies[i].BuildEvaluation(schemaEvaluation.LocalInstance);
				evaluation.SubschemaEvaluations[i] = localEvaluation;
				schemaEvaluation.Results.Details.Add(localEvaluation.Results);
			}
		}
		else
			evaluation.SubschemaEvaluations = Array.Empty<SchemaEvaluation>();

		return evaluation;
	}

#pragma warning disable IDE0060
	public static void NoEvaluation(KeywordEvaluation evaluation)
	{
	}
#pragma warning restore IDE0060
}

public class KeywordEvaluation
{
	public static KeywordEvaluation Skip { get; } = new() { _evaluated = true };

	private bool _evaluated;

	public KeywordConstraint Constraint { get; set; }
	public JsonNode? LocalInstance { get; set; }
	public EvaluationResults Results { get; set; }

	public KeywordEvaluation[] KeywordEvaluations { get; set; }
	public SchemaEvaluation[] SubschemaEvaluations { get; set; }

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
