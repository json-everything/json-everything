using System.Text.Json;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Json.Schema
{
	public class RegexFormat : Format
	{
		private readonly Regex _regex;

		public RegexFormat(string key, [RegexPattern] string regex)
			: base(key)
		{
			_regex = new Regex(regex, RegexOptions.ECMAScript | RegexOptions.Compiled);
		}

		public override bool Validate(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			var str = element.GetString();
			var matches = _regex.Match(str);

			return matches.Value == str;
		}
	}
}