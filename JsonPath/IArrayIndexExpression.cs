using System.Collections.Generic;
using System.Text.Json;

namespace JsonPath
{
	public interface IArrayIndexExpression : IIndexExpression
	{
		IEnumerable<int> GetIndices(JsonElement array);
	}
}