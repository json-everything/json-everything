#if NET8_0_OR_GREATER

using System.ComponentModel.DataAnnotations;

namespace Json.Schema.Generation.DataAnnotations;

public class Base64StringAttributeAttributeHandler : FormatAttributeHandler<Base64StringAttribute>
{
	public Base64StringAttributeAttributeHandler() : base("base64")
	{
	}
}

#endif