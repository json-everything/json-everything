using System.Collections.Generic;
using System.Text.Json;

namespace Json.Path
{
	internal interface IArrayIndexExpression : IIndexExpression
	{
		IEnumerable<int> GetIndices(JsonElement array);
	}
}