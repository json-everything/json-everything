using System.ComponentModel.DataAnnotations;

namespace Json.Schema.Generation.DataAnnotations;

public class EmailAddressAttributeHandler : FormatAttributeHandler<EmailAddressAttribute>
{
	public EmailAddressAttributeHandler() : base(Formats.Email)
	{
	}
}