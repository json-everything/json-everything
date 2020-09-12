using System.Collections.Generic;
using System.Text.Json;

namespace JsonPath
{
	public interface IObjectIndexExpression : IIndexExpression
	{
		IEnumerable<string> GetProperties(JsonElement obj);
	}
}