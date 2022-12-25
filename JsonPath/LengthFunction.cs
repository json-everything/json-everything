using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Path;

public class LengthFunction : IPathFunctionDefinition
{
	public string Name => "length";
	public int MinArgumentCount => 1;
	public int MaxArgumentCount => 1;

	public NodeList Evaluate(IEnumerable<NodeList> arguments)
	{
		var node = arguments.Single().TryGetSingleValue();

		return node switch
		{
			JsonObject obj => (JsonValue)obj.Count,
			JsonArray arr => (JsonValue)arr.Count,
			JsonValue val when val.TryGetValue(out string? s) => (JsonValue)s.Length,
			JsonValue val when val.TryGetValue(out NodeList? l) => (JsonValue)l.Count,
			_ => NodeList.Empty
		};
	}
}