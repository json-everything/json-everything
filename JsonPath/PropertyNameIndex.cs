using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Path
{
	internal class PropertyNameIndex : IObjectIndexExpression
	{
		private readonly string _name;
		private readonly char _quoteChar;

		private PropertyNameIndex(string name, char quoteChar)
		{
			_name = name;
			_quoteChar = quoteChar;
		}

		IEnumerable<string> IObjectIndexExpression.GetProperties(JsonElement obj)
		{
			return new[] {_name};
		}

		internal static bool TryParse(ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out IIndexExpression? index)
		{
			if (span[i] != '\'' && span[i] != '"')
			{
				i = -1;
				index = null;
				return false;
			}

			var start = span[i];
			var other = start == '\'' ? '"' : '\'';
			i++;
			var length = 0;
			while (i + length < span.Length)
			{
				if (span[i + length] == '\\')
				{
					length+=2;
					continue;
				}
				if (span[i + length] == start) break;
				length++;
			}

			var name = span.Slice(i, length);
			i += length + 1;
			var key = name.ToString();
			// don't escape the other quote
			if (Regex.IsMatch(key, $@"(^|[^\\])\\{other}"))
			{
				index = null;
				return false;
			}
			try
			{
				if (start == '\'') 
					key = key.Replace("\\'", "'").Replace("\"", "\\\"");
				using var doc = JsonDocument.Parse($"\"{key}\"");
				key = doc.RootElement.GetString();
			}
			catch
			{
				index = null;
				return false;
			}
			index = new PropertyNameIndex(key, start);
			return true;
		}

		public override string ToString()
		{
			// TODO: add escaping
			return $"{_quoteChar}{_name}{_quoteChar}";
		}
	}
}