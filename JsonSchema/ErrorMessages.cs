using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Json.Schema;

/// <summary>
/// Manages error messages returned by built-in keywords.
/// </summary>
/// <remarks>
/// Default messages are provided by culture-specific resource files.
///
/// Many messages support tokens in the format `[[foo]]`.
/// Each message will specify the tokens it supports.
/// </remarks>
public static partial class ErrorMessages
{
    private static readonly ResourceManager _resourceManager = new("Json.Schema.Localization.Resources", typeof(ErrorMessages).Assembly);

    /// <summary>
    /// Gets or sets a culture to use for error messages.  Default is <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    public static CultureInfo? Culture { get; set; }

    static ErrorMessages()
    {
    }

    private static string Get(CultureInfo? culture = null, [CallerMemberName] string? key = null)
    {
        if (key == null) throw new ArgumentNullException(nameof(key), "Cannot get a null-keyed resource");

        if (key.StartsWith("Get"))
            key = key.Substring(3);

        return _resourceManager.GetString($"Error_{key}", culture ?? Culture ?? CultureInfo.CurrentCulture) ??
               throw new KeyNotFoundException($"Could not find error message with key '{key}'");
    }

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="parameters">
	/// Tuple of the token name (without brackets) and the value which will replace it.
	/// </param>
	/// <returns>The detokenized string.</returns>
	[RequiresDynamicCode("Calls JsonSerializer.Serialize and might require reflection for token values serialization. Prefer ReplaceTokens that takes TypeInfo parameters for AOT scenarios.")]
	[RequiresUnreferencedCode("Calls JsonSerializer.Serialize and might require reflection for token values serialization. Prefer ReplaceTokens that takes TypeInfo parameters for AOT scenarios.")]
	public static string ReplaceTokens(this string message, params (string token, object? value)[] parameters)
    {
        var current = message;
        var values = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            values[i] = JsonSerializer.Serialize(parameter.value);
            current = current.Replace($"[[{parameter.token}]]", $"{{{i}}}");
        }

        return string.Format(current, values);
    }

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="parameters">
	/// Tuple of the token name (without brackets) and the value and matching JsonTypeInfo which will replace it.
	/// </param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceTokens(this string message, params (string token, object? value, JsonTypeInfo typeInfo)[] parameters)
	{
		var current = message;
		var values = new object[parameters.Length];
		for (var i = 0; i < values.Length; i++)
		{
			var parameter = parameters[i];
			values[i] = JsonSerializer.Serialize(parameter.value, parameter.typeInfo);
			current = current.Replace($"[[{parameter.token}]]", $"{{{i}}}");
		}

		return string.Format(current, values);
	}

	/// <summary>
	/// Helper to make a parameter for ReplaceTokens in a with static type checking.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="token"></param>
	/// <param name="value"></param>
	/// <param name="typeInfo"></param>
	/// <returns></returns>
	public static (string token, object? value, JsonTypeInfo typeInfo) MakeParam<T>(
		string token, T value, JsonTypeInfo<T> typeInfo)
	{
		return (token, (object?)value, typeInfo);
	}
}
