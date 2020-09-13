using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Json.Path
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
			return new[] {_name};
		}

		public static bool TryParse(ReadOnlySpan<char> span, ref int i, out IIndexExpression index)
		{
			if (span[i] != '\'' && span[i] != '"')
			{
				index = null;
				return false;
			}

			var start = span[i];
			i++;
			var length = 0;
			while (i + length < span.Length)
			{
				if (span[i + length] == '\\')
				{
					// ignoring escape sequences for now.
					length+=2;
					continue;
				}
				if (span[i + length] == start) break;
				length++;
			}

			var name = span.Slice(i, length);
			i += length + 1;
			index = new PropertyNameIndex(name.ToString());
			return true;
		}
	}
}