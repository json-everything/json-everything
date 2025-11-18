#if NETSTANDARD2_0
using System;
using System.Collections.Generic;

namespace Json.Schema;

internal static class DotnetCompatibility
{
	public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
	{
		return dictionary.TryGetValue(key, out var value) ? value : default;
	}
}

/// <summary>
/// Exception thrown when the program executes an instruction that was thought to be unreachable.
/// </summary>
public sealed class UnreachableException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UnreachableException"/> class with the default error message.
	/// </summary>
	public UnreachableException()
		: base("This shouldn't have happened")
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UnreachableException"/>
	/// class with a specified error message.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	public UnreachableException(string? message)
		: base(message ?? "This shouldn't have happened")
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UnreachableException"/>
	/// class with a specified error message and a reference to the inner exception that is the cause of
	/// this exception.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception.</param>
	public UnreachableException(string? message, Exception? innerException)
		: base(message ?? "This shouldn't have happened", innerException)
	{
	}
}
#endif