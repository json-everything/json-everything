using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Adds attribute-related schema elements.
	/// </summary>
	public static class AttributeHandler
	{
		private static readonly List<IAttributeHandler> _handlers =
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
			if (_handlers.Any(h => h.GetType() == typeof(T))) return;

			_handlers.Add(new T());
		}

		/// <summary>
		/// Adds a handler for a custom attribute that cannot be made to implement <see cref="IAttributeHandler"/>.
		/// </summary>
		/// <param name="handler">The handler.</param>
		public static void AddHandler(IAttributeHandler handler)
		{
			var handlerType = handler.GetType();
			if (_handlers.Any(h => h.GetType() == handlerType)) return;

			_handlers.Add(handler);
		}

		/// <summary>
		/// Removes a handler type.
		/// </summary>
		/// <typeparam name="T">The handler type.</typeparam>
		public static void RemoveHandler<T>()
			where T : IAttributeHandler
		{
			var handler = _handlers.OfType<T>().FirstOrDefault();
			if (handler == null) return;

			_handlers.Remove(handler);
		}

		internal static void HandleAttributes(SchemaGeneratorContext context)
		{
			var handlers = _handlers.Concat(context.Attributes.OfType<IAttributeHandler>());

			foreach (var handler in handlers)
			{
				handler.AddConstraints(context);
			}
		}
	}
}