using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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
		ValidateContext(context);
		var evalContext = new EvaluationContext(context);
		
		return Evaluate(template, evalContext);
	}

	private static void ValidateContext(JsonNode? context)
	{
		if (context is not JsonObject obj)
			throw new TemplateException("context must be an object");

		if (obj.Any(x => !Regex.IsMatch(x.Key, "^[a-zA-Z_][a-zA-Z0-9_]*$")))
			throw new TemplateException("top level keys of context must follow /[a-zA-Z_][a-zA-Z0-9_]*/");
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
					result[kvp.Key] = Evaluate(kvp.Value, context);
				}

				return result;
			}
			case JsonArray arr:
			{
				var result = new JsonArray();
				foreach (var item in arr)
				{
					result.Add(Evaluate(item, context));
				}

				return result;
			}
			default:
				return node.Copy();
		}
	}
}
