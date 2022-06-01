using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Json.Schema.Generation.Generators;

namespace Json.Schema.Generation;

/// <summary>
/// Provides additional configuration for the generator.
/// </summary>
public class SchemaGeneratorConfiguration
{
	private PropertyNamingMethod? _propertyNamingMethod;

	/// <summary>
	/// A collection of refiners.
	/// </summary>
	[UsedImplicitly]
	public List<ISchemaRefiner> Refiners { get; } = new();
	/// <summary>
	/// A collection of generators in addition to the global set.
	/// </summary>
	[UsedImplicitly]
	public List<ISchemaGenerator> Generators { get; } = new();
	/// <summary>
	/// Gets or sets the order in which properties will be listed in the schema.
	/// </summary>
	public PropertyOrder PropertyOrder { get; set; }

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
	/// <summary>
	/// Gets or sets the property naming method.  Default is <see cref="PropertyNamingMethod.AsDeclared"/>.
	/// </summary>
	/// <remarks>
	/// This can be replaced with any `Func&lt;string, string&gt;`.
	/// </remarks>
	public PropertyNamingMethod PropertyNamingMethod
	{
		get => _propertyNamingMethod ??= PropertyNamingMethods.AsDeclared;
		set => _propertyNamingMethod = value;
	}
	/// <summary>
	/// Gets or sets whether to include `null` in the `type` keyword.
	/// Default is <see cref="Nullability.Disabled"/> which means that it will
	/// not ever be included.
	/// </summary>
	public Nullability Nullability { get; set; }
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved

	/// <summary>
	/// Gets or sets whether optimizations (moving common subschemas into `$defs`) will be performed.  Default is true.
	/// </summary>
	public bool Optimize { get; set; } = true;

#pragma warning disable CS8618
	/// <summary>
	/// Thread-static storage of the current configuration. Only to be used for reading
	/// the configuration. Setting values on this object will be overwritten when starting
	/// generation.
	/// </summary>
	[field: ThreadStatic]
	public static SchemaGeneratorConfiguration Current { get; internal set; }
#pragma warning restore CS8618
}