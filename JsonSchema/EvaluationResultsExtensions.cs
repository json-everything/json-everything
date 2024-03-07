using System.Collections.Generic;

namespace Json.Schema;

public static class EvaluationResultsExtensions
{
	private static readonly IRootCauseAnalyzer[] _rootCauseAnalyzers =
	[
		new AllOfResultsAnalyzer(),
		new AnyOfResultsAnalyzer(),
	];

	public static IEnumerable<EvaluationResults> IdentifyRootErrors(this EvaluationResults root)
	{
		var queue = new Queue<EvaluationResults>();
		queue.Enqueue(root);

		while (queue.Count > 0)
		{
			var currentResult = queue.Dequeue();
			if (!currentResult.HasDetails) yield return currentResult;

			foreach (var rootCauseAnalyzer in _rootCauseAnalyzers)
			{
				var locals = rootCauseAnalyzer.FindIssues(currentResult);

				foreach (var local in locals)
				{
					queue.Enqueue(local);
				}
			}
		}
	}
}