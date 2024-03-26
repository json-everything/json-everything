using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

/// <summary>
/// Adds an `enum` keyword for the indicated enumeration type.
/// </summary>
public class EnumDataTypeAttributeHandler : IAttributeHandler<EnumDataTypeAttribute>
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
		var enumDataType = (EnumDataTypeAttribute)attribute;

		var values = Enum.GetNames(enumDataType.EnumType).ToList();

		context.Intents.Add(new EnumIntent(values));
	}
}