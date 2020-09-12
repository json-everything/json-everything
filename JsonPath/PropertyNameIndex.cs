using System;
using System.Collections.Generic;
using System.Text.Json;

namespace JsonPath
{
	public class PropertyNameIndex : IObjectIndexExpression
	{
		private readonly string _name;
		
		public PropertyNameIndex(string name)
		{
			_name = name;
		}

		public IEnumerable<string> GetProperties(JsonElement obj)
		{
			throw new NotImplementedException();
		}

		public static bool TryParse(ReadOnlySpan<char> span, ref int i, out IIndexExpression index)
		{
			index = null;
			i++;
			return false;
		}
	}
}