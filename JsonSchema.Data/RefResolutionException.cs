using System;
using System.Collections.Generic;
using Json.Schema.Data;

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
	public RefResolutionException(IEnumerable<string> references)
		: base("An error occurred attempting to resolve one or more references")
	{
		References = references;
	}
}