using System.Text.Json;

namespace Json.Logic.Components
{
	[Operator("filter")]
	internal class FilterComponent : LogicComponent
	{
		public override JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}