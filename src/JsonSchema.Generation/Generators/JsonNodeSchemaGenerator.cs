using System;
using System.Text.Json.Nodes;

namespace Json.Schema.Generation.Generators;

internal class JsonNodeSchemaGenerator : ISchemaGenerator
{
	public bool Handles(Type type)
	{
		return type == typeof(JsonNode);
	}

	public void AddConstraints(SchemaGenerationContextBase context)
	{
		// don't add anything.  really, we want a true schema here.
	}
}