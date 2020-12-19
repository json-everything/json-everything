using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Json.Logic.Components
{
	internal class IfComponent : LogicComponent
	{
		private readonly LogicComponent _condition;
		private readonly LogicComponent _trueResult;
		private readonly List<LogicComponent> _falseResult;

		public IfComponent(LogicComponent condition, LogicComponent trueResult, LogicComponent falseResult, params LogicComponent[] additional)
		{
			if (additional.Length % 2 != 0)
				throw new ArgumentException("Additional arguments must come in pairs", nameof(additional));
			
			_condition = condition;
			_trueResult = trueResult;
			_falseResult = new List<LogicComponent>{falseResult};
			_falseResult.AddRange(additional);
		}
	
		public override JsonElement Apply(JsonElement data)
		{
			var currentCondition = _condition;
			var currentTrueResult = _trueResult;
			var elseIndex = 0;

			while (currentCondition != null)
			{
				var condition = currentCondition.Apply(data).IsTruthy();

				if (condition)
					return currentTrueResult.Apply(data);

				currentCondition = _falseResult[elseIndex++];

				if (elseIndex >= _falseResult.Count)
					return currentCondition.Apply(data);
				
				currentTrueResult = _falseResult[elseIndex++];
			}

			throw new NotImplementedException("Something went wrong. This shouldn't happen.");
		}
	}
}