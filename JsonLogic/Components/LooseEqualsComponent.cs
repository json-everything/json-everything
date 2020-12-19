using System.Text.Json;

namespace Json.Logic.Components
{
	[Operator("==")]
	internal class LooseEqualsComponent : LogicComponent
	{
		public override JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}