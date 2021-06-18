using System.Collections.Generic;
using Json.Schema.Generation.Generators;
using Json.Schema.Generation.Refiners;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Provides additional configuration for the generator.
	/// </summary>
	public class SchemaGeneratorConfiguration
	{
		/// <summary>
		/// A collection of refiners.
		/// </summary>
		public List<ISchemaRefiner> Refiners { get; set; } = new List<ISchemaRefiner>();
		/// <summary>
		/// A collection of generators in addition to the global set.
		/// </summary>
		public List<ISchemaGenerator> Generators { get; set; } = new List<ISchemaGenerator>();
		/// <summary>
		/// Gets or sets the order in which properties will be listed in the schema.
		/// </summary>
		public PropertyOrder PropertyOrder { get; set; }
		/// <summary>
		/// Gets or sets whether to include `null` in the `type` keyword.
		/// Default is <see cref="Nullability.Disabled"/> which means that it will
		/// not ever be included.
		/// </summary>
		public Nullability Nullability { get; set; }
	}
}