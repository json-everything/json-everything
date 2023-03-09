namespace Json.Path;

/// <summary>
/// Defines a set of configuration options to control parsing behavior.
/// </summary>
public class PathParsingOptions
{
	/// <summary>
	/// Gets or sets whether mathematical operators will be allowed.
	/// </summary>
	/// <remarks>
	/// These operators are an extension of the specification, so they
	/// are disallowed by default.
	/// </remarks>
	public bool AllowMathOperations { get; set; }
}