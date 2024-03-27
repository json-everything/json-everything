using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

/// <summary>
/// Base class for attributes that just need to add a `format` keyword.
/// </summary>
/// <typeparam name="T">The attribute type.</typeparam>
public abstract class FormatAttributeHandler<T> : IAttributeHandler<T>
	where T : Attribute
{
	private readonly Format _format;

	/// <summary>
	/// Creates a new handler that uses a custom format.
	/// </summary>
	/// <param name="format">The format.</param>
	public FormatAttributeHandler(string format)
	{
		_format = new Format(format);
	}

	/// <summary>
	/// Creates a new handler that uses a predefined format.
	/// </summary>
	/// <param name="format">The format.</param>
	public FormatAttributeHandler(Format format)
	{
		_format = format;
	}

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
	public virtual void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		context.Intents.Add(new FormatIntent(_format));
	}
}
