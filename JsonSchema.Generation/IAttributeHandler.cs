using System;

namespace Json.Schema.Generation;

/// <summary>
/// Defines requirements to handle converting an attribute to a keyword intent.
/// </summary>
public interface IAttributeHandler
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
	void AddConstraints(SchemaGenerationContextBase context, Attribute attribute);
}

/// <summary>
/// Processes attributes of type <typeparamref name="T"/> that are present on a
/// type or member and adds intents to the context.
/// </summary>
/// <typeparam name="T">The type of the attribute that is handled.</typeparam>
public interface IAttributeHandler<T> : IAttributeHandler
	where T : Attribute
{
}