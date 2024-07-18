using System.Text.Json.Nodes;

namespace Json.JsonE;

/// <summary>
/// Thrown from <see cref="JsonE.Evaluate(JsonNode?,JsonNode?)"/> when a template cannot be evaluated.
/// </summary>
public class InterpreterException : JsonEException
{
	/// <summary>
	/// Creates a new instance of <see cref="InterpreterException"/>.
	/// </summary>
	/// <param name="message">The error message.</param>
	public InterpreterException(string message) : base(message){}
}