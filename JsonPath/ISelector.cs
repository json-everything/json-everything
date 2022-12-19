using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

public interface ISelector
{
	IEnumerable<PathMatch> Evaluate(JsonNode? node);
	void BuildString(StringBuilder builder);
}