namespace Json.JsonE;

/// <summary>
/// Thrown from <see cref="JsonE.Evaluate"/> when an expression contains invalid syntax.
/// </summary>
public class SyntaxException : JsonEException
{
	/// <summary>
	/// Creates a new instance of <see cref="SyntaxException"/>.
	/// </summary>
	/// <param name="message">The error message.</param>
	public SyntaxException(string message) : base(message) {}
}