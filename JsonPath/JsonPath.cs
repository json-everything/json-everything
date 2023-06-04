using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Path;

/// <summary>
/// Represents a JSON Path.
/// </summary>
[JsonConverter(typeof(JsonPathConverter))]
public class JsonPath
{
	/// <summary>
	/// Gets a JSON Path with only a global root and no selectors, namely `$`.
	/// </summary>
	public static JsonPath Root { get; } = new(PathScope.Global, Enumerable.Empty<PathSegment>());

	/// <summary>
	/// Gets the scope of the path.
	/// </summary>
	public PathScope Scope { get; }

	/// <summary>
	/// Gets whether the path is a singular path.  That is, it can only return a nodelist
	/// containing at most a single value.
	/// </summary>
	/// <remarks>
	/// A singular path can only contain segments which must meet all of the following
	/// conditions:
	///
	/// - is not a recursive descent (`..`)
	/// - contains a single selector
	/// - that selector is either an index selector or a name selector
	///
	/// For example, `$['foo'][1]` is a singular path.  Shorthand syntax (e.g. `$.foo[1]`)
	/// is also allowed.
	/// </remarks>
	public bool IsSingular => Segments.All(x => !x.IsRecursive &&
	                                            x.Selectors.Length == 1 &&
	                                            x.Selectors[0] is IndexSelector or NameSelector);

	/// <summary>
	/// Gets the segments of the path.
	/// </summary>
	public PathSegment[] Segments { get; }

	internal JsonPath(PathScope scope, IEnumerable<PathSegment> segments)
	{
		Scope = scope;
		Segments = segments.ToArray();
	}

	/// <summary>
	/// Parses a <see cref="JsonPath"/> from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="options">(optional) The parsing options.</param>
	/// <returns>The parsed path.</returns>
	/// <exception cref="PathParseException">Thrown if a syntax error occurred.</exception>
	public static JsonPath Parse(string source, PathParsingOptions? options = null)
	{
		options ??= new PathParsingOptions();

		if (options.TolerateExtraWhitespace)
			source = source.Trim();

		int index = 0;
		return PathParser.Parse(source, ref index, options, !options.AllowRelativePathStart);
	}

	/// <summary>
	/// Parses a <see cref="JsonPath"/> from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="options">(optional) The parsing options.</param>
	/// <param name="path">The parsed path, if successful; otherwise null.</param>
	/// <returns>True if successful; otherwise false.</returns>
	public static bool TryParse(string source, [NotNullWhen(true)] out JsonPath? path, PathParsingOptions? options = null)
	{
		options ??= new PathParsingOptions();

		if (!options.TolerateExtraWhitespace && (char.IsWhiteSpace(source[0]) || char.IsWhiteSpace(source[^1])))
		{
			path = null;
			return false;
		}

		source = source.Trim();

		int index = 0;
		if (!PathParser.TryParse(source, ref index, out path, options, true)) return false;
		if (index != source.Length)
		{
			path = null;
			return false;
		}

		return true;
	}

	/// <summary>
	/// Evaluates the path against a JSON instance.
	/// </summary>
	/// <param name="root">The root of the JSON instance.</param>
	/// <param name="options">Evaluation options.</param>
	/// <returns>The results of the evaluation.</returns>
	public PathResult Evaluate(JsonNode? root, PathEvaluationOptions? options = null)
	{
		IEnumerable<Node> currentMatches = new[] { new Node(root, Root) };

		foreach (var segment in Segments)
		{
			currentMatches = currentMatches.SelectMany(x => segment.Evaluate(x, root));
		}

		return new PathResult(new NodeList(currentMatches));
	}

	internal JsonPath Append(string name)
	{
		return new JsonPath(Scope, Segments.Append(new PathSegment(new NameSelector(name).Yield())));
	}

	internal JsonPath Append(int index)
	{
		return new JsonPath(Scope, Segments.Append(new PathSegment(new IndexSelector(index).Yield())));
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		var builder = new StringBuilder();

		BuildString(builder);

		return builder.ToString();
	}

	/// <summary>
	/// Builds a string representation of the path using a <see cref="StringBuilder"/>.
	/// </summary>
	/// <param name="builder">The string builder.</param>
	public void BuildString(StringBuilder builder)
	{
		builder.Append(Scope == PathScope.Global ? '$' : '@');

		foreach (var segment in Segments)
		{
			segment.BuildString(builder);
		}
	}
}