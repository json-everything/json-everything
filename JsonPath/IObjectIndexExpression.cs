using System.Collections.Generic;
using System.Text.Json;

namespace Json.Path
{
	public interface IObjectIndexExpression : IIndexExpression
	{
		IEnumerable<string> GetProperties(JsonElement obj);
	}
}