using System.Collections.Generic;
using System.Text.Json;

namespace Json.Path
{
	public interface IArrayIndexExpression : IIndexExpression
	{
		IEnumerable<int> GetIndices(JsonElement array);
	}
}