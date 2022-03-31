using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Json.Schema;

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
	/// <returns><code>true</code> if the value is a match for the regular expression; <code>false</code> otherwise.</returns>
	[Obsolete("This method isn't called anymore.  It will be removed in the next major version.")]
	public override bool Validate(JsonElement element)
	{
		return Validate(element, out _);
	}

	/// <summary>
	/// Validates an instance against a format and provides an error message.
	/// </summary>
	/// <param name="element">The element to validate.</param>
	/// <param name="errorMessage">An error message.</param>
	/// <returns><code>true</code> if the value is a match for the regular expression; <code>false</code> otherwise.</returns>
	public override bool Validate(JsonElement element, out string? errorMessage)
	{
		if (element.ValueKind != JsonValueKind.String)
		{
			errorMessage = null;
			return true;
		}

		var str = element.GetString();
		var matches = _regex.Match(str);

		if (matches.Value != str)
		{
			errorMessage = $"Value is not a match for the format '{Key}'.";
			return false;
		}

		errorMessage = null;
		return matches.Value == str;
	}
}