using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Path
{
	/// <summary>
	/// Represents a JSON Path.
	/// </summary>
	public class JsonPath
	{
		private delegate bool TryParseMethod(ReadOnlySpan<char> span, ref int i, out IIndexExpression index);

		private static readonly List<TryParseMethod> _parseMethods =
			new List<TryParseMethod>
			{
				ContainerQueryIndex.TryParse,
				ItemQueryIndex.TryParse,
				PropertyNameIndex.TryParse,
				SliceIndex.TryParse,
				SimpleIndex.TryParse
			};
		private static readonly Dictionary<string, IPathNode> _reservedWords =
			new Dictionary<string, IPathNode>
			{
				["length"] = LengthNode.Instance
			};

		private readonly IEnumerable<IPathNode> _nodes;

		private JsonPath(IEnumerable<IPathNode> nodes)
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
			var nodes = new List<IPathNode>();
			while (i < span.Length)
			{
				var node = span[i] switch
				{
					'$' => AddRootNode(span, ref i),
					'@' => AddLocalRootNode(span, ref i),
					'.' => AddPropertyOrRecursive(span, ref i),
					'[' => AddIndex(span, ref i),
					_ => null
				};

				if (node == null)
					throw new PathParseException(i, "Could not identify selector");

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
		public static bool TryParse(string source, out JsonPath path)
		{
			var i = 0;
			var span = source.AsSpan();
			return TryParse(span, ref i, false, out path);
		}

		internal static bool TryParse(ReadOnlySpan<char> span, ref int i, bool allowTrailingContent, out JsonPath path)
		{
			var nodes = new List<IPathNode>();
			while (i < span.Length)
			{
				var node = span[i] switch
				{
					'$' => AddRootNode(span, ref i),
					'@' => AddLocalRootNode(span, ref i),
					'.' => AddPropertyOrRecursive(span, ref i),
					'[' => AddIndex(span, ref i),
					_ => null
				};

				if (node == null)
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

		private static IPathNode AddRootNode(ReadOnlySpan<char> span, ref int i)
		{
			i++;
			return new RootNode();
		}

		private static IPathNode AddLocalRootNode(ReadOnlySpan<char> span, ref int i)
		{
			i++;
			return new LocalRootNode();
		}

		private static IPathNode AddPropertyOrRecursive(ReadOnlySpan<char> span, ref int i)
		{
			var slice = span.Slice(i);
			if (slice.StartsWith("..") || slice.StartsWith(".["))
			{
				i++;
				return new RecursiveNode();
			}

			if (slice.StartsWith(".*"))
			{
				i += 2;
				return new PropertyNode(null);
			}

			slice = slice.Slice(1);
			var propertyNameLength = 0;
			while (propertyNameLength < slice.Length && IsValidForPropertyName(slice[propertyNameLength]))
			{
				propertyNameLength++;
			}

			var propertyName = slice.Slice(0, propertyNameLength);
			i += 1 + propertyNameLength;
			return _reservedWords.TryGetValue(propertyName.ToString(), out var node)
				? node
				: new PropertyNode(propertyName.ToString());
		}

		private static bool IsValidForPropertyName(char ch)
		{
			return ch.In('a'..'z') ||
			       ch.In('A'..'Z') ||
			       ch.In('0'..'9') ||
			       ch.In('_') ||
			       ch.In(0x80..0x10FFFF);
		}

		private static IPathNode AddIndex(ReadOnlySpan<char> span, ref int i)
		{
			var slice = span.Slice(i);
			// replace this with an actual index parser that returns null to handle spaces
			if (slice.StartsWith("[*]"))
			{
				i += 3;
				return new IndexNode(null);
			}

			// consume [
			i++;
			var ch = ',';
			var indices = new List<IIndexExpression>();
			while (ch == ',')
			{
				span.ConsumeWhitespace(ref i);
				if (!ParseIndex(span, ref i, out var index)) return null;

				indices.Add(index);

				span.ConsumeWhitespace(ref i);
				if (i >= span.Length) break;

				ch = span[i];
				i++;
			}

			if (ch != ']') return null;
			
			return new IndexNode(indices);
		}

		private static bool ParseIndex(ReadOnlySpan<char> span, ref int i, out IIndexExpression index)
		{
			foreach (var tryParse in _parseMethods)
			{
				var j = i;
				if (tryParse(span, ref j, out index))
				{
					i = j;
					return true;
				}
			}

			index = null;
			return false;
		}

		/// <summary>
		/// Evaluates the path against a JSON instance.
		/// </summary>
		/// <param name="root">The root of the JSON instance.</param>
		/// <returns>The results of the evaluation.</returns>
		public PathResult Evaluate(in JsonElement root)
		{
			var context = new EvaluationContext(root);

			foreach (var node in _nodes)
			{
				node.Evaluate(context);
			}

			return context.BuildResult();
		}

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return string.Concat(_nodes.Select(n => n.ToString()));
		}
	}
}
