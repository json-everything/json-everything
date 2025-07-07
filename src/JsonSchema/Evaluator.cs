using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

public static class Evaluator
{

	public static EvaluationOutput Evaluate(JsonSchemaNode node, JsonElement instance)
	{
		return Evaluate(node, instance, JsonPointer.Empty, JsonPointer.Empty);
	}
    
	private static EvaluationOutput Evaluate(JsonSchemaNode node, JsonElement instance, JsonPointer currentInstanceLocation, JsonPointer currentEvaluationPath)
    {
        // Track which dependencies have been evaluated
        var evaluatedDependencies = new Dictionary<string, List<EvaluationOutput>>();
        var allResults = new List<EvaluationOutput>();
        var isValid = true;
        var errors = new Dictionary<string, string>();
        var annotations = new Dictionary<string, JsonElement>();

        // Evaluate all dependencies first
        foreach (var kvp in node.Dependencies)
        {
	        var keyword = kvp.Key;
	        var dependencies = kvp.Value;
            foreach (var dependency in dependencies)
            {
                EvaluateDependency(keyword, dependency, instance, currentInstanceLocation, currentEvaluationPath, allResults, evaluatedDependencies);
            }
        }

        // Now evaluate constraints as soon as their dependencies are available
        foreach (var kvp in node.Constraints)
        {
	        var keyword = kvp.Key;
	        var constraint = kvp.Value;
            var keywordDeps = evaluatedDependencies.GetValueOrDefault(keyword, []);
            var result = constraint(instance, keywordDeps);

            if (result.IsValid)
            {
                if (result.Annotation.HasValue)
                    annotations[keyword] = result.Annotation.Value;
            }
            else
            {
                isValid = false;
                if (result.ErrorMessage != null)
                {
                    var errorKeyword = result.KeywordOverride ?? keyword;
                    errors[errorKeyword] = result.ErrorMessage;
                }
            }
        }

        return new EvaluationOutput(
            isValid,
            currentInstanceLocation,
            currentEvaluationPath,
            node.SchemaIri,
            errors.Count > 0 ? errors : null,
            isValid ? annotations : null,
            allResults.Count > 0 ? allResults : null
        );
    }

    private static void EvaluateDependency(string keyword, JsonSchemaNode dependency, JsonElement instance, JsonPointer currentInstanceLocation, JsonPointer currentEvaluationPath, List<EvaluationOutput> allResults, Dictionary<string, List<EvaluationOutput>> evaluatedDependencies)
    {
        // Get all matching instance values for the dependency's path
        var matches = dependency.InstancePathFromParent.EvaluateWithWildcards(instance);
        if (matches.Length == 0) return;

        var dependencyResults = new List<EvaluationOutput>();
        foreach (var (matchValue, matchLocation) in matches)
        {
            // Apply the filter if it exists
            if (dependency.FilterDependencyLocations != null &&
                !dependency.FilterDependencyLocations(matchLocation, matchValue, instance))
                continue;

            // Build the full instance location for the dependency
            var dependencyInstanceLocation = currentInstanceLocation.Combine(matchLocation);
            // Build the evaluation path including the keyword and additional schema path
            var dependencyEvaluationPath =
                dependency.AdditionalSchemaPathFromParent.SegmentCount == 0
                    ? currentEvaluationPath.Combine(keyword)
                    : currentEvaluationPath.Combine(keyword).Combine(dependency.AdditionalSchemaPathFromParent);

            // Evaluate the dependency immediately
            var result = Evaluate(dependency, matchValue, dependencyInstanceLocation, dependencyEvaluationPath);
            dependencyResults.Add(result);
            allResults.Add(result);
        }

        if (dependencyResults.Count > 0) 
            evaluatedDependencies[keyword] = dependencyResults;
    }
}