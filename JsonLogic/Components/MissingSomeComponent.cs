using System.Text.Json;

namespace Json.Logic.Components
{
	[Operator("missing_some")]
	internal class MissingSomeComponent : LogicComponent
	{
		public override JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}