using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

public class KeywordConstraint
{
	public static KeywordConstraint Skip { get; } = new(string.Empty, _ => { });

	public string? Keyword { get; }
	public Action<KeywordEvaluation> Evaluator { get; }

	public KeywordConstraint[] SiblingDependencies { get; set; } = Array.Empty<KeywordConstraint>();
	public SchemaConstraint[] ChildDependencies { get; set; } = Array.Empty<SchemaConstraint>();

	internal Guid Id { get; } = Guid.NewGuid();

	public KeywordConstraint(string keyword, Action<KeywordEvaluation> evaluator)
	{
		Keyword = keyword;
		Evaluator = evaluator;
	}

	internal KeywordEvaluation BuildEvaluation(SchemaEvaluation schemaEvaluation, JsonPointer instanceLocation, JsonPointer evaluationPath)
	{
		var evaluation = new KeywordEvaluation(this, schemaEvaluation.LocalInstance, schemaEvaluation.Results) { Id = Id };

		if (SiblingDependencies.Length != 0)
		{
			evaluation.SiblingEvaluations = new KeywordEvaluation[SiblingDependencies.Length];
			for (int i = 0; i < SiblingDependencies.Length; i++)
			{
				var dependency = schemaEvaluation.FindEvaluation(SiblingDependencies[i].Id);
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
				if (dependency.InstanceLocator != null)
				{
					var relativeInstanceLocations = dependency.InstanceLocator(evaluation).ToArray();
					foreach (var relativeInstanceLocation in relativeInstanceLocations)
					{
						JsonNode? instance;
						if (dependency.UseLocatorAsInstance)
							instance = relativeInstanceLocation.Segments[0].Value;
						else if (!relativeInstanceLocation.TryEvaluate(schemaEvaluation.LocalInstance, out instance)) continue;

						var templatedInstanceLocation = instanceLocation.Combine(relativeInstanceLocation);
						var localEvaluation = dependency.BuildEvaluation(instance, templatedInstanceLocation, evaluationPath);
						localEvaluation.RelativeInstanceLocation = relativeInstanceLocation;
						subschemaEvaluations.Add(localEvaluation);
					}
				}
				else
				{
					if (!dependency.RelativeInstanceLocation.TryEvaluate(schemaEvaluation.LocalInstance, out var instance)) continue;

					var localEvaluation = dependency.BuildEvaluation(instance, instanceLocation, evaluationPath);
					subschemaEvaluations.Add(localEvaluation);
				}
			}
			evaluation.ChildEvaluations = subschemaEvaluations.ToArray();
		}
		else
			evaluation.ChildEvaluations = Array.Empty<SchemaEvaluation>();

		return evaluation;
	}
}