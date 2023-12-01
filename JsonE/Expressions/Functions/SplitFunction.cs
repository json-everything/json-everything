using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Expressions.Functions;

internal class SplitFunction : FunctionDefinition
{
	private const string _name = "split";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonValue val1 || !val1.TryGetValue(out string? source))
			throw new BuiltInException(CommonErrors.IncorrectArgType(_name));
		if (arguments[1] is not JsonValue val2)
			throw new BuiltInException(CommonErrors.IncorrectArgType(_name));
		string? split = null;
		var num = val2.GetNumber();
		if (num.HasValue)
			split = num.ToString();
		else if (!val2.TryGetValue(out split))
			throw new BuiltInException(CommonErrors.IncorrectArgType(_name));

		if (split == string.Empty)
			return source.Select(x => (JsonNode?)x).ToJsonArray();

		return source.Split(new []{split!}, StringSplitOptions.None).Select(x => (JsonNode?)x).ToJsonArray();
	}
}