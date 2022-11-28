using System;

namespace Json.Schema;

public class JsonSchemaException : Exception
{
	public JsonSchemaException(string message)
		: base(message)
	{
	}

	public JsonSchemaException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}