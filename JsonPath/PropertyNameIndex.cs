using System;
using System.Collections.Generic;
using System.Text.Json;

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

		internal static bool TryParse(ReadOnlySpan<char> span, ref int i, out IIndexExpression index)
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
					// TODO: process escape sequences
					length+=2;
					continue;
				}
				if (span[i + length] == start) break;
				length++;
			}

			var name = span.Slice(i, length);
			i += length + 1;
			JsonElement element;
			try
			{
				element = JsonDocument.Parse($"\"{name.ToString()}\"").RootElement;
			}
			catch
			{
				index = null;
				return false;
			}
			index = new PropertyNameIndex(element.GetString(), start);
			return true;
		}

		public override string ToString()
		{
			// TODO: add escaping
			return $"{_quoteChar}{_name}{_quoteChar}";
		}
	}
}