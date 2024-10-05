using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Expressions.Functions;

internal class RangeFunction : FunctionDefinition
{
	private const string _name = "range";

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		// we store decimals, but we need to ensure they're also integer values for this function
		if (arguments[0] is not JsonValue valStart || !valStart.TryGetValue(out decimal? start) ||
		    (int)start.Value != start.Value)
			throw new InterpreterException(CommonErrors.IncorrectArgType(_name));
		if (arguments[1] is not JsonValue valEnd || !valEnd.TryGetValue(out decimal? end) ||
		    (int)end.Value != end.Value)
			throw new InterpreterException(CommonErrors.IncorrectArgType(_name));
		decimal? step = null;
		if (arguments.Length > 2 && (arguments[2] is not JsonValue valStep || !valStep.TryGetValue(out step) ||
		                             (int)step.Value != step.Value || step.Value == 0))
			throw new InterpreterException(CommonErrors.IncorrectArgType(_name));
		step ??= 1;
		var direction = Math.Sign(step.Value);

		var values = new List<decimal>();
		for (var i = start.Value * direction; i < end * direction; i += step.Value * direction)
		{
			values.Add(i * direction);
		}

		return values.Select(x => (JsonNode)x).ToJsonArray();
	}
}