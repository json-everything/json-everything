using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE;

/// <summary>
/// Models a JSON-e template.
/// </summary>
public static class JsonE
{
	internal static readonly JsonNode DeleteMarker = "delete_marker"!;

	/// <summary>
	/// Evaluates the template against a JSON value context.
	/// </summary>
	/// <param name="template"></param>
	/// <param name="context">The JSON value context</param>
	/// <returns>A new JSON value result.</returns>
	public static JsonNode? Evaluate(JsonNode? template, JsonNode? context = null)
	{
		context ??= new JsonObject();

		context.ValidateAsContext();
		var evalContext = new EvaluationContext(context);
		
		var result = Evaluate(template, evalContext);
		result.ValidateNotReturningFunction();
		return ReferenceEquals(result, DeleteMarker) ? null : result;
	}

	internal static JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var op = OperatorRepository.Get(template);

		JsonNode? result;

		if (op == null)
			result = MaybeEvaluateChildren(template, context);
		else
			result = op.Evaluate(template, context);

		return HandleStringInterpolation(result, context);
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
					var local = kvp.Value is JsonValue val &&
					            val.TryGetValue(out JsonExpression? json)
						? json.Expression.Evaluate(context)
						: Evaluate(kvp.Value, context);
					if (!ReferenceEquals(local, DeleteMarker))
						result[HandleEscapedKey(kvp.Key)] = local.Copy();
				}

				return result;
			}
			case JsonArray arr:
			{
				var result = new JsonArray();
				foreach (var item in arr)
				{
					var local = Evaluate(item, context);
					if (!ReferenceEquals(local, DeleteMarker))
						result.Add(local.Copy());
				}

				return result;
			}
			default:
				return node;
		}
	}

	private static string HandleEscapedKey(string key)
	{
		if (key.StartsWith("$$")) return key[1..];
		return key;
	}

	private static JsonNode? HandleStringInterpolation(JsonNode? value, EvaluationContext context)
	{
		if (ReferenceEquals(value, DeleteMarker)) return value;

		switch (value)
		{
			case JsonObject obj:
				foreach (var kvp in obj.ToArray())
				{
					obj.Remove(kvp.Key);
					obj[Interpolate(kvp.Key, context)] = kvp.Value;
				}
				return obj;
			case JsonValue val when val.TryGetValue(out string? str):
				return Interpolate(str, context);
			default:
				return value;
		}
	}

	private static string Interpolate(string value, EvaluationContext context)
	{
		if (value.Length <= 2) return value;

		var interpolated = value;
		var starts = Enumerable.Range(0, value.Length - 2 + 1).Where(index => "${".Equals(value.Substring(index, 2)));

		var source = value.AsSpan();
		foreach (var start in starts)
		{
			char? stringStartChar = null;
			var end = start + 2;
			var nest = 1;
			while (nest != 0 && end < source.Length)
			{
				switch (source[end])
				{
					case '\'':
					case '"':
						if (!stringStartChar.HasValue)
							stringStartChar = source[end];
						else if (stringStartChar.Value == source[end])
							stringStartChar = null;
						break;
					case '{':
						if (!stringStartChar.HasValue)
							nest++;
						break;
					case '}':
						if (!stringStartChar.HasValue)
							nest--;
						break;
				}
				end++;
			}

			if (end > source.Length)
				throw new TemplateException("invalid expression inside string interpolation");

			var textToReplace = source[start..end].ToString();
			if (start != 0 && source[start - 1] == '$')
			{
				var unescaped = source[(start + 1)..end].ToString();
				interpolated = interpolated.Replace(textToReplace, unescaped);
				continue;
			}

			var exprText = source[(start + 2)..(end - 1)];
			var index = 0;
			if (!ExpressionParser.TryParse(exprText, ref index, out var expr))
				throw new TemplateException("invalid expression inside string interpolation");

			var evaluated = expr!.Evaluate(context);
			if (evaluated is null)
			{
				interpolated = interpolated.Replace(textToReplace, string.Empty);
				continue;
			}

			if (evaluated is JsonValue val)
			{
				var n = val.GetNumber();
				if (n.HasValue)
				{
					interpolated = interpolated.Replace(textToReplace, n.ToString());
					continue;
				}
				if (val.TryGetValue(out string? s))
				{
					interpolated = interpolated.Replace(textToReplace, s);
					continue;
				}
				if (val.TryGetValue(out bool b))
				{
					interpolated = interpolated.Replace(textToReplace, b ? "true" : "false");
					continue;
				}
			}

			throw new TemplateException($"interpolation of '{exprText.ToString()}' produced an array or object");
		}

		return interpolated;
	}

}
