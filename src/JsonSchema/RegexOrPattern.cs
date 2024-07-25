using System;
using System.Text.RegularExpressions;

namespace Json.Schema;

/// <summary>
/// a union type based on either a Regex instance or a Pattern
/// </summary>
internal readonly struct RegexOrPattern
{
	private readonly string _pattern;
	private readonly Regex _regex;

	public RegexOrPattern(string pattern)
	{
		_pattern = pattern?.Replace("{Letter}", "{L}").Replace("{digit}", "{Nd}") ?? throw new ArgumentNullException(nameof(pattern));
	}

	public RegexOrPattern(Regex regex)
	{
		if (regex == null) throw new ArgumentNullException(nameof(regex));
		_pattern = regex.ToString();
		_regex = regex;
	}

	/// <summary>
	/// Determines is the string matches the specified pattern.
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public bool IsMatch(string str)
	{
		return _regex?.IsMatch(str) ?? Regex.IsMatch(str, _pattern, RegexOptions.Compiled | RegexOptions.ECMAScript, TimeSpan.FromSeconds(5));
	}

	public static implicit operator string(RegexOrPattern regexOrPattern)
	{
		return regexOrPattern.ToString();
	}

	public static implicit operator RegexOrPattern(string pattern)
	{
		return new RegexOrPattern(pattern);
	}

	public static implicit operator RegexOrPattern(Regex regex)
	{
		return new RegexOrPattern(regex);
	}

	public override bool Equals(object? obj)
	{
		return obj is RegexOrPattern other && Equals(other);
	}

	public bool Equals(RegexOrPattern other)
	{
		return string.Equals(_pattern, other._pattern, StringComparison.Ordinal);
	}

	public override int GetHashCode()
	{
		return _pattern.GetHashCode();
	}

	/// <summary>
	/// Will return the original Regex that created this instance, or it will return a new Regex based on the pattern.
	/// </summary>
	/// <returns></returns>
	public Regex ToRegex() => _regex ?? new Regex(_pattern, RegexOptions.ECMAScript, TimeSpan.FromSeconds(5));

	/// <summary>
	/// Returns the regex pattern
	/// </summary>
	/// <returns></returns>
	public override string ToString()
	{
		return _pattern;
	}
}