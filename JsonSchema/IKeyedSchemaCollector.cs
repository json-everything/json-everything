using System.Collections.Generic;

namespace Json.Schema
{
	/// <summary>
	/// Indicates that the keyword contains a named collection of schemas.
	/// </summary>
	public interface IKeyedSchemaCollector
	{
		/// <summary>
		/// The subschemas.
		/// </summary>
		IReadOnlyDictionary<string, JsonSchema> Schemas { get; }
	}
}