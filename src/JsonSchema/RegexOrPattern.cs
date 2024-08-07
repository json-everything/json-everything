using System;
using System.Text.RegularExpressions;

namespace Json.Schema;

internal readonly struct RegexOrPattern : IEquatable<RegexOrPattern>
{
	private readonly string _pattern;
	private readonly Regex? _regex;

	public RegexOrPattern(string pattern)
	{
		_pattern = pattern.Replace("{Letter}", "{L}").Replace("{digit}", "{Nd}") ?? throw new ArgumentNullException(nameof(pattern));

		_ = Regex.IsMatch("", _pattern); // Validates the pattern and also caches it.
	}

	public RegexOrPattern(Regex regex)
	{
		_pattern = regex?.ToString() ?? throw new ArgumentNullException(nameof(regex));
		_regex = regex;
	}

	public bool IsMatch(string str) => _regex?.IsMatch(str) ??
	                                   Regex.IsMatch(str, _pattern, RegexOptions.Compiled | RegexOptions.ECMAScript, TimeSpan.FromSeconds(5));

	public Regex ToRegex() => _regex ?? new Regex(_pattern, RegexOptions.ECMAScript, TimeSpan.FromSeconds(5));

	public static implicit operator string(RegexOrPattern regexOrPattern)
	{
		return regexOrPattern.ToString();
	}

	public static implicit operator RegexOrPattern(string pattern) => new(pattern);

	public static implicit operator RegexOrPattern(Regex regex) => new(regex);

	public override bool Equals(object? obj) => obj is RegexOrPattern other && Equals(other);

	public bool Equals(RegexOrPattern other) => string.Equals(_pattern, other._pattern, StringComparison.Ordinal);

	public override int GetHashCode() => _pattern.GetHashCode();

	public override string ToString() => _pattern;
}