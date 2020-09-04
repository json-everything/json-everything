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