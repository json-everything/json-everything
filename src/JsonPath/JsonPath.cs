using System;
using System.Collections.Generic;
using System.ComponentModel;
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
[TypeConverter(typeof(JsonPathTypeConverter))]
public class JsonPath : IEquatable<JsonPath>
{
	/// <summary>
	/// Gets a JSON Path with only a global root and no selectors, namely `$`.
	/// </summary>
	public static JsonPath Root { get; } = new(PathScope.Global, []);

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
	public static JsonPath Parse([StringSyntax("jsonpath")] string source, PathParsingOptions? options = null)
	{
		options ??= new PathParsingOptions();

		if (options.TolerateExtraWhitespace)
			source = source.Trim();

		int index = 0;
		return PathParser.Parse(source.AsSpan(), ref index, options, !options.AllowRelativePathStart);
	}

	/// <summary>
	/// Parses a <see cref="JsonPath"/> from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="options">(optional) The parsing options.</param>
	/// <param name="path">The parsed path, if successful; otherwise null.</param>
	/// <returns>True if successful; otherwise false.</returns>
	public static bool TryParse([StringSyntax("jsonpath")] string source, [NotNullWhen(true)] out JsonPath? path, PathParsingOptions? options = null)
	{
		if (source.Length == 0)
		{
			path = null;
			return false;
		}

		options ??= new PathParsingOptions();
		if (!options.TolerateExtraWhitespace && (char.IsWhiteSpace(source[0]) || char.IsWhiteSpace(source[^1])))
		{
			path = null;
			return false;
		}

		source = source.Trim();

		int index = 0;
		if (!PathParser.TryParse(source.AsSpan(), ref index, out path, options, !options.AllowRelativePathStart)) return false;
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
		IEnumerable<Node> currentMatches = [new(root, Root)];

		foreach (var segment in Segments)
		{
			currentMatches = currentMatches.SelectMany(x => segment.Evaluate(x, root));
		}

		return new PathResult(new NodeList(currentMatches));
	}

	internal PathResult Evaluate(JsonNode? globalRoot, JsonNode? localRoot)
	{
		IEnumerable<Node> currentMatches = [new(localRoot, Root)];

		foreach (var segment in Segments)
		{
			currentMatches = currentMatches.SelectMany(x => segment.Evaluate(x, globalRoot));
		}

		return new PathResult(new NodeList(currentMatches));
	}

	/// <summary>
	/// Appends a new segment with the specified name to the current JSON path and returns the resulting path.
	/// </summary>
	/// <remarks>This method can be used to build more complex JSON paths by successively appending named segments.
	/// The original JsonPath instance remains unchanged; this method returns a new instance with the additional
	/// segment.</remarks>
	/// <param name="name">The name of the segment to append to the JSON path. This value cannot be null or empty.</param>
	/// <returns>A new instance of the JsonPath class that includes the appended segment.</returns>
	public JsonPath Append(string name) => new(Scope, Segments.Append(new PathSegment(new NameSelector(name).Yield())));

	/// <summary>
	/// Appends an array index selector to the current JSON path, enabling access to a specific element within a JSON
	/// array.
	/// </summary>
	/// <remarks>Use this method to navigate to a particular element in a JSON array by its index. Supplying an
	/// out-of-range index may result in errors when the path is evaluated against a JSON document.</remarks>
	/// <param name="index">The zero-based index of the element to select from the array. Must be within the bounds of the target array.</param>
	/// <returns>A new JsonPath instance that includes the appended index selector.</returns>
	public JsonPath Append(int index) => new(Scope, Segments.Append(new PathSegment(new IndexSelector(index).Yield())));

	/// <summary>
	/// Appends an existing <see cref="PathSegment"/> to the current JSON path and returns the resulting path.
	/// </summary>
	/// <param name="segment">The segment to append.</param>
	/// <returns>A new <see cref="JsonPath"/> instance that includes the appended segment.</returns>
	public JsonPath Append(PathSegment segment)
	{
		if (segment is null) throw new ArgumentNullException(nameof(segment));
		return new(Scope, Segments.Append(segment));
	}

	/// <summary>
	/// Appends a sequence of <see cref="PathSegment"/> instances to the current JSON path and returns the resulting path.
	/// </summary>
	/// <param name="segments">The segments to append.</param>
	/// <returns>A new <see cref="JsonPath"/> instance that includes the appended segments.</returns>
	public JsonPath Append(IEnumerable<PathSegment> segments)
	{
		if (segments is null) throw new ArgumentNullException(nameof(segments));
		return new(Scope, Segments.Concat(segments));
	}

	/// <summary>
	/// Appends all segments of another <see cref="JsonPath"/> to the current JSON path and returns the resulting path.
	/// </summary>
	/// <param name="path">The path whose segments will be appended.</param>
	/// <returns>A new <see cref="JsonPath"/> instance that includes the appended path's segments.</returns>
	public JsonPath Append(JsonPath path)
	{
		if (path is null) throw new ArgumentNullException(nameof(path));
		return new(Scope, Segments.Concat(path.Segments));
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

	/// <summary>
	/// Indicates whether the current <see cref="JsonPath"/> is equal to another <see cref="JsonPath"/>.
	/// </summary>
	/// <param name="other">A <see cref="JsonPath"/> to compare with this instance.</param>
	/// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
	public bool Equals(JsonPath? other)
	{
		if (ReferenceEquals(this, other)) return true;
		if (other is null) return false;
		if (Scope != other.Scope || Segments.Length != other.Segments.Length) return false;

		for (var i = 0; i < Segments.Length; i++)
		{
			if (!Segments[i].Equals(other.Segments[i])) return false;
		}

		return true;
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
	public override bool Equals(object? obj) => Equals(obj as JsonPath);

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			var hash = (int)Scope;
			foreach (var segment in Segments)
			{
				hash = (hash * 397) ^ segment.GetHashCode();
			}
			return hash;
		}
	}

	/// <summary>
	/// Determines whether two <see cref="JsonPath"/> instances are equal.
	/// </summary>
	public static bool operator ==(JsonPath? left, JsonPath? right) => Equals(left, right);

	/// <summary>
	/// Determines whether two <see cref="JsonPath"/> instances are not equal.
	/// </summary>
	public static bool operator !=(JsonPath? left, JsonPath? right) => !Equals(left, right);
}