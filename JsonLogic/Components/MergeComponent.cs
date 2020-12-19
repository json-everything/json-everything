using System.Text.Json;

namespace Json.Logic.Components
{
	[Operator("merge")]
	internal class MergeComponent : LogicComponent
	{
		public override JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}