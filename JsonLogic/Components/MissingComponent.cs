using System.Text.Json;

namespace Json.Logic.Components
{
	internal class MissingComponent : ILogicComponent
	{
		public JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}