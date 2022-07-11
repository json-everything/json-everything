namespace Json.Schema.OpenApi;

public static class Formats
{
	/// <summary>
	/// Defines a custom format used by OpenAPI v3.1.
	/// </summary>
	/// <remarks>
	/// Little is defined for this format except that it is intended to be a media
	/// type in line with RFC-7231 https://datatracker.ietf.org/doc/html/rfc7231#appendix-D.
	/// For more information, see https://github.com/OAI/OpenAPI-Specification/issues/2714.
	/// </remarks>
	public static readonly Format MediaRange = new UnknownFormat("media-range");

	static Formats()
	{
		Schema.Formats.Register(MediaRange);
	}
}