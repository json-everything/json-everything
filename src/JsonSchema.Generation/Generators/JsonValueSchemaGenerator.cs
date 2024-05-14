using System;
using System.Text.Json.Nodes;

namespace Json.Schema.Generation.Generators;

internal class JsonValueSchemaGenerator : ISchemaGenerator
{
	public bool Handles(Type type)
	{
		return type == typeof(JsonValue);
	}

	public void AddConstraints(SchemaGenerationContextBase context)
	{
		// don't add constraints; any JSON value is fine here
	}
}