using System.ComponentModel.DataAnnotations;

namespace Json.Schema.Generation.DataAnnotations;

/// <summary>
/// Adds a `format` keyword with `uri`.
/// </summary>
/// <remarks>
/// By default, `format` is an annotation only.  No validation will occur unless configured to do so.
/// </remarks>
public class UrlAttributeHandler : FormatAttributeHandler<UrlAttribute>
{
	/// <summary>
	/// Creates a new <see cref="UrlAttributeHandler"/>.
	/// </summary>
	public UrlAttributeHandler() : base(Formats.Uri)
	{
	}
}