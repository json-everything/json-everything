using System.ComponentModel.DataAnnotations;

namespace Json.Schema.Generation.DataAnnotations;

public class UrlAttributeHandler : FormatAttributeHandler<UrlAttribute>
{
	public UrlAttributeHandler() : base(Formats.Uri)
	{
	}
}