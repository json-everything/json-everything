namespace Json.Schema
{
	/// <summary>
	/// Indicates that the keyword contains a single schema.
	/// </summary>
	public interface ISchemaContainer
	{
		/// <summary>
		/// A subschema.
		/// </summary>
		JsonSchema Schema { get; }
	}
}