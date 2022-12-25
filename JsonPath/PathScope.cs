namespace Json.Path;

/// <summary>
/// Indicates the scope of a path.
/// </summary>
public enum PathScope
{
	/// <summary>
	/// The scope is the entire JSON document.  These paths start with `$`.
	/// </summary>
	Global,
	/// <summary>
	/// The scope is the local JSON value.  These paths start with `@`.
	/// </summary>
	Local
}