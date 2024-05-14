using System;
using System.Text.Json.Nodes;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators;

internal class JsonObjectSchemaGenerator : ISchemaGenerator
{
	public bool Handles(Type type)
	{
		return type == typeof(JsonObject);
	}

	public void AddConstraints(SchemaGenerationContextBase context)
	{
		context.Intents.Add(new TypeIntent(SchemaValueType.Object));
	}
}