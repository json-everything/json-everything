using System;

namespace Json.Schema.Api.OpenApi;

internal static class ParsingHelper
{
	public static string Expect(this string source, ref int i, params string[] options)
	{
		var text = source[i..];
		foreach (var option in options)
		{
			if (text.StartsWith(option))
			{
				i += option.Length;
				return option;
			}
		}

		throw new ArgumentOutOfRangeException(nameof(options), "None of the options were found")
		{
			Data = { ["Value"] = text }
		};
	}
}