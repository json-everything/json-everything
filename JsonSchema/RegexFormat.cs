using System.Text.Json;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Json.Schema
{
	/// <summary>
	/// A regular-expression-based format.
	/// </summary>
	public class RegexFormat : Format
	{
		private readonly Regex _regex;

		/// <summary>
		/// Creates a new <see cref="RegexFormat"/>.
		/// </summary>
		/// <param name="key">The format key.</param>
		/// <param name="regex">The regular expression.</param>
		public RegexFormat(string key, [RegexPattern] string regex)
			: base(key)
		{
			_regex = new Regex(regex, RegexOptions.ECMAScript | RegexOptions.Compiled);
		}

		/// <summary>
		/// Validates an instance against a format.
		/// </summary>
		/// <param name="element">The element to validate.</param>
		/// <returns><code>true</code>.  Override to return another value.</returns>
		public override bool Validate(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			var str = element.GetString();
			var matches = _regex.Match(str);

			return matches.Value == str;
		}
	}
}