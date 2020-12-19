using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("or")]
	internal class OrComponent : LogicComponent
	{
		private readonly LogicComponent _a;
		private readonly LogicComponent _b;

		public OrComponent(LogicComponent a, LogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public override JsonElement Apply(JsonElement data)
		{
			return (_a.Apply(data).IsTruthy() ||
			        _b.Apply(data).IsTruthy()).AsJsonElement();
		}
	}
}