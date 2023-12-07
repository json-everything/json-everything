using System.Linq;
using System.Text.Json.Nodes;
using Json.More;
using Json.Path;

namespace Json.Schema.Data;

public class JsonPathIdentifier : IDataResourceIdentifier
{
	public JsonPath Query { get; }

	public JsonPathIdentifier(JsonPath query)
	{
		Query = query;
	}

	public bool TryResolve(KeywordEvaluation evaluation, SchemaRegistry registry, out JsonNode? value)
	{
		var results = Query.Evaluate(evaluation.LocalInstance);
		if (results.Error == null)
		{
			value = null;
			return false;
		}

		value = results.Matches!.Select(x => x.Value).ToJsonArray();
		return true;
	}
}