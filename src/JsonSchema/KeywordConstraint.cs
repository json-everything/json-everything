using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Describes the constraint applied by a single keyword.
/// </summary>
/// <remarks>
/// This represents any work that can be performed as part of static analysis of the schema.
/// </remarks>
public class KeywordConstraint
{
	private readonly Guid _id = Guid.NewGuid();

	/// <summary>
	/// Gets a no-op constraint.  Use for keywords that have no assertion or annotation behavior, e.g. `$defs`.
	/// </summary>
	public static KeywordConstraint Skip { get; } = new(string.Empty, (_, _) => { });

	/// <summary>
	/// The keyword name.
	/// </summary>
	public string? Keyword { get; }
	/// <summary>
	/// A method that used to apply the actual constraint behavior.
	/// </summary>
	/// <remarks>
	/// This method takes a <see cref="KeywordEvaluation"/> which contains the local instance being evaluated
	/// and the local results object.
	/// </remarks>
	public Action<KeywordEvaluation, EvaluationContext> Evaluator { get; }

	/// <summary>
	/// Gets or sets the collection of keyword constraints (i.e. sibling keywords) that this keyword is dependent upon.
	/// The evaluations of these constraints will be available when this keyword is evaluated.
	/// </summary>
	public KeywordConstraint[] SiblingDependencies { get; set; } = [];
	/// <summary>
	/// Gets or sets the collection of schema constraints (i.e. subschemas) that this keyword is dependent upon.
	/// The evaluations of these constraints will be available when this keyword is evaluated.
	/// </summary>
	public SchemaConstraint[] ChildDependencies { get; set; } = [];

	/// <summary>
	/// Creates a new keyword constraint.
	/// </summary>
	/// <param name="keyword">The keyword name.</param>
	/// <param name="evaluator">A method that used to apply the actual constraint behavior.</param>
	public KeywordConstraint(string keyword, Action<KeywordEvaluation, EvaluationContext> evaluator)
	{
		Keyword = keyword;
		Evaluator = evaluator;
	}

	/// <summary>
	/// Creates a keyword constraint that simply applies an annotation.
	/// </summary>
	/// <param name="keyword">The keyword name.</param>
	/// <param name="value">The annotation value.</param>
	/// <returns>A new keyword constraint.</returns>
	public static KeywordConstraint SimpleAnnotation(string keyword, JsonNode? value)
	{
		return new KeywordConstraint(keyword, (e, _) => e.Results.SetAnnotation(keyword, value));
	}

	internal KeywordEvaluation BuildEvaluation(SchemaEvaluation schemaEvaluation, JsonPointer_Old instanceLocation, JsonPointer_Old evaluationPath)
	{
		var evaluation = new KeywordEvaluation(this, schemaEvaluation.LocalInstance, schemaEvaluation.Results) { Id = _id };

		if (SiblingDependencies.Length != 0)
		{
			evaluation.SiblingEvaluations = new KeywordEvaluation[SiblingDependencies.Length];
			for (int i = 0; i < SiblingDependencies.Length; i++)
			{
				var dependency = schemaEvaluation.FindEvaluation(SiblingDependencies[i]._id);
				evaluation.SiblingEvaluations[i] = dependency ?? KeywordEvaluation.Skip;
			}
		}
		else
			evaluation.SiblingEvaluations = [];

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
							instance = relativeInstanceLocation[0];
						else if (!relativeInstanceLocation.TryEvaluate(schemaEvaluation.LocalInstance, out instance)) continue;

						var templatedInstanceLocation = instanceLocation.Combine(relativeInstanceLocation);
						var localEvaluation = dependency.BuildEvaluation(instance, templatedInstanceLocation, evaluationPath, schemaEvaluation.Options);
						localEvaluation.RelativeInstanceLocation = relativeInstanceLocation;
						subschemaEvaluations.Add(localEvaluation);
					}
				}
				else
				{
					if (!dependency.RelativeInstanceLocation.TryEvaluate(schemaEvaluation.LocalInstance, out var instance)) continue;

					var localEvaluation = dependency.BuildEvaluation(instance, instanceLocation, evaluationPath, schemaEvaluation.Options);
					subschemaEvaluations.Add(localEvaluation);
				}
			}
			evaluation.ChildEvaluations = [..subschemaEvaluations];
		}
		else
			evaluation.ChildEvaluations = [];

		return evaluation;
	}
}