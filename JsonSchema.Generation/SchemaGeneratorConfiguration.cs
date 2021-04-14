using System.Collections.Generic;
using Json.Schema.Generation.Generators;

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
	}
}