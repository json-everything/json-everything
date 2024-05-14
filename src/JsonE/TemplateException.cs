using System.Text.Json.Nodes;

namespace Json.JsonE;

/// <summary>
/// Thrown from <see cref="JsonE.Evaluate(JsonNode?,JsonNode?)"/> when a template is invalid.
/// </summary>
public class TemplateException : JsonEException
{
	/// <summary>
	/// Creates a new instance of <see cref="TemplateException"/>.
	/// </summary>
	/// <param name="message">The error message.</param>
	public TemplateException(string message) : base(message){}
}