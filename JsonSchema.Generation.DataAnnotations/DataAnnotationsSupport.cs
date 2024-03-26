using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.DataAnnotations;

public static class DataAnnotationsSupport
{
	private static readonly List<IAttributeHandler> _externalHandlers =
		typeof(DataAnnotationsSupport)
			.Assembly
			.DefinedTypes
			.Where(t => typeof(IAttributeHandler).IsAssignableFrom(t) &&
			            !typeof(Attribute).IsAssignableFrom(t) &&
			            !t.IsAbstract && !t.IsInterface)
			.Select(Activator.CreateInstance)
			.Cast<IAttributeHandler>()
			.ToList();

	public static void AddDataAnnotations()
	{
		foreach (var handler in _externalHandlers)
		{
			AttributeHandler.AddHandler(handler);
		}
	}
}