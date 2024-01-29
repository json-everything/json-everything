using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text.Json;

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
    private static JsonSerializerOptions? _serializerOptions;

    /// <summary>
    /// Gets or sets a culture to use for error messages.  Default is <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    public static CultureInfo? Culture { get; set; }

    static ErrorMessages()
    {
        _serializerOptions = new JsonSerializerOptions
        {
#if NET8_0_OR_GREATER
            TypeInfoResolver = JsonSchema.TypeInfoResolver
#endif
        };
#if NET8_0_OR_GREATER
        JsonSchemaSerializerContext.OptionsManager.TypeInfoResolverUpdated +=
            (_, _) => _serializerOptions = new JsonSerializerOptions
            {
                TypeInfoResolver = JsonSchema.TypeInfoResolver
            };
#endif
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
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
    public static string ReplaceTokens(this string message, params (string token, object? value)[] parameters)
    {
        var current = message;
        var values = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            values[i] = JsonSerializer.Serialize(parameter.value, _serializerOptions);
            current = current.Replace($"[[{parameter.token}]]", $"{{{i}}}");
        }

        return string.Format(current, values);
    }
}