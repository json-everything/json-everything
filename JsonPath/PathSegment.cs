using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Represents a single path segment, generally indicated in the path by square brackets `[]`.
/// </summary>
public class PathSegment
{
	/// <summary>
	/// Gets the collection of selectors present in the path.
	/// </summary>
	public ISelector[] Selectors { get; }
	/// <summary>
	/// Indicates whether the segment is evaluated as a recursive descent operation.
	/// </summary>
	public bool IsRecursive { get; }
	/// <summary>
	/// Indicates whether the segment is represented in its shorthand form (e.g. `.foo` instead of `['foo']`).
	/// </summary>
	public bool IsShorthand { get; }

	internal PathSegment(IEnumerable<ISelector> selectors, bool isRecursive = false, bool isShorthand = false)
	{
		Selectors = selectors.ToArray();
		IsRecursive = isRecursive;
		IsShorthand = isShorthand;
	}

	internal IEnumerable<Node> Evaluate(Node match, JsonNode? rootNode)
	{
		return IsRecursive
			? Selectors.SelectMany(x => RecursivelyEvaluate(x, match, rootNode))
			: Selectors.SelectMany(x => x.Evaluate(match, rootNode));
	}

	private static IEnumerable<Node> RecursivelyEvaluate(ISelector selector, Node match, JsonNode? rootNode)
	{
		return GetAllDescendants(match).SelectMany(child => selector.Evaluate(child, rootNode));
	}

	private static IEnumerable<Node> GetAllDescendants(Node match)
	{
		yield return match;
		if (match.Value is JsonObject obj)
		{
			foreach (var member in obj)
			{
				var localMatch = new Node(member.Value, match.Location.Append(member.Key));
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
				var localMatch = new Node(member, match.Location.Append(i));
				foreach (var descendant in GetAllDescendants(localMatch))
				{
					yield return descendant;
				}
			}
		}
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
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

	/// <summary>
	/// Builds a string representation using a <see cref="StringBuilder"/>.
	/// </summary>
	/// <param name="builder">A string builder.</param>
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
		void AppendShorthand(StringBuilder builder) => ((IHaveShorthand)Selectors[0]).AppendShorthandString(builder);

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