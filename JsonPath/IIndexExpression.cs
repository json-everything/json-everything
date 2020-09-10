using System.Collections.Generic;
using System.Text.Json;

namespace JsonPath
{
	public interface IIndexExpression
	{
		IEnumerable<int> GetIndices(JsonElement array);
	}
}