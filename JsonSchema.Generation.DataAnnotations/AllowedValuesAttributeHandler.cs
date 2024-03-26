#if NET8_0_OR_GREATER

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

/// <summary>
/// Adds an `enum` keyword for the indicated values.
/// </summary>
/// <remarks>
/// For NativeAOT scenarios, only primitive JSON types are supported.
/// </remarks>
public class AllowedValuesAttributeHandler : IAttributeHandler<AllowedValuesAttribute>
{
	/// <summary>
	/// Processes the type and any attributes (present on the context), and adds
	/// intents to the context.
	/// </summary>
	/// <param name="context">The generation context.</param>
	/// <param name="attribute">The attribute.</param>
	/// <remarks>
	/// A common pattern is to implement <see cref="IAttributeHandler"/> on the
	/// attribute itself.  In this case, the <paramref name="attribute"/> parameter
	/// will be the same instance as the handler and can likely be ignored.
	/// </remarks>
	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		var allowedValues = (AllowedValuesAttribute)attribute;

		context.Intents.Add(new EnumIntent(allowedValues.Values.Select(x => JsonSerializer.SerializeToNode(x, DataAnnotationsSerializerContext.Default.Options)!)));
	}
}

#endif