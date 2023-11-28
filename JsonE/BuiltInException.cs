using System;
using System.Text.Json.Nodes;

namespace Json.JsonE;

/// <summary>
/// Thrown from <see cref="JsonE.Evaluate(JsonNode?,JsonNode?)"/> when a built-in function cannot be evaluated.
/// </summary>
public class BuiltInException : JsonEException
{
	/// <summary>
	/// Creates a new instance of <see cref="BuiltInException"/>.
	/// </summary>
	/// <param name="message">The error message.</param>
	public BuiltInException(string message) : base(message){}
}