using System;
using System.Diagnostics;

namespace Json.Schema.Generation;

/// <summary>
/// Provides a context for generating schemas for types.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay}")]
public class TypeGenerationContext : SchemaGenerationContextBase
{
	internal TypeGenerationContext(Type type)
		: base(type)
	{
	}
}