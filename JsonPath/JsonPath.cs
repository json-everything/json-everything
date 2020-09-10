using System;
using System.Collections.Generic;
using System.Text.Json;

namespace JsonPath
{
	public class JsonPath
	{
		private readonly IEnumerable<IPathNode> _nodes;

		internal JsonPath(IEnumerable<IPathNode> nodes)
		{
			_nodes = nodes;
		}

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
					'.' => AddPropertyOrRecursive(span, ref i),
					'[' => AddIndex(span, ref i),
					_ => null
				};

				if (node == null)
					throw new PathParseException(i, "Could not identify operator.");

				nodes.Add(node);
			}

			return new JsonPath(nodes);
		}

		public static bool TryParse(string source, out JsonPath path)
		{
			var i = 0;
			var span = source.AsSpan();
			var nodes = new List<IPathNode>();
			while (i < span.Length)
			{
				var node = span[i] switch
				{
					'$' => AddRootNode(span, ref i),
					'.' => AddPropertyOrRecursive(span, ref i),
					'[' => AddIndex(span, ref i),
					_ => null
				};

				if (node == null)
				{
					path = null;
					return false;
				}

				nodes.Add(node);
			}

			path = new JsonPath(nodes);
			return true;
		}

		private static IPathNode AddRootNode(ReadOnlySpan<char> span, ref int i)
		{
			i++;
			return new RootNode();
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

			var propertyNameLength = slice.IndexOfAny('.', '[');
			if (propertyNameLength == 0)
				propertyNameLength = slice.Length - 1;

			var propertyName = slice.Slice(1, propertyNameLength);
			i += 1 + propertyNameLength;
			return new PropertyNode(propertyName.ToString());
		}

		private static IPathNode AddIndex(ReadOnlySpan<char> span, ref int i)
		{
			var openCount = 1;
			var length = 0;
			while (openCount != 0 && i + length < span.Length)
			{
				if (span[i + length] == '[') openCount++;
				if (span[i + length] == ']') openCount--;
				length++;
			}

			if (openCount != 0) return null;

			var newSpan = span.Slice(i, length - 1);
			var ranges = new List<IIndexExpression>();
			var index = 0;
			//while (RangeIndex.TryParse(newSpan, ref index, out var range))
			//{
			//	ranges.Add(range);
			//}

			i += length;
			return new IndexNode(ranges);
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