using System;
using System.Text.Json.Nodes;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators;

internal class JsonArraySchemaGenerator : ISchemaGenerator
{
	public bool Handles(Type type)
	{
		return type == typeof(JsonArray);
	}

	public void AddConstraints(SchemaGenerationContextBase context)
	{
		context.Intents.Add(new TypeIntent(SchemaValueType.Array));
	}
}