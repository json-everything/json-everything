using System.Text.Json;

namespace Json.Logic.Components
{
	[Operator("!==")]
	internal class StrictNotEqualsComponent : LogicComponent
	{
		public override JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}