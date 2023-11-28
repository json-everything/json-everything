using System;

namespace Json.JsonE;

/// <summary>
/// Thrown when something goes wrong during a JSON-e evaluation.
/// </summary>
public abstract class JsonEException : Exception
{
	/// <summary>
	/// Initializes a new <see cref="JsonEException"/>.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	protected JsonEException(string message) : base(message) {}
}