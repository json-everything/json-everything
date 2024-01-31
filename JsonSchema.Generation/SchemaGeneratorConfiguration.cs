using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Json.Schema.Generation.Generators;

namespace Json.Schema.Generation;

/// <summary>
/// Provides additional configuration for the generator.
/// </summary>
public class SchemaGeneratorConfiguration
{
	private PropertyNameResolver? _propertyNameResolver;

	private sealed class DummyInfo : MemberInfo
	{
		public override object[] GetCustomAttributes(bool inherit) => Array.Empty<object>();

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) => Array.Empty<object>();

		public override bool IsDefined(Type attributeType, bool inherit) => false;

		public override Type DeclaringType { get; } = typeof(DummyInfo);
		public override MemberTypes MemberType => MemberTypes.Property;
		public override string Name { get; }
		public override Type? ReflectedType => null;

		public DummyInfo(string name)
		{
			Name = name;
		}
	}

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

	/// <summary>
	/// Gets or sets the property name resolving method. Default is <see cref="PropertyNameResolvers.AsDeclared"/>.
	/// </summary>
	/// <remarks>
	/// This can be replaced with any `Func&lt;MemberInfo, string&gt;`.
	/// </remarks>
	public PropertyNameResolver? PropertyNameResolver
	{
		get => _propertyNameResolver ??= PropertyNameResolvers.AsDeclared;
		set => _propertyNameResolver = value;
	}

	/// <summary>
	/// Gets or sets whether to include `null` in the `type` keyword.
	/// Default is <see cref="Nullability.Disabled"/> which means that it will
	/// not ever be included.
	/// </summary>
	public Nullability Nullability { get; set; }

	/// <summary>
	/// Gets or sets whether optimizations (moving common subschemas into `$defs`) will be performed.  Default is true.
	/// </summary>
	public bool Optimize { get; set; } = true;

	/// <summary>
	/// Gets or sets whether properties that are affected by conditionals are defined
	/// globally or only within their respective `then` subschemas.  True restricts
	/// those property definitions to `then` subschemas and adds a top-level
	/// `unevaluatedProperties: false`; false (default) defines them globally.
	/// </summary>
	public bool StrictConditionals { get; set; }

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