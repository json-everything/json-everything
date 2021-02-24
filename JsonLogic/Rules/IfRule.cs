using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Rules
{
	[Operator("if")]
	[Operator("?:")]
	internal class IfRule : Rule
	{
		private readonly List<Rule> _components;

		public IfRule(params Rule[] components)
		{
			_components = new List<Rule>(components);
		}
	
		public override JsonElement Apply(JsonElement data)
		{
			bool condition;
			switch (_components.Count)
			{
				case 0:
					return ((string?) null).AsJsonElement();
				case 1:
					return _components[0].Apply(data);
				case 2:
					condition = _components[0].Apply(data).IsTruthy();
					var thenResult = _components[1];

					return condition
						? thenResult.Apply(data)
						: ((string?) null).AsJsonElement();
				default:
					var currentCondition = _components[0];
					var currentTrueResult = _components[1];
					var elseIndex = 2;

					while (currentCondition != null)
					{
						condition = currentCondition.Apply(data).IsTruthy();

						if (condition)
							return currentTrueResult.Apply(data);

						if (elseIndex == _components.Count) return ((string?) null).AsJsonElement();

						currentCondition = _components[elseIndex++];

						if (elseIndex >= _components.Count)
							return currentCondition.Apply(data);

						currentTrueResult = _components[elseIndex++];
					}
					break;
			}

			throw new NotImplementedException("Something went wrong. This shouldn't happen.");
		}
	}
}