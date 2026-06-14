using System;
using System.Text.RegularExpressions;

namespace Json.Schema;

internal readonly struct RegexOrPattern : IEquatable<RegexOrPattern>
{
	private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

	private readonly string _pattern;
	private readonly Regex? _regex;

	private static string NormalizePattern(string pattern)
	{
		return pattern.Replace("{Letter}", "{L}").Replace("{digit}", "{Nd}");
	}

	private RegexOrPattern(string pattern)
	{
		_pattern = NormalizePattern(pattern ?? throw new ArgumentNullException(nameof(pattern)));
		_ = CreateRegex(_pattern, false);
	}

	private RegexOrPattern(Regex regex)
	{
		_pattern = regex?.ToString() ?? throw new ArgumentNullException(nameof(regex));
		_regex = regex;
	}

	private static bool RequiresUnicodeMode(string pattern)
	{
		for (int i = 0; i < pattern.Length - 1; i++)
		{
			if (pattern[i] != '\\') continue;

			var next = pattern[i + 1];
			if ((next == 'p' || next == 'P') && i + 2 < pattern.Length && pattern[i + 2] == '{') return true;
			if (next == 'u' && i + 2 < pattern.Length && pattern[i + 2] == '{') return true;

			i++;
		}

		return false;
	}

	private static RegexOptions GetOptions(string pattern, bool compiled)
	{
		var options = RequiresUnicodeMode(pattern) ? RegexOptions.None : RegexOptions.ECMAScript;
		if (compiled) options |= RegexOptions.Compiled;
		return options;
	}

	internal static Regex CreateRegex(string pattern, bool compiled)
	{
		var normalizedPattern = NormalizePattern(pattern);
		var options = GetOptions(normalizedPattern, compiled);
		return new Regex(normalizedPattern, options, _timeout);
	}

	public bool IsMatch(string str) => _regex?.IsMatch(str) ??
	                                   Regex.IsMatch(str, _pattern, GetOptions(_pattern, true), _timeout);

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