using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Analysis;

public interface IRule
{
	IEnumerable<Diagnostic> Run(JsonNode schema);
}
