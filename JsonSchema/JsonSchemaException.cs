using System;

namespace Json.Schema;

/// <summary>
/// Thrown when a schema is invalid or cannot be processed with the current configuration.
/// </summary>
public class JsonSchemaException : Exception
{
	/// <summary>
	/// Creates a new instance of the <see cref="JsonSchemaException"/> type.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public JsonSchemaException(string message)
		: base(message)
	{
	}
}