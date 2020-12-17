using System.Globalization;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic.Components
{
	internal class CatComponent : ILogicComponent
	{
		private readonly ILogicComponent _a;
		private readonly ILogicComponent _b;

		public CatComponent(ILogicComponent a, ILogicComponent b)
		{
			_a = a;
			_b = b;
		}
		
		public JsonElement Apply(JsonElement data)
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