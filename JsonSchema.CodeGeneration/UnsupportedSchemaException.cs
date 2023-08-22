using System;

namespace Json.Schema.CodeGeneration;

/// <summary>
/// Thrown when the generator encounters an unsupported scenario.
/// </summary>
public class UnsupportedSchemaException : Exception
{
	/// <summary>
	/// Creates a new instance of the exception.
	/// </summary>
	/// <param name="message">The exception message.</param>
	public UnsupportedSchemaException(string message)
		: base(message)
	{
	}
}