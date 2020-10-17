using System.Collections.Generic;

namespace Json.Schema
{
	/// <summary>
	/// Indicates that the keyword contains a collection of schemas.
	/// </summary>
	public interface ISchemaCollector
	{
		/// <summary>
		/// The subschemas.
		/// </summary>
		IReadOnlyList<JsonSchema> Schemas { get; }
	}
}