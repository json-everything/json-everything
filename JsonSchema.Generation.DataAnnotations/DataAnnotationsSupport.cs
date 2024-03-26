using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#pragma warning disable IL2026

namespace Json.Schema.Generation.DataAnnotations;

/// <summary>
/// Exposes support for the System.ComponentModel.DataAnnotations namespace.
/// </summary>
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

	/// <summary>
	/// Adds support for the System.ComponentModel.DataAnnotations namespace attributes.
	/// </summary>
	[RequiresDynamicCode("This method uses reflection to query types and is not suited for AOT scenarios.")]
	public static void AddDataAnnotations()
	{
		foreach (var handler in _externalHandlers)
		{
			AttributeHandler.AddHandler(handler);
		}
	}
}