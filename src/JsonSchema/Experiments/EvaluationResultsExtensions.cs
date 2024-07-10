using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public static class EvaluationResultsExtensions
{
	public static IEnumerable<T> GetAllAnnotations<T>(this IEnumerable<EvaluationResults> results, string keyword)
		where T : JsonNode
	{
		var toCheck = new Queue<EvaluationResults>(results);

		while (toCheck.Any())
		{
			var current = toCheck.Dequeue();
			if (!current.Valid) continue;

			if (current.Annotations is not null)
			{
				if (current.Annotations.TryGetValue(keyword, out var node) && node is T annotation)
					yield return annotation;
			}

			if (current.Details is not null)
			{
				foreach (var detail in current.Details)
				{
					toCheck.Enqueue(detail);
				}
			}
		}
	}
}