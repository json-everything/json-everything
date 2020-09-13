using System.Collections.Generic;

namespace Json.Path
{
	public class JsonPathBuilder
	{
		private readonly List<IPathNode> _nodes = new List<IPathNode>();

		public JsonPath Build()
		{
			return new JsonPath(_nodes);
		}

		public void Add(IPathNode node)
		{
			_nodes.Add(node);
		}

		public static implicit operator JsonPath(JsonPathBuilder builder)
		{
			return builder.Build();
		}
	}
}