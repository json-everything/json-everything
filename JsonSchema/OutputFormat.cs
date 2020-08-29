namespace Json.Schema
{
	/// <summary>
	/// Enumerates the available output formats.
	/// </summary>
	public enum OutputFormat
	{
		/// <summary>
		/// Indicates that only a single pass/fail node will be returned.
		/// </summary>
		Flag,
		/// <summary>
		/// Indicates that all nodes will be listed as children of the top node.
		/// </summary>
		Basic,
		/// <summary>
		/// Indicates that nodes will be organized in a condensed structure that mimicks the schema.
		/// </summary>
		Detailed,
		/// <summary>
		/// Indicates that nodes will match the structure of the schema.
		/// </summary>
		Verbose
	}
}