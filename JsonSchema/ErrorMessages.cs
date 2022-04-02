using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

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
public static class ErrorMessages
{
	private static readonly ResourceManager _resourceManager = new("Json.Schema.Localization.Resources", typeof(ErrorMessages).Assembly);

	private static string? _minimum;

	public static CultureInfo? Culture { get; set; }

	/// <summary>
	/// Gets or sets the error message for <see cref="MinimumKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string Minimum
	{
		get => _minimum ?? Get();
		set => _minimum = value;
	}

	private static string Get([CallerMemberName] string? key = null)
	{
		if (key == null) throw new ArgumentNullException(nameof(key), "Cannot get a null-keyed resource");
	
		return _resourceManager.GetString($"Error_{key}", Culture ?? CultureInfo.CurrentCulture) ??
		       throw new KeyNotFoundException($"Could not find error message with key '{key}'");
	}
}