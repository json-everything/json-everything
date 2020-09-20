using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Path
{
	public class JsonPath
	{
		private delegate bool TryParseMethod(ReadOnlySpan<char> span, ref int i, out IIndexExpression index);

		private static readonly List<TryParseMethod> _parseMethods =
			new List<TryParseMethod>
			{
				RangeIndex.TryParse,
				SimpleIndex.TryParse,
				PropertyNameIndex.TryParse
			};

		private readonly IEnumerable<IPathNode> _nodes;

		private JsonPath(IEnumerable<IPathNode> nodes)
		{
			_nodes = nodes;
		}

		public static JsonPath Parse(string source)
		{
			return Parse(source, false);
		}

		internal static JsonPath Parse(string source, bool allowTrailingContent)
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
				{
					if (allowTrailingContent) break;
					throw new PathParseException(i, "Could not identify operator");
				}

				nodes.Add(node);
			}

			if (!nodes.Any())
				throw new PathParseException(i, "No path found");

			return new JsonPath(nodes);
		}

		public static bool TryParse(string source, out JsonPath path)
		{
			return TryParse(source, false, out path);
		}

		internal static bool TryParse(string source, bool allowTrailingContent, out JsonPath path)
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
			var propertyNameLength = slice.IndexOfAny('.', '[');
			if (propertyNameLength == -1)
				propertyNameLength = slice.Length;

			var propertyName = slice.Slice(0, propertyNameLength);
			i += 1 + propertyNameLength;
			return new PropertyNode(propertyName.ToString());
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

		public PathResult Evaluate(in JsonElement root)
		{
			var context = new EvaluationContext(root);

			foreach (var node in _nodes)
			{
				node.Evaluate(context);
			}

			return context.BuildResult();
		}
	}
}