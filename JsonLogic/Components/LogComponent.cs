using System.Text.Json;

namespace Json.Logic.Components
{
	[Operator("log")]
	internal class LogComponent : LogicComponent
	{
		public override JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}