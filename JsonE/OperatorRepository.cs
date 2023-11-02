using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.JsonE.Operators;

namespace Json.JsonE;

internal static class OperatorRepository
{
	private static readonly Dictionary<string, IOperator> _operators = new()
	{
		["$eval"] = new EvalOperator(),
		[FlattenOperator.Name] = new FlattenOperator(),
	};

	public static (IOperator?, JsonNode?) Get(JsonNode? node)
	{
		if (node is not JsonObject obj) return (null, node);

		var operatorKeys = obj.Select(x => x.Key).Intersect(_operators.Keys).ToArray();
		var op = operatorKeys.Length switch
		{
			> 1 => throw new TemplateException("only one operator allowed"),
			// TODO: check if `<identifier>` should be replaced by the key
			0 => HasReservedWords(obj)
				? throw new TemplateException("$<identifier> is reserved; use $$<identifier>")
				: null,
			_ => _operators[operatorKeys[0]]
		};

		if (op is null) return (null, node);

		var value = obj[operatorKeys[0]];
		var newTemplate = JsonETemplate.CreateInternal(value);
		obj[operatorKeys[0]] = newTemplate;

		op.Validate(obj);
		
		return (op, obj);
	}

	private static bool HasReservedWords(JsonObject obj)
	{
		return obj.Any(x => Regex.IsMatch(x.Key, @"^\$[^$]"));
	}
}