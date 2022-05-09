using System;

namespace Json.Schema.Generation;

public class TypeGenerationContext : SchemaGenerationContextBase
{
	internal TypeGenerationContext(Type type)
	{
		Type = type;
	}
}