using System.Text.Json;

namespace Json.Logic.Components
{
	[Operator("!=")]
	internal class LooseNotEqualsComponent : LogicComponent
	{
		public override JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}