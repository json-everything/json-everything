using System;
using System.Text.Json.Nodes;

namespace Json.JsonE;

/// <summary>
/// Thrown from <see cref="JsonETemplate.Create(JsonNode?)"/> when a template is invalid.
/// </summary>
public class TemplateException : Exception
{
	/// <summary>
	/// Creates a new instance of <see cref="TemplateException"/>.
	/// </summary>
	/// <param name="message">The error message.</param>
	public TemplateException(string message) : base(message){}
}