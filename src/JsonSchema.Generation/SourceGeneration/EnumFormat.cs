namespace Json.Schema.Generation.SourceGeneration;

/// <summary>
/// Indicates how enumerations are described in source-generated schemas.
/// </summary>
public enum EnumFormat
{
	/// <summary>
	/// Enumerations are described by an `enum` of their member names.
	/// </summary>
	Names,
	/// <summary>
	/// Enumerations are described as an `integer`.
	/// </summary>
	Values,
	/// <summary>
	/// Enumerations are described by an `anyOf` accepting either the member names or an `integer`.
	/// </summary>
	NamesAndValues
}
