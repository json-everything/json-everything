namespace Json.Path;

/// <summary>
/// Indicates the return type of a filter expression function.
/// </summary>
public enum FunctionType
{
	/// <summary>
	/// Holder for a default value.  Not an actual valid function type.
	/// </summary>
	Unspecified,
	/// <summary>
	/// Indicates the function returns a JSON-like value that can be
	/// compared with equality and inequality operators.
	/// </summary>
	Value,
	/// <summary>
	/// Indicates the function returns a non-JSON boolean value that can be
	/// compared with logical operators.
	/// </summary>
	Logical,
	/// <summary>
	/// Indicates the function returns a nodelist.
	/// </summary>
	Nodelist
}