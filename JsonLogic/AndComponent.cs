using System.Text.Json;
using Json.More;

namespace Json.Logic
{
	internal class AndComponent : ILogicComponent
	{
		private readonly ILogicComponent _a;
		private readonly ILogicComponent _b;

		public AndComponent(ILogicComponent a, ILogicComponent b)
		{
			_a = a;
			_b = b;
		}

		public JsonElement Apply(JsonElement data)
		{
			return (_a.Apply(data).ValueKind == JsonValueKind.True &&
			        _b.Apply(data).ValueKind == JsonValueKind.True).AsJsonElement();
		}
	}
}