using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Logic.Rules;

[Operator("if")]
[Operator("?:")]
internal class IfRule : Rule
{
	private readonly List<Rule> _components;

	public IfRule(params Rule[] components)
	{
		_components = new List<Rule>(components);
	}

	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		bool condition;
		switch (_components.Count)
		{
			case 0:
				return null;
			case 1:
				return _components[0].Apply(data, contextData);
			case 2:
				condition = _components[0].Apply(data, contextData).IsTruthy();
				var thenResult = _components[1];

				return condition
					? thenResult.Apply(data, contextData)
					: null;
			default:
				var currentCondition = _components[0];
				var currentTrueResult = _components[1];
				var elseIndex = 2;

				while (currentCondition != null)
				{
					condition = currentCondition.Apply(data, contextData).IsTruthy();

					if (condition)
						return currentTrueResult.Apply(data, contextData);

					if (elseIndex == _components.Count) return null;

					currentCondition = _components[elseIndex++];

					if (elseIndex >= _components.Count)
						return currentCondition.Apply(data, contextData);

					currentTrueResult = _components[elseIndex++];
				}
				break;
		}

		throw new NotImplementedException("Something went wrong. This shouldn't happen.");
	}
}