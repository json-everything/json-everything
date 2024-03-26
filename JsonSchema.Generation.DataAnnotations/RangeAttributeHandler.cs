using System;
using System.ComponentModel.DataAnnotations;
using Json.More;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

/// <summary>
/// Adds `minimum` and `maximum` keywords.
/// </summary>
/// <remarks>
/// Only numeric types are supported.
/// </remarks>
public class RangeAttributeHandler : IAttributeHandler<RangeAttribute>
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
		var range = (RangeAttribute)attribute;
		if (!range.OperandType.IsNumber() && !range.OperandType.IsNullableNumber()) return;

		context.Intents.Add(new MinimumIntent(Convert.ToDecimal(range.Minimum)));
		context.Intents.Add(new MaximumIntent(Convert.ToDecimal(range.Maximum)));
	}
}