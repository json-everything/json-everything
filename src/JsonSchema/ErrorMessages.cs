using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
            key = key[3..];

        return _resourceManager.GetString($"Error_{key}", culture ?? Culture ?? CultureInfo.CurrentCulture) ??
               throw new KeyNotFoundException($"Could not find error message with key '{key}'");
    }

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <returns>The detokenized string.</returns>
	[RequiresDynamicCode("Calls JsonSerializer.Serialize and might require reflection for token values serialization. Prefer ReplaceToken that takes TypeInfo parameters for AOT scenarios.")]
	[RequiresUnreferencedCode("Calls JsonSerializer.Serialize and might require reflection for token values serialization. Prefer ReplaceToken that takes TypeInfo parameters for AOT scenarios.")]
	public static string ReplaceToken(this string message, string name, object? value)
    {
		var stringValue = JsonSerializer.Serialize(value);
		return message.Replace($"[[{name}]]", stringValue);
	}

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <param name="typeInfo">A JsonTypeInfo that matches <paramref name="value"/>.</param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceToken<T>(this string message, string name, T? value, JsonTypeInfo<T> typeInfo)
	{
		var stringValue = JsonSerializer.Serialize(value, typeInfo!);
		return message.Replace($"[[{name}]]", stringValue);
	}

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceToken(this string message, string name, bool value) => 
		ReplaceToken(message, name, value, JsonSchemaSerializerContext.Default.Boolean);

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceToken(this string message, string name, int value) => 
		ReplaceToken(message, name, value, JsonSchemaSerializerContext.Default.Int32);

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceToken(this string message, string name, long value) =>
		ReplaceToken(message, name, value, JsonSchemaSerializerContext.Default.Int64);

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceToken(this string message, string name, decimal value) => 
		ReplaceToken(message, name, value, JsonSchemaSerializerContext.Default.Decimal);

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceToken(this string message, string name, double value) => 
		ReplaceToken(message, name, value, JsonSchemaSerializerContext.Default.Double);

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceToken(this string message, string name, string value) => 
		ReplaceToken(message, name, value, JsonSchemaSerializerContext.Default.String);

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceToken(this string message, string name, string[] value) =>
		ReplaceToken(message, name, value, JsonSchemaSerializerContext.Default.StringArray);

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceToken(this string message, string name, Dictionary<string, string[]> value) =>
		ReplaceToken(message, name, value, JsonSchemaSerializerContext.Default.DictionaryStringStringArray);

	/// <summary>
	/// Replaces tokens in the form of `[[token]]` with a specified value, serialized as JSON.
	/// </summary>
	/// <param name="message">The message template.</param>
	/// <param name="name">The token name (without brackets)</param>
	/// <param name="value">The value.</param>
	/// <returns>The detokenized string.</returns>
	public static string ReplaceToken(this string message, string name, JsonElement value) =>
		ReplaceToken(message, name, value, JsonSchemaSerializerContext.Default.JsonElement);
}
