using System;
using System.Collections.Generic;
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
	private readonly IEnumerable<PathSegment> _nodes;

	public PathScope Scope { get; }

	internal JsonPath(PathScope scope, IEnumerable<PathSegment> nodes)
	{
		Scope = scope;
		_nodes = nodes;
	}

	/// <summary>
	/// Parses a <see cref="JsonPath"/> from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <returns>The parsed path.</returns>
	/// <exception cref="PathParseException">Thrown if a syntax error occurred.</exception>
	public static JsonPath Parse(string source) => PathParser.Parse(source);

	/// <summary>
	/// Evaluates the path against a JSON instance.
	/// </summary>
	/// <param name="root">The root of the JSON instance.</param>
	/// <param name="options">Evaluation options.</param>
	/// <returns>The results of the evaluation.</returns>
	public PathResult Evaluate(JsonNode? root, PathEvaluationOptions? options = null)
	{
		options ??= new PathEvaluationOptions();

		throw new NotImplementedException();

		//var context = new EvaluationContext(root, options);

		//foreach (var node in _nodes)
		//{
		//	node.Evaluate(context);

		//	ReferenceHandler.Handle(context);
		//}

		//return context.BuildResult();
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		var builder = new StringBuilder();

		BuildString(builder);

		return builder.ToString();
	}

	public void BuildString(StringBuilder builder)
	{
		builder.Append(Scope == PathScope.Global ? '$' : '@');

		foreach (var node in _nodes)
		{
			node.BuildString(builder);
		}
	}
}

public class PathEvaluationOptions
{
	
}