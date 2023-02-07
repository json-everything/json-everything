namespace Json.Path;

/// <summary>
/// Defines a set of configuration options to control parsing behavior.
/// </summary>
public class PathParsingOptions
{
	/// <summary>
	/// Gets or sets whether explicit typing errors are checked for
	/// filter expressions.
	/// </summary>
	/// <remarks>
	/// For example, `length(1)` is invalid because `1` is not
	/// something that has a length.
	///
	/// This kind of type checking is not prescribed by the specification.
	/// The specification requires that such expressions are to be
	/// accepted and tolerated during evaluation.
	/// </remarks>
	public bool StrictTypeChecking { get; set; }
}