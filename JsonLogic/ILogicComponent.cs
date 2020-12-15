using System.Text.Json;

namespace Json.Logic
{
	public interface ILogicComponent
	{
		JsonElement Apply(JsonElement data);
	}
}
