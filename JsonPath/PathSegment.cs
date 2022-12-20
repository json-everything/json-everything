using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

public class PathSegment
{
	public ISelector[] Selectors { get; }
	public bool IsRecursive { get; }
	public bool IsShorthand { get; }

	internal PathSegment(IEnumerable<ISelector> selectors, bool isRecursive = false, bool isShorthand = false)
	{
		Selectors = selectors.ToArray();
		IsRecursive = isRecursive;
		IsShorthand = isShorthand;
	}

	internal IEnumerable<PathMatch> Evaluate(PathMatch match, JsonNode? rootNode)
	{
		return IsRecursive
			? Selectors.SelectMany(x => RecursivelyEvaluate(x, match, rootNode))
			: Selectors.SelectMany(x => x.Evaluate(match, rootNode));
	}

	private static IEnumerable<PathMatch> RecursivelyEvaluate(ISelector selector, PathMatch match, JsonNode? rootNode)
	{
		var allDescendants = GetAllDescendants(match);
		return allDescendants.SelectMany(child => selector.Evaluate(child, rootNode));
	}

	private static IEnumerable<PathMatch> GetAllDescendants(PathMatch match)
	{
		yield return match;
		if (match.Value is JsonObject obj)
		{
			foreach (var member in obj)
			{
				var localMatch = new PathMatch(member.Value, match.Location.Append(member.Key));
				foreach (var descendant in GetAllDescendants(localMatch))
				{
					yield return descendant;
				}
			}
		}
		else if (match.Value is JsonArray arr)
		{
			for (var i = 0; i < arr.Count; i++)
			{
				var member = arr[i];
				var localMatch = new PathMatch(member, match.Location.Append(i));
				foreach (var descendant in GetAllDescendants(localMatch))
				{
					yield return descendant;
				}
			}
		}
	}

	public override string ToString()
	{
		string GetNormalized() => $"[{string.Join(',', Selectors.Select(x => x.ToString()))}]";
		string GetShorthand() => $"{((IHaveShorthand)Selectors[0]).ToShorthandString()}";

		if (IsRecursive)
		{
			if (IsShorthand)
				return "." + GetShorthand();

			return ".." + GetNormalized();
		}

		if (IsShorthand)
			return $"{((IHaveShorthand)Selectors[0]).ToShorthandString()}";
		
		return $"[{string.Join(',', Selectors.Select(x => x.ToString()))}]";
	}

	public void BuildString(StringBuilder builder)
	{
		void AppendNormalized()
		{
			builder.Append('[');

			Selectors[0].BuildString(builder);

			for (int i = 1; i < Selectors.Length; i++)
			{
				builder.Append(',');
				Selectors[i].BuildString(builder);
			}

			builder.Append(']');
		}
		void AppendShorthand(StringBuilder builder) => builder.Append(((IHaveShorthand)Selectors[0]).ToShorthandString());

		if (IsRecursive)
		{
			builder.Append('.');
			if (IsShorthand)
			{
				AppendShorthand(builder);
				return;
			}

			builder.Append('.');
			AppendNormalized();
			return;
		}


		if (IsShorthand)
		{
			AppendShorthand(builder);
			return;
		}

		AppendNormalized();

	}


}