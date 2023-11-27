using System;
using System.Text.Json.Nodes;

namespace Json.JsonE;

/// <summary>
/// Thrown from <see cref="JsonE.Evaluate(JsonNode?, JsonNode?)"/> when ???.
/// </summary>
public class TypeException : JsonEException
{
	/// <summary>
	/// Creates a new instance of <see cref="TemplateException"/>.
	/// </summary>
	/// <param name="message">The error message.</param>
	public TypeException(string message) : base(message){}
}