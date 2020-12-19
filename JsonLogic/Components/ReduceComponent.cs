using System.Text.Json;

namespace Json.Logic.Components
{
	[Operator("reduce")]
	internal class ReduceComponent : LogicComponent
	{
		public override JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}