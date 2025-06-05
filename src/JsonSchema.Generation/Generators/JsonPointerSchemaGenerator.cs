﻿using System;
using Json.Pointer;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators;

internal class JsonPointerSchemaGenerator : ISchemaGenerator
{
	public bool Handles(Type type)
	{
		return type == typeof(JsonPointer_Old);
	}

	public void AddConstraints(SchemaGenerationContextBase context)
	{
		context.Intents.Add(new TypeIntent(SchemaValueType.String));
		context.Intents.Add(new FormatIntent(Formats.JsonPointer));
	}
}