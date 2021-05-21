namespace Json.Schema.Generation
{
	/// <summary>
	/// Indicates the sequence in which properties will be listed in the schema.
	/// </summary>
	public enum PropertyOrder
	{
		/// <summary>
		/// Properties will be listed in the order they're declared in code.
		/// </summary>
		AsDeclared,
		/// <summary>
		/// Properties will be sorted by name, case-insensitive.
		/// </summary>
		ByName
	}
}