using System.Collections.Generic;
using System.Text.Json;

namespace Json.Path
{
	internal interface IObjectIndexExpression : IIndexExpression
	{
		IEnumerable<string> GetProperties(JsonElement obj);
	}
}