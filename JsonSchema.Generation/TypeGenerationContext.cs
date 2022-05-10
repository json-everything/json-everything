using System;

namespace Json.Schema.Generation;

/// <summary>
/// Provides a context for generating schemas for types.
/// </summary>
public class TypeGenerationContext : SchemaGenerationContextBase
{
	internal TypeGenerationContext(Type type)
		: base(type)
	{
	}
}