using System;
using System.Collections.Generic;
using System.Reflection;

namespace Json.Schema.Generation;

/// <summary>
/// Extension methods for context objects.
/// </summary>
public static class ContextExtensions
{
	/// <summary>
	/// Gets the attribute set.  Type contexts get type attributes; member context
	/// get member attributes.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <returns>The attribute set.</returns>
	/// <exception cref="InvalidOperationException">Thrown for other context types.</exception>
	public static IEnumerable<Attribute> GetAttributes(this SchemaGenerationContextBase context) =>
		context switch
		{
			MemberGenerationContext memberContext => memberContext.Attributes,
			TypeGenerationContext typeContext => typeContext.Type.GetCustomAttributes(),
			_ => throw new InvalidOperationException($"Unknown context type: {context.GetType().Name}")
		};
}