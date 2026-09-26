using System;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Places a controller's endpoints in named OpenAPI descriptions rather than the default one.
/// </summary>
/// <remarks>
/// <para>
/// A controller without this attribute appears in the default description — the one
/// configured by the <c>AddOpenApi</c> overload that takes no name.  Applying the attribute
/// moves the controller out of the default description and into the ones it names, so
/// applying it to one controller does not change where any other controller appears.
/// </para>
/// <para>
/// Minimal APIs are always described in the default description.  Splitting them is not
/// supported.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [OpenApiDocument("admin")]
/// [ApiController]
/// [Route("api/admin")]
/// public class AdminController : ControllerBase;
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class)]
public sealed class OpenApiDocumentAttribute : Attribute
{
	/// <summary>
	/// Gets the names of the descriptions the controller appears in.
	/// </summary>
	public string[] Names { get; }

	/// <summary>
	/// Creates a new <see cref="OpenApiDocumentAttribute"/>.
	/// </summary>
	/// <param name="names">
	/// The names of the descriptions the controller appears in.  A controller that belongs to
	/// more than one audience can name several.
	/// </param>
	public OpenApiDocumentAttribute(params string[] names)
	{
		Names = names;
	}
}
