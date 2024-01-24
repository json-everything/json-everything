using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.JsonE.Expressions;
using Json.More;

namespace Json.JsonE.Operators;

internal partial class SortOperator : IOperator
{
#if NETSTANDARD2_0
	private static readonly Regex _byForm = new(@"^by\(\s*(?<var>[a-zA-Z_][a-zA-Z0-9_]*)\s*\)");
#else
	[GeneratedRegex(@"^by\(\s*(?<var>[a-zA-Z_][a-zA-Z0-9_]*)\s*\)")]
	private static partial Regex MyRegex();
	private static readonly Regex _byForm = MyRegex();
#endif
	public const string Name = "$sort";

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		obj.VerifyNoUndefinedProperties(Name, _byForm);

		if (obj.Count > 2)
			throw new TemplateException("Expected no more than two keys");

		var parameter = obj[Name];
		if (!parameter.IsTemplateOr<JsonArray>())
			throw new TemplateException(CommonErrors.SortSameType());

		var value = JsonE.Evaluate(parameter, context)!.AsArray();
		if (value.Count == 0) return value;

		var sortExpression = ExpressionParser.Parse("x".AsSpan());
		var variableName = "x";

		var accessorEntry = obj.FirstOrDefault(x => x.Key != Name);
		if (accessorEntry.Key != null)
		{
			sortExpression = ExpressionParser.Parse(accessorEntry.Value!.GetValue<string>().AsSpan());
			variableName = _byForm.Match(accessorEntry.Key).Groups["var"].Value;
		}

		var itemContext = new JsonObject
		{
			[variableName] = value[0].Copy()
		};
		context.Push(itemContext);
		var firstSortValue = sortExpression.Evaluate(context);
		var comparer = firstSortValue switch
		{
			JsonValue v when v.TryGetValue<char>(out _) => JsonNodeCharComparer.Instance,
			JsonValue v when v.TryGetValue<string>(out _) => JsonNodeStringComparer.Instance,
			JsonValue v when v.GetNumber() != null => JsonNodeNumberComparer.Instance,
			_ => (IComparer<JsonNode>?)null
		} ?? throw new TemplateException(CommonErrors.SortSameType());

		try
		{
			var sorted = value.OrderBy(x =>
			{
				itemContext[variableName] = x.Copy();
				return sortExpression.Evaluate(context);
			}, comparer!).ToJsonArray();

			context.Pop();
			return sorted;
		}
		catch (InvalidOperationException e)
		{
			// .OrderBy() seems to have its own try/catch which wraps any exceptions.
			// I hate doing this, but I really want the exception thrown by the comparer.
			throw e.InnerException!;
		}
	}
}

internal class JsonNodeCharComparer : IComparer<JsonNode>
{
	public static JsonNodeCharComparer Instance { get; } = new();

	private JsonNodeCharComparer(){}

	public int Compare(JsonNode? x, JsonNode? y)
	{
		var sX = (x as JsonValue)?.GetValue<char>() ?? throw new TemplateException(CommonErrors.SortSameType());
		var sY = (y as JsonValue)?.GetValue<char>() ?? throw new TemplateException(CommonErrors.SortSameType());

		return sX < sY ? -1 : 1;
	}
}

internal class JsonNodeStringComparer : IComparer<JsonNode>
{
	public static JsonNodeStringComparer Instance { get; } = new();

	private JsonNodeStringComparer(){}

	public int Compare(JsonNode? x, JsonNode? y)
	{
		var sX = (x as JsonValue)?.GetValue<string>() ?? throw new TemplateException(CommonErrors.SortSameType());
		var sY = (y as JsonValue)?.GetValue<string>() ?? throw new TemplateException(CommonErrors.SortSameType());

		return string.CompareOrdinal(sX, sY);
	}
}

internal class JsonNodeNumberComparer : IComparer<JsonNode>
{
	public static JsonNodeNumberComparer Instance { get; } = new();

	private JsonNodeNumberComparer(){}

	public int Compare(JsonNode? x, JsonNode? y)
	{
		var nX = (x as JsonValue)?.GetNumber() ?? throw new TemplateException(CommonErrors.SortSameType());
		var nY = (y as JsonValue)?.GetNumber() ?? throw new TemplateException(CommonErrors.SortSameType());

		return nX <= nY ? -1 : 1;
	}
}