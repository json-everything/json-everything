using System.Globalization;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	[Operator("cat")]
	internal class CatComponent : LogicComponent
	{
		private readonly LogicComponent _a;
		private readonly LogicComponent _b;

		public CatComponent(LogicComponent a, LogicComponent b)
		{
			_a = a;
			_b = b;
		}
		
		public override JsonElement Apply(JsonElement data)
		{
			var a = _a.Apply(data);
			var b = _b.Apply(data);

			var stringA = a.Stringify();
			var stringB = b.Stringify();

			if (stringA == null || stringB == null)
				throw new JsonLogicException($"Cannot concatenate types {a.ValueKind} and {b.ValueKind}.");

			return (stringA + stringB).AsJsonElement();
		}
	}
}