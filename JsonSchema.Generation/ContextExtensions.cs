using System;
using System.Collections.Generic;
using System.Reflection;

namespace Json.Schema.Generation;

public static class ContextExtensions
{
	public static IEnumerable<Attribute> GetAttributes(this SchemaGenerationContextBase context) =>
		context switch
		{
			MemberGenerationContext memberContext => memberContext.Attributes,
			TypeGenerationContext typeContext => typeContext.Type.GetCustomAttributes(),
			_ => throw new InvalidOperationException($"Unknown context type: {context.GetType().Name}")
		};
}