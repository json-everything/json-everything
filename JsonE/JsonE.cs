using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE;

/// <summary>
/// Models a JSON-e template.
/// </summary>
public static class JsonE
{
	/// <summary>
	/// Evaluates the template against a JSON value context.
	/// </summary>
	/// <param name="template"></param>
	/// <param name="context">The JSON value context</param>
	/// <returns>A new JSON value result.</returns>
	public static JsonNode? Evaluate(JsonNode? template, JsonNode? context)
	{
		context.ValidateAsContext();
		var evalContext = new EvaluationContext(context);
		
		var result = Evaluate(template, evalContext);
		return ReferenceEquals(result, IfThenElseOperator.DeleteMarker) ? null : result;
	}

	internal static JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var op = OperatorRepository.Get(template);

		if (op == null)
			return MaybeEvaluateChildren(template, context);
		
		return op.Evaluate(template, context);
	}

	private static JsonNode? MaybeEvaluateChildren(JsonNode? node, EvaluationContext context)
	{
		switch (node)
		{
			case JsonObject obj:
			{
				var result = new JsonObject();
				foreach (var kvp in obj)
				{
					var local = Evaluate(kvp.Value, context);
					if (!ReferenceEquals(local, IfThenElseOperator.DeleteMarker))
						result[kvp.Key] = local.Copy();
				}

				return result;
			}
			case JsonArray arr:
			{
				var result = new JsonArray();
				foreach (var item in arr)
				{
					var local = Evaluate(item, context);
					if (!ReferenceEquals(local, IfThenElseOperator.DeleteMarker))
						result.Add(local.Copy());
				}

				return result;
			}
			default:
				return node;
		}
	}
}
