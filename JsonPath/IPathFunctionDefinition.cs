using System.Collections.Generic;

namespace Json.Path;

public interface IPathFunctionDefinition
{
	string Name { get; }
	int MinArgumentCount { get; }
	int MaxArgumentCount { get; }

	NodeList Evaluate(IEnumerable<NodeList> arguments);
}