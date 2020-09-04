using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace JsonPath
{
	public class JsonPath
	{
		private readonly IEnumerable<PathNode> _nodes;

		internal JsonPath(IEnumerable<PathNode> nodes)
		{
			_nodes = nodes;
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

	public abstract class PathNode
	{
		public virtual void Evaluate(EvaluationContext context)
		{
			var toProcess = new List<PathMatch>(context.Current);
			context.Current.Clear();
			foreach (var match in toProcess)
			{
				context.Current.AddRange(ProcessMatch(match));
			}
		}

		protected abstract IEnumerable<PathMatch> ProcessMatch(PathMatch match);
	}

	internal class PropertyNode : PathNode
	{
		public string Name { get; }

		public PropertyNode(string name)
		{
			Name = name;
		}

		protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
		{
			if (match.Value.ValueKind != JsonValueKind.Object) yield break;
			if (!match.Value.TryGetProperty(Name, out var prop)) yield break;

			yield return new PathMatch(prop, match.Location.Combine(PointerSegment.Create(Name)));
		}
	}

	internal class RangeIndexNode : PathNode
	{
		public IReadOnlyList<IndexOrRange> Ranges { get; }

		public RangeIndexNode(IEnumerable<IndexOrRange> ranges)
		{
			Ranges = ranges.ToList();
		}

		protected override IEnumerable<PathMatch> ProcessMatch(PathMatch match)
		{
			if (match.Value.ValueKind != JsonValueKind.Array) yield break;

			var array = match.Value.EnumerateArray().ToArray();
			foreach (var indexOrRange in Ranges)
			{
				if (indexOrRange.IsRange)
				{
					var start = indexOrRange.Range.Start.IsFromEnd
						? array.Length - indexOrRange.Range.Start.Value
						: indexOrRange.Range.Start.Value;
					var i = 0;
					foreach (var item in array[indexOrRange.Range])
					{
						yield return new PathMatch(item, match.Location.Combine(PointerSegment.Create($"{i + start}")));
					}
				}
				else
				{
					var index = indexOrRange.Index.IsFromEnd
						? array.Length - indexOrRange.Index.Value
						: indexOrRange.Index.Value;
					yield return new PathMatch(array[indexOrRange.Index], match.Location.Combine(PointerSegment.Create($"{index}")));
				}
			}
		}
	}

	public class EvaluationContext
	{
		public JsonElement Root { get; }
		public List<PathMatch> Current { get; }

		internal EvaluationContext(in JsonElement root)
		{
			Root = root.Clone();
			Current = new List<PathMatch>{new PathMatch(root, JsonPointer.Empty)};
		}

		internal PathResult BuildResult()
		{
			return new PathResult(Current);
		}
	}

	public static class JsonPathBuilderExtensions
	{
		public static JsonPathBuilder Property(this JsonPathBuilder builder, string prop)
		{
			builder.Add(new PropertyNode(prop));
			return builder;
		}

		public static JsonPathBuilder Index(this JsonPathBuilder builder, params IndexOrRange[] ranges)
		{
			builder.Add(new RangeIndexNode(ranges));
			return builder;
		}
	}
}
