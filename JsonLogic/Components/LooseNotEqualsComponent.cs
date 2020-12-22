using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("!=")]
	internal class LooseNotEqualsComponent : LogicComponent
	{
		private readonly LogicComponent _a;
		private readonly LogicComponent _b;

		public LooseNotEqualsComponent(LogicComponent a, LogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var a = _a.Apply(data);
			var b = _b.Apply(data);

			return (!a.LooseEquals(b)).AsJsonElement();
		}
	}
}