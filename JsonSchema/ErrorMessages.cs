using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Json.Schema;

/// <summary>
/// Manages error messages returned by built-in keywords.
/// </summary>
/// <remarks>
/// Default messages are provided by culture-specific resource files.
///
/// Many messages support tokens in the format <code>[[foo]]</code>.
/// Each message will specify the tokens it supports.
/// </remarks>
public static partial class ErrorMessages
{
	private const string _tokenPatternFormat = @"\[\[{0}(:[^\[]+)?]]";
	private static readonly ResourceManager _resourceManager = new("Json.Schema.Localization.Resources", typeof(ErrorMessages).Assembly);
	
	/// <summary>
	/// Gets or sets a culture to use for error messages.  Default is <see cref="CultureInfo.CurrentCulture"/>.
	/// </summary>
	public static CultureInfo? Culture { get; set; }

	private static string Get([CallerMemberName] string? key = null)
	{
		if (key == null) throw new ArgumentNullException(nameof(key), "Cannot get a null-keyed resource");

		return _resourceManager.GetString($"Error_{key}", Culture ?? CultureInfo.CurrentCulture) ??
		       throw new KeyNotFoundException($"Could not find error message with key '{key}'");
	}

	/// <summary>
	/// Replaces tokens in the form of <code>[[token]]</code> or <code>[[token:format]]</code> with a specified value.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="parameters">
	/// Tuple of the token name (without brackets) and the value which will replace it.
	/// The value will be converted to a string using <code>.ToString()</code>.
	/// </param>
	/// <returns>The detokenized string.</returns>
	/// <remarks>
	/// Formatting is provided by <see cref="string.Format(string, object[])"/>, so all standard and custom
	/// formats apply.
	/// </remarks>
	public static string ReplaceTokens(this string message, params (string token, object value)[] parameters)
	{
		var current = message;
		var values = new object[parameters.Length];
		for (var i = 0; i < parameters.Length; i++)
		{
			var parameter = parameters[i];
			values[i] = parameter.value;
			var pattern = string.Format(_tokenPatternFormat, parameter.token);
			current = Regex.Replace(current, pattern, $"{{{i}$1}}");
		}

		return string.Format(current, values);
	}
}