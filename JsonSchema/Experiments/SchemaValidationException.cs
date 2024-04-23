using System;
using Json.Pointer;

namespace Json.Schema.Experiments;

public class SchemaValidationException : Exception
{
	public Uri BaseUri { get; }
	public JsonPointer Location { get; }

	public SchemaValidationException(string message, Uri baseUri, JsonPointer location)
		: base($"{message} at {baseUri}#{location}")
	{
		BaseUri = baseUri;
		Location = location;
	}

	public SchemaValidationException(string message, EvaluationContext context)
		: base($"{message} at {context.BaseUri}#{context.SchemaLocation}")
	{
		BaseUri = context.BaseUri;
		Location = context.SchemaLocation;
	}
}