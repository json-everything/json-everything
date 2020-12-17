using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Json.Logic.Components
{
	internal class IfComponent : ILogicComponent
	{
		private readonly ILogicComponent _condition;
		private readonly ILogicComponent _trueResult;
		private readonly List<ILogicComponent> _falseResult;

		public IfComponent(ILogicComponent condition, ILogicComponent trueResult, ILogicComponent falseResult, params ILogicComponent[] additional)
		{
			if (additional.Length % 2 != 0)
				throw new ArgumentException("Additional arguments must come in pairs", nameof(additional));
			
			_condition = condition;
			_trueResult = trueResult;
			_falseResult = new List<ILogicComponent>{falseResult};
			_falseResult.AddRange(additional);
		}
	
		public JsonElement Apply(JsonElement data)
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