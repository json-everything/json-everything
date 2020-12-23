using System.Text.Json;

namespace Json.Logic.Rules
{
	[Operator("log")]
	internal class LogRule : Rule
	{
		public override JsonElement Apply(JsonElement data)
		{
			throw new System.NotImplementedException();
		}
	}
}