using System.ComponentModel;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Allows specifying order direction for <see cref="OrderingKeyword"/>.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<Direction>))]
public enum Direction
{
	/// <summary>
	/// Default value.  Not valid.
	/// </summary>
	Unknown,
	/// <summary>
	/// Indicates ascending order.
	/// </summary>
	[Description("asc")]
	Ascending,
	/// <summary>
	/// Indicates descending order.
	/// </summary>
	[Description("desc")]
	Descending
}