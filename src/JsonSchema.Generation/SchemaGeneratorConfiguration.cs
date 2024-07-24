using System;
using System.Collections.Generic;
using Json.Schema.Generation.Generators;
using Json.Schema.Generation.XmlComments;

namespace Json.Schema.Generation;

/// <summary>
/// Provides additional configuration for the generator.
/// </summary>
public class SchemaGeneratorConfiguration
{
	private readonly Dictionary<string, string> _xmlCommentsFiles = [];
	private PropertyNameResolver? _propertyNameResolver;

	/// <summary>
	/// Thread-static storage of the current configuration. Only to be used for reading
	/// the configuration. Setting values on this object will be overwritten when starting
	/// generation.
	/// </summary>
	[field: ThreadStatic]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public static SchemaGeneratorConfiguration Current { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	internal DocXmlReader XmlReader { get; }

	/// <summary>
	/// A collection of refiners.
	/// </summary>
	public List<ISchemaRefiner> Refiners { get; } = [];
	/// <summary>
	/// A collection of generators in addition to the global set.
	/// </summary>
	// ReSharper disable once CollectionNeverUpdated.Global
	public List<ISchemaGenerator> Generators { get; } = [];
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

	/// <summary>
	/// Allows mapping of types to external schema `$id`s.  When encountering one
	/// of these types, a `$ref` keyword will be generated instead of a full schema.
	/// </summary>
	public Dictionary<Type, Uri> ExternalReferences { get; } = [];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	/// <summary>
	/// Creates a new <see cref="SchemaGeneratorConfiguration"/>.
	/// </summary>
	public SchemaGeneratorConfiguration()
	{
		XmlReader = new DocXmlReader(assembly => _xmlCommentsFiles.TryGetValue(assembly.FullName, out var path) ? path : null);
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	/// <summary>
	/// Registers an assembly's XML comment file.
	/// </summary>
	/// <typeparam name="T">Any type in the assembly.</typeparam>
	/// <param name="filename">The file name of the XML file.</param>
	public void RegisterXmlCommentFile<T>(string filename)
	{
		var assembly = typeof(T).Assembly;
		_xmlCommentsFiles[assembly.FullName] = filename;
	}
}