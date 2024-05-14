using System.Collections.Generic;

namespace Json.Schema.Generation.XmlComments;

/// <summary>
///     Class, Struct or  delegate comments
/// </summary>
internal class TypeComments : CommonComments
{
	/// <summary>
	///     This list contains descriptions of delegate type parameters.
	///     For non-delegate types this list is empty.
	///     For delegate types this list contains tuples where
	///     Item1 is the "param" item "name" attribute and
	///     Item2 is the body of the comment
	/// </summary>
	public List<(string Name, string Text)> Parameters { get; set; } = [];

	/// <summary>
	///     "returns" comment of the method.
	/// </summary>
	public string? Returns { get; set; }
}
