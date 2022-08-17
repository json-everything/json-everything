using System;

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
	public Uri Uri { get; }

	/// <summary>
	/// Thrown when a reference cannot be resolved.
	/// </summary>
	/// <param name="uri">The URI that could not be resolved.</param>
	/// <param name="inner">The inner exception.</param>
	public RefResolutionException(Uri uri, Exception inner)
		: base($"An error occurred attempting to resolve URI `{uri}`", inner)
	{
		Uri = uri;
	}
}