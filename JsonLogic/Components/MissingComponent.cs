using System.Text.Json;

namespace Json.Logic.Components
{
	internal class MissingComponent : LogicComponent
	{
		public override JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}