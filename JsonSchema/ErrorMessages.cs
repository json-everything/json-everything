using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;

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
	/// Replaces tokens in the form of <code>[[token]]</code> with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="parameters">
	/// Tuple of the token name (without brackets) and the value which will replace it.
	/// </param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceTokens(this string message, params (string token, object value)[] parameters)
	{
		var current = message;
		var values = new object[parameters.Length];
		for (var i = 0; i < parameters.Length; i++)
		{
			var parameter = parameters[i];
			values[i] = JsonSerializer.Serialize(parameter.value, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
			current = current.Replace($"[[{parameter.token}]]", $"{{{i}}}");
		}

		return string.Format(current, values);
	}
}