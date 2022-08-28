using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Json.Schema;

/// <summary>
/// Thrown when a reference cannot be resolved.
/// </summary>
/// <remarks>
/// This class will be moved into a future version of JsonSchema.Net.
/// </remarks>
public class RefResolutionException : Exception
{
	/// <summary>
	/// The URI that could not be resolved.
	/// </summary>
	public IEnumerable<string> References { get; }

	/// <summary>
	/// Thrown when a reference cannot be resolved.
	/// </summary>
	/// <param name="references">The references that could not be resolved.</param>
	/// <param name="innerException">The exception that caused this error.</param>
	public RefResolutionException(IEnumerable<string> references, Exception? innerException = null)
		: base("An error occurred attempting to resolve one or more references", innerException)
	{
		References = references;
	}
}