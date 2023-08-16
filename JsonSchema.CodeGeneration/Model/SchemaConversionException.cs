using System;

namespace Json.Schema.CodeGeneration.Model;

public class SchemaConversionException : Exception
{
	public SchemaConversionException(string message)
		: base(message)
	{
	}
}