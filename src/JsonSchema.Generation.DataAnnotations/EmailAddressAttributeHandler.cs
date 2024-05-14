using System.ComponentModel.DataAnnotations;

namespace Json.Schema.Generation.DataAnnotations;

/// <summary>
/// Adds a `format` keyword with `email`.
/// </summary>
/// <remarks>
/// By default, `format` is an annotation only.  No validation will occur unless configured to do so.
/// </remarks>
public class EmailAddressAttributeHandler : FormatAttributeHandler<EmailAddressAttribute>
{
	/// <summary>
	/// Creates a new <see cref="EmailAddressAttributeHandler"/>.
	/// </summary>
	public EmailAddressAttributeHandler() : base(Formats.Email)
	{
	}
}