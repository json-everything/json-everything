using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;
using Json.Path.Expressions;

namespace Json.Path;

internal class FilterSelector : ISelector
{
	public BooleanResultExpressionNode Expression { get; }

	public FilterSelector(BooleanResultExpressionNode expression)
	{
		Expression = expression;
	}

	public IEnumerable<Node> Evaluate(Node match, JsonNode? rootNode)
	{
		var node = match.Value;
		if (node is JsonObject obj)
		{
			foreach (var member in obj)
			{
				if (Expression.Evaluate(rootNode, member.Value))
					yield return new Node(member.Value, match.Location.Append(member.Key));
			}
		}
		else if (node is JsonArray arr)
		{
			for (var i = 0; i < arr.Count; i++)
			{
				var member = arr[(Index)i];
				if (Expression.Evaluate(rootNode, member))
					yield return new Node(member, match.Location.Append(i));
			}
		}
	}

	public void BuildString(StringBuilder builder)
	{
		builder.Append('?');
		Expression.BuildString(builder);
	}

	public override string ToString()
	{
		return $"?{Expression}";
	}
}

internal class FilterSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ISelector? selector)
	{
		if (source[index] != '?')
		{
			selector = null;
			return false;
		}

		index++; // consume ?
		if (!ExpressionParser.TryParse(source, ref index, out var expression))
		{
			selector = null;
			return false;
		}

		selector = new FilterSelector(expression);
		return true;
	}
}