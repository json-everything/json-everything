using System.Collections.Generic;

namespace JsonPath
{
	public class JsonPathBuilder
	{
		private readonly List<PathNode> _nodes = new List<PathNode>();

		public JsonPath Build()
		{
			return new JsonPath(_nodes);
		}

		public void Add(PathNode node)
		{
			_nodes.Add(node);
		}

		public static implicit operator JsonPath(JsonPathBuilder builder)
		{
			return builder.Build();
		}
	}
}