namespace Json.Schema.ArrayExt;

/// <summary>
/// Allows specifying order direction for <see cref="OrderingKeyword"/>.
/// </summary>
public enum Direction
{
	/// <summary>
	/// Default value.  Not valid.
	/// </summary>
	Unknown,
	/// <summary>
	/// Indicates ascending order.
	/// </summary>
	Ascending,
	/// <summary>
	/// Indicates descending order.
	/// </summary>
	Descending
}