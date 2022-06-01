using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema.Generation;

/// <summary>
/// Adds attribute-related schema elements.
/// </summary>
public static class AttributeHandler
{
	private static readonly List<IAttributeHandler> _externalHandlers =
		typeof(IAttributeHandler)
			.Assembly
			.DefinedTypes
			.Where(t => typeof(IAttributeHandler).IsAssignableFrom(t) &&
						!typeof(Attribute).IsAssignableFrom(t) &&
						!t.IsAbstract && !t.IsInterface)
			.Select(Activator.CreateInstance)
			.Cast<IAttributeHandler>()
			.ToList();

	/// <summary>
	/// Adds a handler for a custom attribute that cannot be made to implement <see cref="IAttributeHandler"/>.
	/// </summary>
	/// <typeparam name="T">The handler type.</typeparam>
	public static void AddHandler<T>()
		where T : IAttributeHandler, new()
	{
		if (typeof(Attribute).IsAssignableFrom(typeof(T))) return;
		if (_externalHandlers.Any(h => h.GetType() == typeof(T))) return;

		_externalHandlers.Add(new T());
	}

	/// <summary>
	/// Adds a handler for a custom attribute that cannot be made to implement <see cref="IAttributeHandler"/>.
	/// </summary>
	/// <param name="handler">The handler.</param>
	public static void AddHandler(IAttributeHandler handler)
	{
		var handlerType = handler.GetType();
		if (typeof(Attribute).IsAssignableFrom(handlerType)) return;
		if (_externalHandlers.Any(h => h.GetType() == handlerType)) return;

		_externalHandlers.Add(handler);
	}

	/// <summary>
	/// Removes a handler type.
	/// </summary>
	/// <typeparam name="T">The handler type.</typeparam>
	public static void RemoveHandler<T>()
		where T : IAttributeHandler
	{
		var handler = _externalHandlers.OfType<T>().FirstOrDefault();
		if (handler == null) return;

		_externalHandlers.Remove(handler);
	}

	internal static void HandleAttributes(SchemaGenerationContextBase context)
	{
		IEnumerable<IAttributeHandler> handlers = _externalHandlers;

		var attributes = context.GetAttributes().ToList();

		handlers = handlers.Concat(attributes.OfType<IAttributeHandler>());

		foreach (var handler in handlers)
		{
			var attribute = handler as Attribute;
			if (attribute == null)
			{
				var interfaces = handler.GetType().GetInterfaces();
				var handlerInterface = interfaces.FirstOrDefault(x => x.IsGenericType &&
																	  x.GetGenericTypeDefinition() == typeof(IAttributeHandler<>));
				if (handlerInterface == null) continue;

				var attributeType = handlerInterface.GetGenericArguments()[0];
				attribute = attributes.FirstOrDefault(x => x.GetType() == attributeType);

				if (attribute == null) continue;
			}

			handler.AddConstraints(context, attribute);
		}
	}

	internal static IEnumerable<Attribute> WhereHandled(this IEnumerable<Attribute> attributes)
	{
		return attributes.Where(x => x is IAttributeHandler ||
		                             _externalHandlers.Any(h => typeof(IAttributeHandler<>)
			                             .MakeGenericType(x.GetType())
			                             .IsInstanceOfType(h)));
	}
}