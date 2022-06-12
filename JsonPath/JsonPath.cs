using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Path;

/// <summary>
/// Represents a JSON Path.
/// </summary>
[JsonConverter(typeof(JsonPathConverter))]
public class JsonPath
{
	private delegate bool TryParseMethod(ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out IIndexExpression? index);

	private static readonly List<TryParseMethod> _parseMethods =
		new()
		{
			ContainerQueryIndex.TryParse,
			ItemQueryIndex.TryParse,
			PropertyNameIndex.TryParse,
			SliceIndex.TryParse,
			SimpleIndex.TryParse
		};
	private static readonly Dictionary<string, ISelector> _reservedWords =
		new()
		{
			["length"] = LengthSelector.Instance
		};

	private readonly IEnumerable<ISelector> _nodes;

	internal static readonly JsonPath Root = new(new[] { new RootNodeSelector() });

	private JsonPath(IEnumerable<ISelector> nodes)
	{
		_nodes = nodes;
	}

	/// <summary>
	/// Parses a <see cref="JsonPath"/> from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <returns>The parsed path.</returns>
	/// <exception cref="PathParseException">Thrown if a syntax error occurred.</exception>
	public static JsonPath Parse(string source)
	{
		var i = 0;
		var span = source.AsSpan();
		var nodes = new List<ISelector>();
		while (i < span.Length)
		{
			var node = span[i] switch
			{
				'$' => AddRootNode(ref i),
				'@' => AddLocalRootNode(ref i),
				'.' => AddPropertyOrRecursive(span, ref i),
				'[' => AddIndex(span, ref i),
				_ => null
			};

			if (node == null)
				throw new PathParseException(i, "Could not identify selector");

			if (node is ErrorSelector error)
				throw new PathParseException(i, error.ErrorMessage);

			nodes.Add(node);
		}

		if (!nodes.Any())
			throw new PathParseException(i, "No path found");

		return new JsonPath(nodes);
	}

	/// <summary>
	/// Attempts to parse a <see cref="JsonPath"/> from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="path">The resulting path.</param>
	/// <returns><code>true</code> if successful; otherwise <code>false</code>.</returns>
	public static bool TryParse(string source, out JsonPath? path)
	{
		var i = 0;
		var span = source.AsSpan();
		return TryParse(span, ref i, false, out path);
	}

	internal static bool TryParse(ReadOnlySpan<char> span, ref int i, bool allowTrailingContent, [NotNullWhen(true)] out JsonPath? path)
	{
		var nodes = new List<ISelector>();
		while (i < span.Length)
		{
			var node = span[i] switch
			{
				'$' => AddRootNode(ref i),
				'@' => AddLocalRootNode(ref i),
				'.' => AddPropertyOrRecursive(span, ref i),
				'[' => AddIndex(span, ref i),
				_ => null
			};

			if (node is null or ErrorSelector)
			{
				if (allowTrailingContent) break;
				path = null;
				return false;
			}

			nodes.Add(node);
		}

		if (!nodes.Any())
		{
			path = null;
			return false;
		}

		path = new JsonPath(nodes);
		return true;
	}

	private static ISelector AddRootNode(ref int i)
	{
		i++;
		return new RootNodeSelector();
	}

	private static ISelector AddLocalRootNode(ref int i)
	{
		i++;
		return new LocalNodeSelector();
	}

	private static ISelector AddPropertyOrRecursive(ReadOnlySpan<char> span, ref int i)
	{
		var slice = span[i..];
		if (slice.StartsWith("..") || slice.StartsWith(".["))
		{
			if (slice.StartsWith("..["))
				// this handles cases like $..['foo-1'] that can't
				// be represented by a dot-name selector
				i += 2;
			else
				i++;
			return new RecursiveDescentSelector();
		}

		if (slice.StartsWith(".*"))
		{
			i += 2;
			return new PropertySelector(null);
		}

		slice = slice[1..];
		var propertyNameLength = 0;
		while (propertyNameLength < slice.Length && IsValidForPropertyName(slice[propertyNameLength]))
		{
			propertyNameLength++;
		}

		var propertyName = slice[..propertyNameLength];
		i += 1 + propertyNameLength;
		return _reservedWords.TryGetValue(propertyName.ToString(), out var node)
			? node
			: new PropertySelector(propertyName.ToString());
	}

	private static bool IsValidForPropertyName(char ch)
	{
		return ch.In('a'..('z' + 1)) ||
			   ch.In('A'..('Z' + 1)) ||
			   ch.In('0'..('9' + 1)) ||
			   ch.In('_') ||
			   ch.In(0x80..0x10FFFF);
	}

	private static ISelector AddIndex(ReadOnlySpan<char> span, ref int i)
	{
		var slice = span[i..];
		// replace this with an actual index parser that returns null to handle spaces
		if (slice.StartsWith("[*]"))
		{
			i += 3;
			return new IndexSelector(null);
		}

		// consume [
		i++;
		var ch = ',';
		var indices = new List<IIndexExpression>();
		while (ch == ',')
		{
			span.ConsumeWhitespace(ref i);
			if (!ParseIndex(span, ref i, out var index))
				return new ErrorSelector("Error parsing path index value");

			indices.Add(index!);

			span.ConsumeWhitespace(ref i);
			if (i >= span.Length) break;

			ch = span[i];
			i++;
		}

		if (ch != ']')
			return new ErrorSelector("Expected ']' or ','");

		return new IndexSelector(indices);
	}

	private static bool ParseIndex(ReadOnlySpan<char> span, ref int i, out IIndexExpression? index)
	{
		foreach (var tryParse in _parseMethods)
		{
			var j = i;
			if (tryParse(span, ref j, out index))
			{
				i = j;
				return true;
			}

			if (j != -1)
			{
				i = j;
				index = null;
				return false;
			}
		}

		index = null;
		return false;
	}

	/// <summary>
	/// Evaluates the path against a JSON instance.
	/// </summary>
	/// <param name="root">The root of the JSON instance.</param>
	/// <param name="options">Evaluation options.</param>
	/// <returns>The results of the evaluation.</returns>
	public PathResult Evaluate(JsonElement root, PathEvaluationOptions? options = null)
	{
		options ??= new PathEvaluationOptions();

		var context = new EvaluationContext(root, options);

		foreach (var node in _nodes)
		{
			node.Evaluate(context);

			ReferenceHandler.Handle(context);
		}

		return context.BuildResult();
	}

	internal JsonPath AddSelector(ISelector selector)
	{
		var newNodes = new List<ISelector>(_nodes) { selector };
		return new JsonPath(newNodes);
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return string.Concat(_nodes.Select(n => n.ToString()));
	}
}

/// <summary>
/// JSON converter for <see cref="JsonPath"/>.
/// </summary>
public class JsonPathConverter : JsonConverter<JsonPath>
{
	/// <summary>Reads and converts the JSON to type <see cref="JsonPath"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override JsonPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var text = reader.GetString()!;
		return JsonPath.Parse(text);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, JsonPath value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.ToString(), options);
	}
}