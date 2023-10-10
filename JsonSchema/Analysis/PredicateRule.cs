using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Analysis;

public class PredicateRule : IRule
{
	public Func<JsonNode, IEnumerable<Diagnostic>> Runner { get; set; }

	public IEnumerable<Diagnostic> Run(JsonNode schema)
	{
		return Runner(schema);
	}
}