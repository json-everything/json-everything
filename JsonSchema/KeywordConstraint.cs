using System;
using System.Collections.Generic;
using System.Linq;
using Json.Pointer;

namespace Json.Schema;

public class KeywordConstraint
{
	public static KeywordConstraint Skip { get; } = new(string.Empty, _ => { });

	public string? Keyword { get; }
	public Action<KeywordEvaluation> Evaluator { get; }

	public KeywordConstraint[] SiblingDependencies { get; set; } = Array.Empty<KeywordConstraint>();
	public SchemaConstraint[] ChildDependencies { get; set; } = Array.Empty<SchemaConstraint>();

	public KeywordConstraint(string keyword, Action<KeywordEvaluation> evaluator)
	{
		Keyword = keyword;
		Evaluator = evaluator;
	}

	internal KeywordEvaluation BuildEvaluation(SchemaEvaluation schemaEvaluation, JsonPointer instanceLocation, JsonPointer evaluationPath)
	{
		var evaluation = new KeywordEvaluation(this, schemaEvaluation.LocalInstance, schemaEvaluation.Results);

		if (SiblingDependencies.Length != 0)
		{
			evaluation.SiblingEvaluations = new KeywordEvaluation[SiblingDependencies.Length];
			for (int i = 0; i < SiblingDependencies.Length; i++)
			{
				var dependency = schemaEvaluation.KeywordEvaluations
					.FirstOrDefault(x => x.Constraint.Keyword == SiblingDependencies[i].Keyword);
				evaluation.SiblingEvaluations[i] = dependency ?? KeywordEvaluation.Skip;
			}
		}
		else
			evaluation.SiblingEvaluations = Array.Empty<KeywordEvaluation>();

		if (ChildDependencies.Length != 0)
		{
			var subschemaEvaluations = new List<SchemaEvaluation>();
			foreach (var dependency in ChildDependencies)
			{
				if (dependency.InstanceLocationGenerator != null)
				{
					var relativeInstanceLocations = dependency.InstanceLocationGenerator(evaluation).ToArray();
					foreach (var relativeInstanceLocation in relativeInstanceLocations)
					{
						if (!relativeInstanceLocation.TryEvaluate(schemaEvaluation.LocalInstance, out var instance)) continue;

						var templatedInstanceLocation = instanceLocation.Combine(relativeInstanceLocation);
						var localEvaluation = dependency.BuildEvaluation(instance, templatedInstanceLocation, evaluationPath);
						localEvaluation.RelativeInstanceLocation = relativeInstanceLocation;
						subschemaEvaluations.Add(localEvaluation);
						schemaEvaluation.Results.Details.Add(localEvaluation.Results);
					}
				}
				else
				{
					if (!dependency.RelativeInstanceLocation.TryEvaluate(schemaEvaluation.LocalInstance, out var instance)) continue;

					var localEvaluation = dependency.BuildEvaluation(instance, instanceLocation, evaluationPath);
					subschemaEvaluations.Add(localEvaluation);
					schemaEvaluation.Results.Details.Add(localEvaluation.Results);
				}
			}
			evaluation.ChildEvaluations = subschemaEvaluations.ToArray();
		}
		else
			evaluation.ChildEvaluations = Array.Empty<SchemaEvaluation>();

		return evaluation;
	}
}