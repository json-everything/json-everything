using Json.Schema.Generation.Generators;
using System.Collections.Generic;

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
		/// Gets or sets the application of type `null` to schema.
		/// </summary>
		public Nullability Nullability { get; set; } = Nullability.Disabled;

	}
}