using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Json.Schema
{
	public class Format
	{
		public string Key { get; }

		internal bool IsUnknown => Key == null;

		internal Format(){}
		public Format(string key)
		{
			Key = key;
		}

		public virtual bool Validate(JsonElement element)
		{
			return true;
		}
	}

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

	public class PredicateFormat : Format
	{
		private readonly Func<JsonElement, bool> _predicate;

		public PredicateFormat(string key, Func<JsonElement, bool> predicate)
			: base(key)
		{
			_predicate = predicate;
		}

		public override bool Validate(JsonElement element)
		{
			return _predicate(element);
		}
	}
}