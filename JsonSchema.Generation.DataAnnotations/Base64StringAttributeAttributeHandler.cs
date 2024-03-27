#if NET8_0_OR_GREATER

using System.ComponentModel.DataAnnotations;

namespace Json.Schema.Generation.DataAnnotations;

/// <summary>
/// Adds a `format` keyword with `base64`.
/// </summary>
/// <remarks>
/// By default, `format` is an annotation only.  No validation will occur unless configured to do so.
/// 
/// The `base64` format is defined by the OpenAPI 3.1 specification.
/// </remarks>
public class Base64StringAttributeAttributeHandler : FormatAttributeHandler<Base64StringAttribute>
{
	/// <summary>
	/// Creates a new <see cref="Base64StringAttributeAttributeHandler"/>.
	/// </summary>
	public Base64StringAttributeAttributeHandler() : base("base64")
	{
	}
}

#endif