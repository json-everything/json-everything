using System;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Thrown when a `$ref` cannot be resolved.
/// </summary>
public class RefResolutionException : Exception
{
	/// <summary>
	/// Creates a new <see cref="RefResolutionException"/>
	/// </summary>
	/// <param name="message">The exception message.</param>
	public RefResolutionException(string message)
		: base(message)
	{
	}
}